using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{

    /// <summary>
    /// إضافة موظف وعرض الموظفين
    /// </summary>
    public class EmployeeController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEmployees([FromQuery] SpecPrams specParams)
        {
            try
            {
                var employees = _userManager.Users
                    .Where(u => u.UserRole == 1 || u.UserRole == 2 || u.UserRole == 3) // Admin, Trainer, Receptionist
                    .Where(u => !u.IsDeleted);

                if (!string.IsNullOrEmpty(specParams.Search))
                {
                    employees = employees.Where(u => u.DisplayName.ToLower().Contains(specParams.Search.ToLower()));
                }

                if (!string.IsNullOrEmpty(specParams.Sort))
                {
                    employees = specParams.Sort.ToLower() switch
                    {
                        "name" => employees.OrderBy(u => u.DisplayName),
                        "namedesc" => employees.OrderByDescending(u => u.DisplayName),
                        _ => employees.OrderBy(u => u.Id)
                    };
                }

                var totalItems = employees.Count();
                if (specParams.PageSize > 0)
                {
                    employees = employees.Skip((specParams.PageIndex - 1) * specParams.PageSize).Take(specParams.PageSize);
                }

                var employeeList = employees.Select(u => new EmployeeDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    UserRole = u.UserRole,
                    Gender = u.Gender
                }).ToList();

                return Ok(new ApiResponse(200, "Employees retrieved successfully", new { Items = employeeList, TotalItems = totalItems }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving employees", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDto employeeDto)
        {
            if (!ModelState.IsValid || employeeDto == null)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                    StatusCode = 400,
                    Message = "Invalid employee data"
                });
            }

            try
            {
                var user = new AppUser
                {
                    UserName = employeeDto.Email,
                    Email = employeeDto.Email,
                    DisplayName = employeeDto.DisplayName,
                    UserRole = employeeDto.UserRole,
                    Gender = employeeDto.Gender
                };

                var result = await _userManager.CreateAsync(user, "DefaultPassword123!"); // كلمة مرور افتراضية
                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse(400, "Failed to create employee.", result.Errors));
                }

                var role = employeeDto.UserRole switch
                {
                    1 => "Admin",
                    2 => "Trainer",
                    3 => "Receptionist",
                    _ => "Member"
                };
                await _userManager.AddToRoleAsync(user, role);

                return StatusCode(StatusCodes.Status201Created, new ApiResponse(201, "Employee created successfully", new { Id = user.Id }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the employee", ex.Message));
            }
        }
    }

}