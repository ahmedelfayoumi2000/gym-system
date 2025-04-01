using GymSystem.BLL.Dtos.Role;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.DAL.Entities.Enums.Auth;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymSystem.API.Controllers;

namespace GymMangamentSystem.Apis.Controllers
{

    [Authorize(Roles = "Admin")]
    public class RoleController : BaseApiController
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RoleController> _logger;


        public RoleController(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ILogger<RoleController> logger)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetRoles()
        {
            try
            {
                _logger.LogInformation("Fetching all roles");

                var roles = await _roleManager.Roles.ToListAsync();
                var roleDtos = roles.Select(r => new RoleDTO
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                _logger.LogInformation("Successfully retrieved {RoleCount} roles", roleDtos.Count);
                return Ok(new ApiResponse(200, "Roles retrieved successfully", roleDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateRole([FromBody] RoleFormDTO model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateRole with Name: {RoleName}", model?.Name);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            try
            {
                _logger.LogInformation("Creating new role with Name: {RoleName}", model.Name);

                if (await _roleManager.RoleExistsAsync(model.Name))
                {
                    _logger.LogWarning("Role with Name {RoleName} already exists", model.Name);
                    return Conflict(new ApiResponse(409, $"Role '{model.Name}' already exists"));
                }

                var role = new IdentityRole(model.Name.Trim());
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create role with Name {RoleName}: {Errors}", model.Name, errors);
                    return BadRequest(new ApiResponse(400, $"Failed to create role: {errors}"));
                }

                var roleDto = new RoleDTO { Id = role.Id, Name = role.Name };
                _logger.LogInformation("Role created successfully with ID: {RoleId}", role.Id);
                return StatusCode(StatusCodes.Status201Created, new ApiResponse(201, "Role created successfully", roleDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role with Name: {RoleName}", model.Name);
                return HandleException(ex);
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("DeleteRole called with null or empty ID");
                return BadRequest(new ApiResponse(400, "Role ID is required"));
            }

            try
            {
                _logger.LogInformation("Deleting role with ID: {RoleId}", id);

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound(new ApiResponse(404, $"Role with ID {id} not found"));
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete role with ID {RoleId}: {Errors}", id, errors);
                    return BadRequest(new ApiResponse(400, $"Failed to delete role: {errors}"));
                }

                _logger.LogInformation("Role deleted successfully with ID: {RoleId}", id);
                return Ok(new ApiResponse(200, "Role deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role with ID: {RoleId}", id);
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpdateRole(string id, [FromBody] RoleDTO model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateRole with ID: {RoleId}", id);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            if (string.IsNullOrEmpty(id) || id != model.Id)
            {
                _logger.LogWarning("Mismatch between route ID {RouteId} and model ID {ModelId}", id, model.Id);
                return BadRequest(new ApiResponse(400, "Role ID in route and model must match"));
            }

            try
            {
                _logger.LogInformation("Updating role with ID: {RoleId}", id);

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound(new ApiResponse(404, $"Role with ID {id} not found"));
                }

                if (await _roleManager.RoleExistsAsync(model.Name) && role.Name != model.Name)
                {
                    _logger.LogWarning("Role with Name {RoleName} already exists", model.Name);
                    return Conflict(new ApiResponse(409, $"Role '{model.Name}' already exists"));
                }

                role.Name = model.Name.Trim();
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to update role with ID {RoleId}: {Errors}", id, errors);
                    return BadRequest(new ApiResponse(400, $"Failed to update role: {errors}"));
                }

                var updatedRoleDto = new RoleDTO { Id = role.Id, Name = role.Name };
                _logger.LogInformation("Role updated successfully with ID: {RoleId}", id);
                return Ok(new ApiResponse(200, "Role updated successfully", updatedRoleDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role with ID: {RoleId}", id);
                return HandleException(ex);
            }
        }

        private ActionResult<ApiResponse> HandleException(Exception ex)
        {
            return StatusCode(500, new ApiExceptionResponse(500, "An unexpected error occurred", ex.Message));
        }


    }
}