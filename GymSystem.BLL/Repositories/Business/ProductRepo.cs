using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Product;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
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
            IMapper mapper)
            //ILogger<ProductRepo> logger
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // دالة إضافة منتج جديد
        // : الفرونت بيبعتلي بيانات منتج، بشوف لو موجود قبل كدا، لو لأ بضيفه وأرجع الـ DTO
        public async Task<ApiResponse> CreateAsync(ProductCreateDto productCreateDto)
        {
            if (productCreateDto == null)
            {
                return new ApiResponse(400, "Product data cannot be null.");
            }

            try
            {

                var spec = new BaseSpecification<Product>(p => p.Name == productCreateDto.Name && !p.IsDeleted);
                var existingProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);
                if (existingProduct != null)
                {
                    return new ApiResponse(409, $"Product '{productCreateDto.Name}' already exists.");
                }

                var product = _mapper.Map<Product>(productCreateDto);
                await _unitOfWork.Repository<Product>().Add(product);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save the product to the database.");
                }

                var createdDto = _mapper.Map<ProductViewDto>(product);
                return new ApiResponse(201, "Product created successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حصل مشكلة وأنا بضيف المنتج {Name}", productCreateDto.Name);
                return new ApiExceptionResponse(500, "An error occurred while creating the product", ex.Message);
            }
        }

        // دالة جلب كل المنتجات
        //  الفرونت عايز كل المنتجات، بجيبهاله كلها مع التفاصيل
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

        // دالة تعديل منتج
        //  الفرونت بيبعتلي بيانات منتج معدل، بظبطه وأرجع الـ DTO الجديد
        public async Task<ApiResponse> UpdateAsync(int productId, ProductCreateDto productCreateDto)
        {
            if (productId <= 0)
            {
                return new ApiResponse(400, "Product ID must be a positive integer.");
            }

            if (productCreateDto == null)
            {
                return new ApiResponse(400, "Product data cannot be null.");
            }

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

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to update the product in the database.");
                }

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