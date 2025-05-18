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

                var roles = await _roleManager.Roles.ToListAsync();
                var roleDtos = roles.Select(r => new RoleDTO
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                return Ok(new ApiResponse(200, "Roles retrieved successfully", roleDtos));
            }
            catch (Exception ex)
            {
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
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            try
            {

                if (await _roleManager.RoleExistsAsync(model.Name))
                {
                    return Conflict(new ApiResponse(409, $"Role '{model.Name}' already exists"));
                }

                var role = new IdentityRole(model.Name.Trim());
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to create role: {errors}"));
                }

                var roleDto = new RoleDTO { Id = role.Id, Name = role.Name };
                return StatusCode(StatusCodes.Status201Created, new ApiResponse(201, "Role created successfully", roleDto));
            }
            catch (Exception ex)
            {
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
                return BadRequest(new ApiResponse(400, "Role ID is required"));
            }

            try
            {

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
                    return BadRequest(new ApiResponse(400, $"Failed to delete role: {errors}"));
                }

                return Ok(new ApiResponse(200, "Role deleted successfully"));
            }
            catch (Exception ex)
            {
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

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new ApiResponse(404, $"Role with ID {id} not found"));
                }

                if (await _roleManager.RoleExistsAsync(model.Name) && role.Name != model.Name)
                {
                    return Conflict(new ApiResponse(409, $"Role '{model.Name}' already exists"));
                }

                role.Name = model.Name.Trim();
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to update role: {errors}"));
                }

                var updatedRoleDto = new RoleDTO { Id = role.Id, Name = role.Name };
                return Ok(new ApiResponse(200, "Role updated successfully", updatedRoleDto));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        private ActionResult<ApiResponse> HandleException(Exception ex)
        {
            return StatusCode(500, new ApiExceptionResponse(500, "An unexpected error occurred", ex.Message));
        }


    }
}