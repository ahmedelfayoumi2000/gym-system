// BLL/Repositories/Business/OrderRepo.cs
using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
                _logger.LogWarning("Attempted to create a null OrderDto.");
                return new ApiResponse(400, "Order data cannot be null.");
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Current user ID is missing or invalid.");
                return new ApiResponse(401, "User authentication required.");
            }

            try
            {

                var productSpec = new BaseSpecification<Product>(p => p.Id == orderCreateDto.ProductId && !p.IsDeleted && p.IsAvailable);
                var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(productSpec);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found or not available.", orderCreateDto.ProductId);
                    return new ApiResponse(404, $"Product with ID {orderCreateDto.ProductId} not found or not available.");
                }

                if (product.Count < orderCreateDto.Count)
                {
                    _logger.LogWarning("Insufficient stock for Product ID {ProductId}. Available: {Available}, Requested: {Requested}",
                        orderCreateDto.ProductId, product.Count, orderCreateDto.Count);
                    return new ApiResponse(400, $"Insufficient stock for Product ID {orderCreateDto.ProductId}. Only {product.Count} available.");
                }

                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", currentUserId);
                    return new ApiResponse(404, "User not found.");
                }

                var order = _mapper.Map<Order>(orderCreateDto);
                order.CreatedByUserId = currentUserId;
                order.CreatedAt = DateTime.UtcNow;
                order.Price = product.Price; 

                await _unitOfWork.Repository<Order>().Add(order);

                product.Count -= orderCreateDto.Count;
                if (product.Count == 0)
                {
                    product.IsAvailable = false; 
                }
                _unitOfWork.Repository<Product>().Update(product);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save the order and update product stock in the database.");
                }

                var createdDto = _mapper.Map<OrderViewDto>(order);
                createdDto.ProductName = product.Name;
                createdDto.CreatedByUserName = user.DisplayName;

                return new ApiResponse(201, "Order created successfully and product stock updated", createdDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while creating the order and updating product stock", ex.Message);
            }
        }

      
        public async Task<IEnumerable<OrderViewDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all orders.");

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
                    dto.CreatedByUserName = order.CreatedByUser?.DisplayName;
                }

                _logger.LogInformation("Retrieved {Count} orders successfully.", orderDtos.Count());
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders.");
                throw new ApplicationException($"Failed to retrieve orders: {ex.Message}", ex);
            }
        }

       
        public async Task<ApiResponse> UpdateAsync(int orderId, OrderCreateDto orderCreateDto, string currentUserId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("Invalid order ID: {OrderId}", orderId);
                return new ApiResponse(400, "Order ID must be a positive integer.");
            }

            if (orderCreateDto == null)
            {
                _logger.LogWarning("Attempted to update order with ID {OrderId} using null OrderDto.", orderId);
                return new ApiResponse(400, "Order data cannot be null.");
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Current user ID is missing or invalid for order update.");
                return new ApiResponse(401, "User authentication required.");
            }

            try
            {
                _logger.LogInformation("Attempting to update order with ID: {OrderId}", orderId);

                var spec = new BaseSpecification<Order>(o => o.Id == orderId && !o.IsDeleted);
                var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
                if (existingOrder == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                    return new ApiResponse(404, $"Order with ID {orderId} not found.");
                }

                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for order update.", currentUserId);
                    return new ApiResponse(404, "User not found.");
                }

                // Check if the product still exists and is available
                var productSpec = new BaseSpecification<Product>(p => p.Id == existingOrder.ProductId && !p.IsDeleted && p.IsAvailable);
                var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(productSpec);
                if (product == null || !product.IsAvailable)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found or not available for order update.", existingOrder.ProductId);
                    return new ApiResponse(404, $"Product with ID {existingOrder.ProductId} not found or not available.");
                }

                _mapper.Map(orderCreateDto, existingOrder);
                existingOrder.CreatedByUserId = currentUserId; // Update the user who made the modification
                existingOrder.CreatedAt = DateTime.UtcNow; // Update modification time
                existingOrder.Price = product.Price; // Ensure price matches the current product price

                _unitOfWork.Repository<Order>().Update(existingOrder);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to update order with ID: {OrderId}", orderId);
                    return new ApiResponse(500, "Failed to update the order in the database.");
                }

                var updatedDto = _mapper.Map<OrderViewDto>(existingOrder);
                updatedDto.ProductName = product.Name;
                updatedDto.CreatedByUserName = user.DisplayName;

                _logger.LogInformation("Order with ID {OrderId} updated successfully.", orderId);
                return new ApiResponse(200, "Order updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order with ID: {OrderId}", orderId);
                return new ApiExceptionResponse(500, "An error occurred while updating the order", ex.Message);
            }
        }
    }
}