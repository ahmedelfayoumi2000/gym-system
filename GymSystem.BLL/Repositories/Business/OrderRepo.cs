using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.OrderSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ApiResponse> CreateAsync(OrderCreateDto orderCreateDto, string currentUserId)
        {
            if (orderCreateDto == null)
            {
                return new ApiResponse(400, "Order data cannot be null.");
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                return new ApiResponse(401, "User authentication required.");
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var productSpec = new ProductByIdAndActiveSpecification(orderCreateDto.ProductId);
                    var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(productSpec);
                    if (product == null)
                    {
                        return new ApiResponse(404, $"Product with ID {orderCreateDto.ProductId} not found or not available.");
                    }
                    if (!product.IsActive)
                    {
                        return new ApiResponse(404, $"Product with ID {orderCreateDto.ProductId} is out of stock.");
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
                    if (product.Count == 0)
                    {
                        product.IsActive = false;
                    }
                    _unitOfWork.Repository<Product>().Update(product);

                    await RecordFinancialTransaction(order, TransactionType.Payment, currentUserId);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        transactionScope.Dispose();
                        return new ApiResponse(500, "Failed to save the order and update product stock in the database.");
                    }

                    transactionScope.Complete();
                    var createdDto = _mapper.Map<OrderViewDto>(order);
                    createdDto.ProductName = product.Name;
                    createdDto.ProductPrice = product.Price;
                    createdDto.CreatedByUserName = user.DisplayName;

                    return new ApiResponse(201, "Order created successfully and product stock updated", createdDto);
                }
                catch (Exception ex)
                {
                    return new ApiExceptionResponse(500, "An error occurred while creating the order and updating product stock", ex.Message);
                }
            }
        }

        public async Task<IEnumerable<OrderViewDto>> GetAllAsync()
        {
            try
            {
                var spec = new AllOrdersSpecification();
                var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
                var orderDtos = _mapper.Map<IEnumerable<OrderViewDto>>(orders);

                foreach (var dto in orderDtos)
                {
                    var order = orders.First(o => o.Id == dto.Id);
                    dto.ProductName = order.Product?.Name;
                    dto.CreatedByUserName = order.CreatedByUser?.DisplayName;
                }

                return orderDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve orders: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> UpdateAsync(int orderId, OrderCreateDto orderCreateDto, string currentUserId)
        {
            if (orderId <= 0)
            {
                return new ApiResponse(400, "Order ID must be a positive integer.");
            }

            if (orderCreateDto == null)
            {
                return new ApiResponse(400, "Order data cannot be null.");
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                return new ApiResponse(401, "User authentication required.");
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var spec = new OrderByIdSpecification(orderId);
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

                    // Check if the product still exists and is available
                    var productSpec = new ProductByIdAndActiveSpecification(existingOrder.ProductId);
                    var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(productSpec);

                    if (product == null || !product.IsActive)
                    {
                        return new ApiResponse(404, $"Product with ID {existingOrder.ProductId} not found or not available.");
                    }

                    var spectransaction = new FinancialTransactionByOrderSpecification(
                        existingOrder.Id,
                        existingOrder.ProductName,
                        existingOrder.CreatedAt,
                        existingOrder.Total
                    );
                    var existingTransaction = await _unitOfWork.Repository<FinancialTransaction>().GetEntityWithSpecAsync(spectransaction);

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

                    await RecordFinancialTransaction(existingOrder, TransactionType.Payment, currentUserId, existingTransaction);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        return new ApiResponse(500, "Failed to update the order in the database.");
                    }

                    var updatedDto = _mapper.Map<OrderViewDto>(existingOrder);
                    updatedDto.ProductName = product.Name;
                    updatedDto.IsAvailable = product.IsActive;
                    updatedDto.CreatedByUserName = user.DisplayName;

                    return new ApiResponse(200, "Order updated successfully", updatedDto);
                }
                catch (Exception ex)
                {
                    return new ApiExceptionResponse(500, "An error occurred while updating the order", ex.Message);
                }
            }
        }

        #region Private Helper Methods

        private async Task RecordFinancialTransaction(Order order, TransactionType transactionType, string userId, FinancialTransaction existingTransaction = null)
        {
            if (existingTransaction != null)
            {
                existingTransaction.TransactionType = transactionType;
                existingTransaction.Amount = order.Total;
                existingTransaction.TransactionDate = order.CreatedAt;
                existingTransaction.Description = $"Order payment for Order ID: {order.Id}, Product: {order.ProductName}";
                existingTransaction.CreatedByUserId = userId;
                existingTransaction.CreatedAt = DateTime.UtcNow;
                existingTransaction.IsDeleted = false;

                _unitOfWork.Repository<FinancialTransaction>().Update(existingTransaction);
            }
            else
            {
                var transaction = new FinancialTransaction
                {
                    TransactionType = transactionType,
                    Amount = order.Total,
                    TransactionDate = order.CreatedAt,
                    Description = $"Order payment for Order ID: {order.Id}, Product: {order.ProductName}",
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
            }
        }

        #endregion
    }
}