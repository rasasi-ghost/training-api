using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingApi.Models;
using TrainingApi.Repositories;

namespace TrainingApi.Controllers
{
    /// <summary>
    /// Controller for user profile operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get the profile of the currently authenticated user
        /// </summary>
        /// <returns>User profile with role information</returns>
        /// <response code="200">Returns the user profile</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">User not found</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrentUser()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetUserAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // If user is a teacher, check approval status
            if (user.Role == UserRole.Teacher)
            {
                var teacher = (Teacher)user;
                return Ok(new
                {
                    user,
                    role = user.Role.ToString(),
                    approvalStatus = teacher.ApprovalStatus.ToString(),
                    isApproved = teacher.ApprovalStatus == TeacherApprovalStatus.Approved
                });
            }

            return Ok(new
            {
                user,
                role = user.Role.ToString()
            });
        }
    }

    /// <summary>
    /// Response with user profile information
    /// </summary>
    public class UserProfileResponse
    {
        /// <summary>
        /// User object with role-specific properties
        /// </summary>
        public User User { get; set; }
        
        /// <summary>
        /// User role as string
        /// </summary>
        /// <example>Teacher</example>
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Teacher approval status (only for teachers)
        /// </summary>
        /// <example>Approved</example>
        public string? ApprovalStatus { get; set; }
        
        /// <summary>
        /// Whether the teacher is approved (only for teachers)
        /// </summary>
        /// <example>true</example>
        public bool? IsApproved { get; set; }
    }
}
