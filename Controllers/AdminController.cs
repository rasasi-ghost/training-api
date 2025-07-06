using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingApi.Models;
using TrainingApi.Repositories;
using TrainingApi.Services;

namespace TrainingApi.Controllers
{
    /// <summary>
    /// Controller for admin operations including user management and teacher approvals
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IFirebaseAuthService _firebaseAuthService;

        public AdminController(IUserRepository userRepository, IFirebaseAuthService firebaseAuthService)
        {
            _userRepository = userRepository;
            _firebaseAuthService = firebaseAuthService;
        }

        /// <summary>
        /// Get a list of all users in the system
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code="200">Returns the list of users</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not an admin)</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get detailed information about a specific user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        /// <response code="200">Returns the user details</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not an admin)</response>
        /// <response code="404">User not found</response>
        [HttpGet("users/{id}")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var userRecord = await _firebaseAuthService.CreateUserAsync(request.Email, request.Password, request.Role);
                
                User user;
                switch (request.Role)
                {
                    case UserRole.Admin:
                        user = new AdminUser
                        {
                            IsSuperAdmin = request.IsSuperAdmin
                        };
                        break;
                    case UserRole.Teacher:
                        user = new Teacher
                        {
                            Department = request.Department,
                            Qualification = request.Qualification
                        };
                        break;
                    case UserRole.Student:
                        user = new Student
                        {
                            StudentId = request.StudentId ?? "STU" + new Random().Next(10000, 99999),
                            Year = request.Year,
                        };
                        break;
                    default:
                        user = new User();
                        break;
                }

                user.Id = userRecord.Uid;
                user.Email = request.Email;
                user.DisplayName = request.DisplayName ?? userRecord.DisplayName;
                user.Role = request.Role;
                user.CreatedAt = DateTime.UtcNow;

                await _userRepository.CreateUserAsync(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await _firebaseAuthService.UpdateUserRoleAsync(id, request.Role);
                
                user.Role = request.Role;
                await _userRepository.UpdateUserAsync(user);
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await _firebaseAuthService.DeleteUserAsync(id);
                await _userRepository.DeleteUserAsync(id);
                
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a list of teacher accounts pending approval
        /// </summary>
        /// <returns>List of pending teacher accounts</returns>
        /// <response code="200">Returns the list of pending teachers</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not an admin)</response>
        [HttpGet("pending-teachers")]
        [ProducesResponseType(typeof(List<Teacher>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetPendingTeachers()
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            var pendingTeachers = allUsers
                .Where(u => u.Role == UserRole.Teacher)
                .Cast<Teacher>()
                .Where(t => t.ApprovalStatus == TeacherApprovalStatus.Pending)
                .ToList();
                
            return Ok(pendingTeachers);
        }

        [HttpPut("teachers/{id}/approve")]
        public async Task<IActionResult> ApproveTeacher(string id)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                
                if (user.Role != UserRole.Teacher)
                {
                    return BadRequest(new { error = "User is not a teacher" });
                }
                
                var teacher = (Teacher)user;
                teacher.ApprovalStatus = TeacherApprovalStatus.Approved;
                
                await _userRepository.UpdateUserAsync(teacher);
                
                return Ok(new { message = "Teacher approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("teachers/{id}/reject")]
        public async Task<IActionResult> RejectTeacher(string id)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                
                if (user.Role != UserRole.Teacher)
                {
                    return BadRequest(new { error = "User is not a teacher" });
                }
                
                var teacher = (Teacher)user;
                teacher.ApprovalStatus = TeacherApprovalStatus.Rejected;
                
                await _userRepository.UpdateUserAsync(teacher);
                
                return Ok(new { message = "Teacher rejected" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request to create a new user
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Email address for the new user
        /// </summary>
        /// <example>user@example.com</example>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Password for the new user (minimum 6 characters)
        /// </summary>
        /// <example>SecurePassword123</example>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Display name for the user
        /// </summary>
        /// <example>John Doe</example>
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Role assigned to the user
        /// </summary>
        /// <example>Teacher</example>
        public UserRole Role { get; set; }
        
        public bool IsSuperAdmin { get; set; }
        public string? Department { get; set; }
        public string? Qualification { get; set; }
        public string? StudentId { get; set; }
        public int Year { get; set; } = 1;
    }

    public class UpdateRoleRequest
    {
        public UserRole Role { get; set; }
    }
}