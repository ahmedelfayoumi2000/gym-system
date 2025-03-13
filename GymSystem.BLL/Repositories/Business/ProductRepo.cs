using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Dtos.Product;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class ProductRepo : IProductRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductRepo> _logger;

        public ProductRepo(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        //public async Task<ApiResponse> CreateAsync(ProductCreateDto productCreateDto)
        //{
        //    try
        //    {

        //        var spec = new BaseSpecification<Product>(p => p.Name == productCreateDto.Name && !p.IsDeleted);
        //        var existingProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);
        //        if (existingProduct != null)
        //        {
        //            return new ApiResponse(409, $"Product '{productCreateDto.Name}' already exists.");
        //        }

        //        var product = _mapper.Map<Product>(productCreateDto);
        //        await _unitOfWork.Repository<Product>().Add(product);
        //        await _unitOfWork.Complete();

        //        var createdDto = _mapper.Map<ProductViewDto>(product);
        //        return new ApiResponse(201, "Product created successfully", createdDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiExceptionResponse(500, "An error occurred while creating the product", ex.Message);
        //    }
        //}

        public async Task<ApiResponse> CreateAsync(ProductCreateDto productCreateDto)
        {
            // Validation
            if (productCreateDto == null)
            {
                _logger.LogWarning("Attempted to create a product with null ProductCreateDto.");
                return new ApiResponse(400, "Product data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(productCreateDto.Name))
            {
                _logger.LogWarning("Attempted to create a product with invalid name.");
                return new ApiResponse(400, "Product name cannot be empty.");
            }

            if (productCreateDto.Price <= 0)
            {
                _logger.LogWarning("Attempted to create a product with invalid price: {Price}", productCreateDto.Price);
                return new ApiResponse(400, "Product price must be greater than zero.");
            }

            if (productCreateDto.Count < 0)
            {
                _logger.LogWarning("Attempted to create a product with invalid count: {Count}", productCreateDto.Count);
                return new ApiResponse(400, "Product count cannot be negative.");
            }

            _logger.LogInformation("Creating product with name: {ProductName}", productCreateDto.Name);

            try
            {
                // Check for existing product
                var spec = new BaseSpecification<Product>(p => p.Name == productCreateDto.Name && !p.IsDeleted);
                var existingProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);
                if (existingProduct != null)
                {
                    _logger.LogWarning("Product with name {ProductName} already exists.", productCreateDto.Name);
                    return new ApiResponse(409, $"Product '{productCreateDto.Name}' already exists.");
                }

                // Map DTO to Entity
                var product = _mapper.Map<Product>(productCreateDto);

                // Set default values for required fields
                product.IsActive = true;
                product.IsDeleted = false;

                _logger.LogDebug("Product entity after mapping: {@Product}", product);

                // Add to repository
                await _unitOfWork.Repository<Product>().Add(product);

                // Save changes
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save product with name {ProductName} to the database.", productCreateDto.Name);
                    return new ApiResponse(500, "Failed to save the product to the database.");
                }

                // Map back to DTO
                var createdDto = _mapper.Map<ProductViewDto>(product);
                _logger.LogInformation("Product {ProductName} created successfully with ID: {ProductId}", product.Name, product.Id);
                return new ApiResponse(201, "Product created successfully", createdDto);
            }
            catch (DbUpdateException dbEx)
            {
                // Log the detailed database error
                _logger.LogError(dbEx, "Database error occurred while creating product {ProductName}. Inner Exception: {InnerException}",
                    productCreateDto.Name, dbEx.InnerException?.Message);
                return new ApiExceptionResponse(500, "A database error occurred while creating the product.", dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                // Log the general error
                _logger.LogError(ex, "An unexpected error occurred while creating product {ProductName}.", productCreateDto.Name);
                return new ApiExceptionResponse(500, "An unexpected error occurred while creating the product.", ex.Message);
            }
        }
    


        public async Task<IEnumerable<ProductViewDto>> GetAllAsync()
        {
            try
            {

                var spec = new BaseSpecification<Product>(p => !p.IsDeleted);
                var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
                var productDtos = _mapper.Map<IEnumerable<ProductViewDto>>(products);

                return productDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve products: {ex.Message}", ex);
            }
        }


        public async Task<ApiResponse> UpdateAsync(int productId, ProductCreateDto productCreateDto)
        {
            try
            {

                var spec = new BaseSpecification<Product>(p => p.Id == productId && !p.IsDeleted);
                var existingProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);
                if (existingProduct == null)
                {
                    return new ApiResponse(404, $"Product with ID {productId} not found.");
                }

                _mapper.Map(productCreateDto, existingProduct);
                _unitOfWork.Repository<Product>().Update(existingProduct);

                await _unitOfWork.Complete();

                var updatedDto = _mapper.Map<ProductViewDto>(existingProduct);
                return new ApiResponse(200, "Product updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while updating the product", ex.Message);
            }
        }


    }

}