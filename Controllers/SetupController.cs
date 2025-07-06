using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TrainingApi.Models;
using TrainingApi.Repositories;
using TrainingApi.Services;

namespace TrainingApi.Controllers
{
    /// <summary>
    /// Controller for system setup operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SetupController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IConfiguration _configuration;

        public SetupController(
            IUserRepository userRepository, 
            IFirebaseAuthService firebaseAuthService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _firebaseAuthService = firebaseAuthService;
            _configuration = configuration;
        }

        /// <summary>
        /// Create the first admin user in the system (can only be used once)
        /// </summary>
        /// <param name="request">Admin setup information including setup key</param>
        /// <returns>Admin creation confirmation with admin code</returns>
        /// <response code="200">Admin created successfully</response>
        /// <response code="400">Invalid setup key or admin already exists</response>
        [HttpPost("create-first-admin")]
        [ProducesResponseType(typeof(FirstAdminResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> CreateFirstAdmin([FromBody] CreateFirstAdminRequest request)
        {
            // Check if setup key matches the one in configuration
            if (request.SetupKey != _configuration["AdminSetup:InitialSetupKey"])
            {
                return BadRequest(new { error = "Invalid setup key" });
            }

            try
            {
                // Check if any admin already exists
                var users = await _userRepository.GetAllUsersAsync();
                var adminExists = users.Any(u => u.Role == UserRole.Admin);
                
                if (adminExists)
                {
                    return BadRequest(new { error = "Admin user already exists. This endpoint can only be used for initial setup." });
                }
                
                // Create the first admin user
                var userRecord = await _firebaseAuthService.CreateUserAsync(request.Email, request.Password, UserRole.Admin);
                
                var currentUtcTime = DateTime.UtcNow;
                var admin = new AdminUser
                {
                    Id = userRecord.Uid,
                    Email = request.Email,
                    DisplayName = request.DisplayName ?? userRecord.DisplayName,
                    Role = UserRole.Admin,
                    IsSuperAdmin = true,
                    CreatedAt = currentUtcTime,
                    UpdatedAt = currentUtcTime,
                    LastLogin = currentUtcTime
                };

                await _userRepository.CreateUserAsync(admin);
                
                return Ok(new { 
                    message = "First admin created successfully", 
                    userId = admin.Id,
                    adminCode = "AdminSecretCode123" // In a real app, generate this dynamically and store securely
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request to create the first admin user
    /// </summary>
    public class CreateFirstAdminRequest
    {
        /// <summary>
        /// Setup key from configuration
        /// </summary>
        /// <example>SecureSetupKey123456</example>
        public string SetupKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Email address for the admin
        /// </summary>
        /// <example>admin@example.com</example>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Password for the admin (minimum 6 characters)
        /// </summary>
        /// <example>SecureAdminPassword123</example>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Display name for the admin
        /// </summary>
        /// <example>System Administrator</example>
        public string? DisplayName { get; set; }
    }

    /// <summary>
    /// Response for successful first admin creation
    /// </summary>
    public class FirstAdminResponse
    {
        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Admin user ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Admin verification code to use for admin login
        /// </summary>
        public string AdminCode { get; set; } = string.Empty;
    }
}
