using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.ExerciseCategorySpec;
using GymSystem.DAL.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories
{
    public class ExerciseCategoryRepo : IExerciseCategoryRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ExerciseCategoryRepo(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        public async Task<ApiResponse> AddExerciseCategory(ExerciseCategoryDto exerciseCategoryDto)
        {
            try
            {
                if (exerciseCategoryDto == null)
                {
                    return new ApiResponse(400, "Exercise category data cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(exerciseCategoryDto.CategoryName))
                {
                    return new ApiResponse(400, "Category name is required.");
                }

                var spec = new ExerciseCategoryByNameSpecification(exerciseCategoryDto.CategoryName);
                var existingCategory = await _unitOfWork.Repository<ExerciseCategory>().GetEntityWithSpecAsync(spec);
                if (existingCategory != null)
                {
                    return new ApiResponse(409, $"Exercise category with name '{exerciseCategoryDto.CategoryName}' already exists.");
                }

                var category = _mapper.Map<ExerciseCategory>(exerciseCategoryDto);


                if (exerciseCategoryDto.Image != null)
                {
                    var uploadResult = await _imageService.UploadImageAsync(exerciseCategoryDto.Image);
                    if (uploadResult.Item1 == 1)
                    {
                        category.ImageUrl = uploadResult.Item2;
                    }
                    else
                    {
                        return new ApiResponse(400, $"Failed to Upload Image: {uploadResult.Item2}"); // رسالة الخطأ
                    }
                }

                category.IsDeleted = false;
                _unitOfWork.Repository<ExerciseCategory>().Add(category);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to add the exercise category to the database.");
                }

                return new ApiResponse(201, "Exercise category created successfully", _mapper.Map<ExerciseCategoryDto>(category));
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An unexpected error occurred while adding the exercise category.", ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteExerciseCategory(int id)
        {
            try
            {
                var category = await _unitOfWork.Repository<ExerciseCategory>().GetByIdAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return new ApiResponse(404, $"Exercise category with ID {id} not found.");
                }

                category.IsDeleted = true;
                _unitOfWork.Repository<ExerciseCategory>().Update(category);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete the exercise category.");
                }

                return new ApiResponse(200, "Exercise category deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An unexpected error occurred while deleting the exercise category.", ex.Message);
            }
        }

        public async Task<ExerciseCategoryDto> GetExerciseCategory(int id)
        {
            try
            {
                var spec = new ExerciseCategoryByIdSpecification(id);
                var category = await _unitOfWork.Repository<ExerciseCategory>().GetEntityWithSpecAsync(spec);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Exercise category with ID {id} not found.");
                }

                return _mapper.Map<ExerciseCategoryDto>(category);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the exercise category.", ex);
            }
        }

        public async Task<IEnumerable<ExerciseCategoryDto>> GetExerciseCategories()
        {
            try
            {
                var spec = new AllExerciseCategoriesSpecification();
                var categories = await _unitOfWork.Repository<ExerciseCategory>().GetAllWithSpecAsync(spec);
                return _mapper.Map<IEnumerable<ExerciseCategoryDto>>(categories);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving exercise categories.", ex);
            }
        }

        public async Task<ApiResponse> UpdateExerciseCategory(int id, ExerciseCategoryDto exerciseCategoryDto)
        {
            try
            {
                if (exerciseCategoryDto == null)
                {
                    return new ApiResponse(400, "Exercise category data cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(exerciseCategoryDto.CategoryName))
                {
                    return new ApiResponse(400, "Category name is required.");
                }

                var spec = new ExerciseCategoryByIdSpecification(id);
                var category = await _unitOfWork.Repository<ExerciseCategory>().GetEntityWithSpecAsync(spec);
                if (category == null)
                {
                    return new ApiResponse(404, $"Exercise category with ID {id} not found.");
                }

                var existingSpec = new ExerciseCategoryByNameSpecification(exerciseCategoryDto.CategoryName, id);
                var existingCategory = await _unitOfWork.Repository<ExerciseCategory>().GetEntityWithSpecAsync(existingSpec);
                if (existingCategory != null)
                {
                    return new ApiResponse(409, $"Exercise category with name '{exerciseCategoryDto.CategoryName}' already exists.");
                }

                // تحديث الحقول
                category.CategoryName = exerciseCategoryDto.CategoryName;


                if (exerciseCategoryDto.Image != null)
                {

                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        await _imageService.DeleteImageAsync(category.ImageUrl);
                    }
                    var uploadResult = await _imageService.UploadImageAsync(exerciseCategoryDto.Image);
                    if (uploadResult.Item1 == 1)
                    {
                        category.ImageUrl = uploadResult.Item2;
                    }
                    else
                    {
                        return new ApiResponse(400, $"Failed to Upload Image: {uploadResult.Item2}"); 
                    }
                }

                _unitOfWork.Repository<ExerciseCategory>().Update(category);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to update the exercise category.");
                }

                return new ApiResponse(200, "Exercise category updated successfully", _mapper.Map<ExerciseCategoryDto>(category));
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An unexpected error occurred while updating the exercise category.", ex.Message);
            }
        }
    }
}