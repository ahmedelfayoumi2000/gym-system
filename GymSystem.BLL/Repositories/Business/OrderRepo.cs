using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace GymSystem.BLL.Repositories.Business
{
    public class OrderRepo : IOrderRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderRepo> _logger;

        public OrderRepo(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<OrderRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CRUD Operations
        public async Task<ApiResponse> CreateAsync(OrderCreateDto orderCreateDto, string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("User authentication required for creating order.");
                return new ApiResponse(401, "User authentication required.");
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var productSpec = new BaseSpecification<Product>(p => p.Id == orderCreateDto.ProductId && !p.IsDeleted);
                    var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(productSpec);
                    if (product == null)
                    {
                        return new ApiResponse(404, $"Product with ID {orderCreateDto.ProductId} not found.");
                    }

                    if (!product.IsActive)
                    {
                        return new ApiResponse(400, $"Product with ID {orderCreateDto.ProductId} is out of stock.");
                    }

                    if (product.Count < orderCreateDto.Count)
                    {
                        return new ApiResponse(400, $"Insufficient stock for Product ID {orderCreateDto.ProductId}. Only {product.Count} available.");
                    }

                    var user = await _userManager.FindByIdAsync(currentUserId);
                    if (user == null)
                    {
                        return new ApiResponse(404, "User not found.");
                    }

                    var order = _mapper.Map<Order>(orderCreateDto);
                    order.CreatedByUserId = currentUserId;
                    order.ProductName = product.Name;
                    order.Total = product.Price * orderCreateDto.Count;

                    await _unitOfWork.Repository<Order>().Add(order);

                    product.Count -= orderCreateDto.Count;
                    product.IsActive = product.Count > 0;
                    _unitOfWork.Repository<Product>().Update(product);

                    // Record financial transaction for the order as a Payment
                    await RecordFinancialTransaction(order, TransactionType.Payment, currentUserId);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        _logger.LogError("Failed to save order and update product stock for Product ID: {ProductId}", orderCreateDto.ProductId);
                        return new ApiResponse(500, "Failed to save the order and update product stock.");
                    }

                    transactionScope.Complete();
                    var createdDto = _mapper.Map<OrderViewDto>(order);
                    createdDto.ProductName = product.Name;
                    createdDto.ProductPrice = product.Price;
                    createdDto.CreatedByUserName = user.DisplayName;

                    _logger.LogInformation("Order created successfully for Product ID: {ProductId} with Order ID: {OrderId}", orderCreateDto.ProductId, order.Id);
                    return new ApiResponse(201, "Order created successfully and product stock updated", createdDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order for Product ID: {ProductId}", orderCreateDto.ProductId);
                    return new ApiExceptionResponse(500, "An error occurred while creating the order.", ex.Message);
                }
            }
        }

     
        public async Task<ApiResponse> UpdateAsync(int orderId, OrderCreateDto orderCreateDto, string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new ApiResponse(401, "User authentication required.");
            }


            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var spec = new BaseSpecification<Order>(o => o.Id == orderId && !o.IsDeleted);
                    var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
                    if (existingOrder == null)
                    {
                        return new ApiResponse(404, $"Order with ID {orderId} not found.");
                    }

                    var user = await _userManager.FindByIdAsync(currentUserId);
                    if (user == null)
                    {
                        return new ApiResponse(404, "User not found.");
                    }

                    var productSpec = new BaseSpecification<Product>(p => p.Id == orderCreateDto.ProductId && !p.IsDeleted);
                    var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(productSpec);
                    if (product == null)
                    {
                        return new ApiResponse(404, $"Product with ID {orderCreateDto.ProductId} not found.");
                    }

                    if (!product.IsActive)
                    {
                        return new ApiResponse(400, $"Product with ID {orderCreateDto.ProductId} is out of stock.");
                    }

                    product.Count += existingOrder.Count;
                    if (product.Count < orderCreateDto.Count)
                    {
                        return new ApiResponse(400, $"Insufficient stock for Product ID {orderCreateDto.ProductId}. Only {product.Count} available.");
                    }

                    product.Count -= orderCreateDto.Count;
                    product.IsActive = product.Count > 0;
                    _unitOfWork.Repository<Product>().Update(product);

                    _mapper.Map(orderCreateDto, existingOrder);
                    existingOrder.CreatedByUserId = currentUserId;
                    existingOrder.CreatedAt = DateTime.UtcNow;
                    existingOrder.Total = product.Price * orderCreateDto.Count;

                    _unitOfWork.Repository<Order>().Update(existingOrder);

                    // Record financial transaction for the updated order as a Payment
                    await RecordFinancialTransaction(existingOrder, TransactionType.Payment, currentUserId);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        return new ApiResponse(500, "Failed to update the order and product stock.");
                    }

                    transactionScope.Complete();
                    var updatedDto = _mapper.Map<OrderViewDto>(existingOrder);
                    updatedDto.ProductName = product.Name;
                    updatedDto.ProductPrice = product.Price;
                    updatedDto.CreatedByUserName = user.DisplayName;

                    return new ApiResponse(200, "Order updated successfully", updatedDto);
                }
                catch (Exception ex)
                {
                    return new ApiExceptionResponse(500, "An error occurred while updating the order.", ex.Message);
                }
            }
        }

        #endregion

        #region Retrieval Methods

        /// <summary>
        /// Retrieves all orders with related product and user details.
        /// </summary>
        public async Task<IEnumerable<OrderViewDto>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all orders.");

            try
            {
                var spec = new BaseSpecification<Order>(o => !o.IsDeleted)
                {
                    Includes = new List<Expression<Func<Order, object>>>
                    {
                        o => o.Product,
                        o => o.CreatedByUser
                    }
                };
                var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
                var orderDtos = _mapper.Map<IEnumerable<OrderViewDto>>(orders);

                foreach (var dto in orderDtos)
                {
                    var order = orders.First(o => o.Id == dto.Id);
                    dto.ProductName = order.Product?.Name;
                    dto.ProductPrice = order.Product.Price;
                    dto.CreatedByUserName = order.CreatedByUser?.DisplayName;
                }

                _logger.LogInformation("Retrieved {Count} orders successfully.", orderDtos.Count());
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders.");
                throw new ApplicationException("Failed to retrieve orders.", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task RecordFinancialTransaction(Order order, TransactionType transactionType, string userId)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType, 
                Amount = order.Total,
                TransactionDate = DateTime.UtcNow,
                Description = $"Order payment for Order ID: {order.Id}, Product: {order.ProductName}",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
        }

        #endregion
    }
}