using AutoMapper;
using GymSystem.API.Controllers;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Role;
using GymSystem.BLL.Errors;
using GymSystem.DAL.Entities.Enums.Auth;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GymSystem.BLL.Dtos.User;

namespace GymMangamentSystem.Apis.Controllers
{

    public class UserController : BaseApiController

    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ILogger<UserController> logger,
            IMapper mapper)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("trainers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetTrainers()
        {
            try
            {
                var trainers = await _userManager.GetUsersInRoleAsync("Trainer");
                var trainerDtos = await MapUsersToDtos(trainers);

                return Ok(new ApiResponse(200, "Trainers retrieved successfully", trainerDtos));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("members")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetMembers()
        {
            try
            {
                var members = await _userManager.GetUsersInRoleAsync("Member");
                var memberDtos = await MapUsersToDtos(members);

                return Ok(new ApiResponse(200, "Members retrieved successfully", memberDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving members.");
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("receptionists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetReceptionists()
        {
            try
            {
                var receptionists = await _userManager.GetUsersInRoleAsync("Receptionist");
                var receptionistDtos = await MapUsersToDtos(receptionists);

                return Ok(new ApiResponse(200, "Receptionists retrieved successfully", receptionistDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving receptionists.");
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "User ID is required and must not be empty." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return NotFound(new ApiResponse(404, $"User with ID {id} not found"));
                }


                var userDto = await MapUserToDto(user);

                var roles = await _userManager.GetRolesAsync(user);
                userDto = new UserDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    Age = user.Age,
                    Roles = roles.ToList(),
                    UserCode = user.UserCode
                };

                return Ok(new ApiResponse(200, "User retrieved successfully", userDto));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/roles/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpdateUserRoles(string id, [FromBody] UserRoleDTO model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return BadRequest(CreateValidationErrorResponse("Invalid user role data"));
            }

            if (id != model.UserId)
            {
                _logger.LogWarning("Mismatch between route ID {RouteId} and model UserId {ModelUserId}", id, model.UserId);
                return BadRequest(new ApiResponse(400, "User ID in route and model must match"));
            }

            try
            {

                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, $"User with ID {id} not found"));
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var requestedRoles = model.Roles?.Where(r => r.IsSelected).Select(r => r.Name).ToList() ?? new List<string>();

                foreach (var role in requestedRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        return BadRequest(new ApiResponse(400, $"Role '{role}' does not exist in the system."));
                    }
                }

                var rolesToAdd = requestedRoles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(requestedRoles).ToList();

                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        return BadRequest(new ApiResponse(400, $"Failed to add roles: {errors}"));
                    }
                }

                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        return BadRequest(new ApiResponse(400, $"Failed to remove roles: {errors}"));
                    }
                }

                var updatedRoles = await _userManager.GetRolesAsync(user);

                return Ok(new ApiResponse(200, "User roles updated successfully", updatedRoles));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/roles/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AddUserRole(string id, [FromBody] AddUserRoleDTO model)
        {
            if (!ModelState.IsValid || model == null || string.IsNullOrEmpty(model.Role))
            {
                return BadRequest(CreateValidationErrorResponse("Invalid role data. Role cannot be empty."));
            }

            if (id != model.UserId)
            {
                return BadRequest(new ApiResponse(400, "User ID in route and model must match"));
            }

            try
            {

                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, $"User with ID {id} not found"));
                }

                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    return BadRequest(new ApiResponse(400, $"Role '{model.Role}' does not exist in the system."));
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains(model.Role))
                {
                    return BadRequest(new ApiResponse(400, $"User already has the role '{model.Role}'."));
                }

                var addResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to add role: {errors}"));
                }


                var updatedRoles = await _userManager.GetRolesAsync(user);

                return Ok(new ApiResponse(200, $"Role '{model.Role}' added successfully to user.", updatedRoles));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost("users")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AddUser([FromBody] Register model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return BadRequest(CreateValidationErrorResponse("Invalid user registration data"));
            }

            try
            {

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Conflict(new ApiResponse(409, $"User with email '{model.Email}' already exists"));
                }

                var user = new AppUser
                {
                    DisplayName = model.DisplayName,
                    Email = model.Email,
                    Gender = model.Gender,
                    UserName = model.Email.Split('@')[0],
                    UserRole = (int)model.UserRole,
                    EmailConfirmed = true // Admin-confirmed user creation
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to create user: {errors}"));
                }

                var roleName = GetRoleNameFromEnum(model.UserRole);
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user); 
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to assign role: {errors}"));
                }


                var userDto = await MapUserToDto(user);

                userDto = new UserDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Gender = user.Gender,
                    Roles = new List<string> { roleName }
                };

                return StatusCode(StatusCodes.Status201Created, new ApiResponse(201, "User created successfully", userDto));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid || userDto == null)
            {
                return BadRequest(CreateValidationErrorResponse("Invalid user update data"));
            }

            if (id != userDto.Id)
            {
                return BadRequest(new ApiResponse(400, "User ID in route and model must match"));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, $"User with ID {id} not found"));
                }

                user.DisplayName = userDto.DisplayName;
                user.Email = userDto.Email;
                user.UserName = userDto.UserName;
                user.PhoneNumber = userDto.PhoneNumber;
                user.Gender = userDto.Gender;


                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to update user: {errors}"));
                }

                var updatedUserDto = await MapUserToDto(user);
                return Ok(new ApiResponse(200, "User updated successfully", updatedUserDto));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "User ID is required and must not be empty." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, $"User with ID {id} not found"));
                }

                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to delete user: {errors}"));
                }

                return Ok(new ApiResponse(200, "User deleted successfully"));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Helper Methods

        private async Task<List<UserDto>> MapUsersToDtos(IList<AppUser> users)
        {
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    UserCode = user.UserCode
                });
            }
            return userDtos;
        }

        private async Task<UserDto> MapUserToDto(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                UserCode = user.UserCode
            };
        }

        private string GetRoleNameFromEnum(UserRoleEnum role)
        {
            return role switch
            {
                UserRoleEnum.Admin => "Admin",
                UserRoleEnum.Member => "Member",
                UserRoleEnum.Trainer => "Trainer",
                UserRoleEnum.Receptionist => "Receptionist",
                _ => throw new ArgumentException("Invalid user role", nameof(role))
            };
        }

        private ApiValidationErrorResponse CreateValidationErrorResponse(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(x => x.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }

        private ActionResult<ApiResponse> HandleException(Exception ex)
        {
            return StatusCode(500, new ApiExceptionResponse(500, "An unexpected error occurred", ex.Message));
        }
    }
}