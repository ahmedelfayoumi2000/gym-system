using AutoMapper;
using GymSystem.API.Helpers;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications.EmployeeSpec;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
           IEmployeeRepository employeeRepository,
            IMapper mapper
            )
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _employeeRepository = employeeRepository;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEmployees([FromQuery] SpecPrams specParams)
        {
            try
            {
                var spec = new EmployeeSpecification(specParams);
                var countSpec = new EmployeeWithFiltersForCountSpecification(specParams);

                var totalItems = await _employeeRepository.GetCountAsync(countSpec);
                var employees = await _employeeRepository.GetAllWithSpecAsync(spec);

                var data = _mapper.Map<IReadOnlyList<AppUser>, IReadOnlyList<EmployeeDto>>(employees);

                return Ok(new Pagination<EmployeeDto>(
                    specParams.PageIndex,
                    specParams.PageSize,
                    totalItems,
                    data
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving employees", ex.Message));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            try
            {
                var spec = new EmployeeSpecification(id);
                var employee = (await _employeeRepository.GetAllWithSpecAsync(spec)).FirstOrDefault();

                if (employee == null)
                {
                    return NotFound(new ApiResponse(404, $"Employee with ID {id} not found"));
                }

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Ok(new ApiResponse(200, "Employee retrieved successfully", employeeDto));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving the employee", ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDto employeeDto)
        {
            if (!ModelState.IsValid || employeeDto == null)
            {
                return BadRequest(CreateValidationError("Invalid employee data"));
            }

            try
            {
                var user = _mapper.Map<AppUser>(employeeDto);
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, employeeDto.PassWord);
                if (!result.Succeeded)
                {

                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to create employee: {errors}"));
                }

                await AssignRole(user, employeeDto.UserRole);

                var responseData = new { Id = user.Id, UserRole = user.UserRole, Salary = user.Salary };
                return StatusCode(StatusCodes.Status201Created,
                    new ApiResponse(201, "Employee created successfully", responseData));
            }
            catch (Exception ex)
            {
                var user = await _userManager.FindByEmailAsync(employeeDto.Email);
                await _userManager.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the employee", ex.Message));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] EmployeeDto employeeDto)
        {
            if (!ModelState.IsValid || employeeDto == null)
            {
                return BadRequest(CreateValidationError("Invalid employee update data"));
            }

            if (id != employeeDto.Id)
            {
                return BadRequest(new ApiResponse(400, "Employee ID in route and model must match"));
            }

            try
            {
                var employee = await _userManager.FindByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(new ApiResponse(404, $"Employee with ID {id} not found"));
                }

                var emailValidationResult = await ValidateEmail(employee, employeeDto.Email);
                if (emailValidationResult != null)
                {
                    return BadRequest(emailValidationResult);
                }

                UpdateEmployeeDetails(employee, employeeDto);

                var roleValidationResult = await UpdateEmployeeRole(employee, employeeDto.UserRole);
                if (roleValidationResult != null)
                {
                    return BadRequest(roleValidationResult);
                }

                var updateResult = await _userManager.UpdateAsync(employee);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return BadRequest(new ApiResponse(400, $"Failed to update Employee: {errors}"));
                }

                return Ok(new ApiResponse(200, "Employee updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the employee", ex.Message));
            }
        }

        #region Private Helper Methods

        private async Task AssignRole(AppUser user, int userRole)
        {
            var role = userRole switch
            {
                1 => "Admin",
                2 => "Trainer",
                3 => "Receptionist",
                _ => throw new ArgumentException("Invalid user role", nameof(userRole))
            };

            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                throw new InvalidOperationException($"Role '{role}' does not exist.");
            }

            await _userManager.AddToRoleAsync(user, role);
        }

        private async Task<ApiResponse> ValidateEmail(AppUser employee, string newEmail)
        {
            if (string.Equals(employee.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != employee.Id)
            {
                return new ApiResponse(400, "Email is already in use by another user.");
            }

            employee.Email = newEmail;
            employee.NormalizedEmail = newEmail.ToUpper();
            return null;
        }

        private void UpdateEmployeeDetails(AppUser employee, EmployeeDto employeeDto)
        {
            employee.DisplayName = employeeDto.DisplayName;
            employee.Gender = employeeDto.Gender;
            employee.Salary = employeeDto.Salary;
        }

        private async Task<ApiResponse> UpdateEmployeeRole(AppUser employee, int userRole)
        {
            var currentRoles = await _userManager.GetRolesAsync(employee);
            await _userManager.RemoveFromRolesAsync(employee, currentRoles);

            var newRole = userRole switch
            {
                1 => "Admin",
                2 => "Trainer",
                3 => "Receptionist",
                _ => throw new ArgumentException("Invalid user role", nameof(userRole))
            };

            var roleExists = await _roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                return new ApiResponse(400, $"Role '{newRole}' does not exist.");
            }

            await _userManager.AddToRoleAsync(employee, newRole);
            return null;
        }

        private ApiValidationErrorResponse CreateValidationError(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }

        #endregion
    }
}