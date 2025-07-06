using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainingApi.Models;
using TrainingApi.Repositories;
using TrainingApi.Services;

namespace TrainingApi.Controllers
{
    /// <summary>
    /// Controller for user authentication operations including registration and login
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IFirebaseAuthService _firebaseAuthService;

        public AuthController(IUserRepository userRepository, IFirebaseAuthService firebaseAuthService)
        {
            _userRepository = userRepository;
            _firebaseAuthService = firebaseAuthService;
        }

        /// <summary>
        /// Register a new student account
        /// </summary>
        /// <param name="request">Student registration information</param>
        /// <returns>Registration confirmation with user ID</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid registration data</response>
        [HttpPost("register/student")]
        [ProducesResponseType(typeof(SuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentRequest request)
        {
            try
            {
                var userRecord = await _firebaseAuthService.CreateUserAsync(request.Email, request.Password, UserRole.Student);
                
                var student = new Student
                {
                    Id = userRecord.Uid,
                    Email = request.Email,
                    DisplayName = request.DisplayName ?? userRecord.DisplayName,
                    Role = UserRole.Student,
                    StudentId = "STU" + new Random().Next(10000, 99999),
                    Year = request.Year,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUserAsync(student);
                
                return Ok(new { message = "Student registration successful", userId = student.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Register a new teacher account (requires admin approval)
        /// </summary>
        /// <param name="request">Teacher registration information</param>
        /// <returns>Registration confirmation with approval pending status</returns>
        /// <response code="200">Registration submitted successfully</response>
        /// <response code="400">Invalid registration data</response>
        [HttpPost("register/teacher")]
        [ProducesResponseType(typeof(SuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherRequest request)
        {
            try
            {
                // Teachers need approval before they can access teacher features, 
                // so we'll create them with a pending status
                var userRecord = await _firebaseAuthService.CreateUserAsync(request.Email, request.Password, UserRole.Teacher);
                
                var teacher = new Teacher
                {
                    Id = userRecord.Uid,
                    Email = request.Email,
                    DisplayName = request.DisplayName ?? userRecord.DisplayName,
                    Role = UserRole.Teacher,
                    Department = request.Department,
                    Qualification = request.Qualification,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUserAsync(teacher);
                
                return Ok(new { 
                    message = "Teacher registration submitted successfully. An administrator will review your application.", 
                    userId = teacher.Id 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// User login with Firebase token
        /// </summary>
        /// <param name="request">Login request with Firebase ID token</param>
        /// <returns>User information and role</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid token</response>
        /// <response code="404">User not found</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Verify the Firebase token
                var uid = await _firebaseAuthService.VerifyIdTokenAsync(request.IdToken);
                var user = await _userRepository.GetUserAsync(uid);
                
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                
                // Return user data with role information
                return Ok(new { 
                    user = user,
                    role = user.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Admin login with additional security verification
        /// </summary>
        /// <param name="request">Admin login request with Firebase ID token and admin code</param>
        /// <returns>Admin user information</returns>
        /// <response code="200">Admin login successful</response>
        /// <response code="400">Invalid token or admin code</response>
        /// <response code="403">Not an admin account</response>
        /// <response code="404">User not found</response>
        [HttpPost("admin/login")]
        [ProducesResponseType(typeof(AdminLoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            try
            {
                // For admin login, we'll require a special code in addition to normal authentication
                // This adds an extra layer of security for admin accounts
                
                // First verify Firebase token
                var uid = await _firebaseAuthService.VerifyIdTokenAsync(request.IdToken);
                var user = await _userRepository.GetUserAsync(uid);
                
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Check if the user is an admin
                if (user.Role != UserRole.Admin)
                {
                    return Forbid();
                }

                // Check admin verification code
                // In a real application, this could be a time-based OTP or another secure method
                if (request.AdminCode != "AdminSecretCode123") // Replace with a more secure mechanism
                {
                    return BadRequest(new { error = "Invalid admin verification code" });
                }

                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                
                // Return user data with admin information
                return Ok(new { 
                    user = user,
                    role = "Admin",
                    isAdmin = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Student registration request data
    /// </summary>
    public class RegisterStudentRequest
    {
        /// <summary>
        /// Email address (will be used for login)
        /// </summary>
        /// <example>student@example.com</example>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Password (minimum 6 characters)
        /// </summary>
        /// <example>SecurePassword123</example>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional display name (if not provided, will use email username)
        /// </summary>
        /// <example>John Student</example>
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Current academic year
        /// </summary>
        /// <example>1</example>
        public int Year { get; set; } = 1;
    }

    /// <summary>
    /// Teacher registration request data
    /// </summary>
    public class RegisterTeacherRequest
    {
        /// <summary>
        /// Email address (will be used for login)
        /// </summary>
        /// <example>teacher@example.com</example>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Password (minimum 6 characters)
        /// </summary>
        /// <example>SecurePassword123</example>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional display name (if not provided, will use email username)
        /// </summary>
        /// <example>Jane Teacher</example>
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Department of the teacher
        /// </summary>
        /// <example>Mathematics</example>
        public string Department { get; set; } = string.Empty;
        
        /// <summary>
        /// Qualification of the teacher
        /// </summary>
        /// <example>PhD in Mathematics</example>
        public string Qualification { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login request data
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Firebase ID token
        /// </summary>
        /// <example>eyJhbGciOiJSUzI1NiIsImtpZCI6...</example>
        public string IdToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Admin login request data
    /// </summary>
    public class AdminLoginRequest
    {
        /// <summary>
        /// Firebase ID token
        /// </summary>
        /// <example>eyJhbGciOiJSUzI1NiIsImtpZCI6...</example>
        public string IdToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Admin verification code
        /// </summary>
        /// <example>AdminSecretCode123</example>
        public string AdminCode { get; set; } = string.Empty;
    }
}

/// <summary>
/// Standard success response
/// </summary>
public class SuccessResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID (if applicable)
    /// </summary>
    public string? UserId { get; set; }
}

/// <summary>
/// Error response
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Error { get; set; } = string.Empty;
}

/// <summary>
/// Login response with user data
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// User information
    /// </summary>
    public User User { get; set; }
    
    /// <summary>
    /// User role
    /// </summary>
    /// <example>Student</example>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Admin login response
/// </summary>
public class AdminLoginResponse
{
    /// <summary>
    /// Admin user information
    /// </summary>
    public User User { get; set; }
    
    /// <summary>
    /// User role (always "Admin")
    /// </summary>
    /// <example>Admin</example>
    public string Role { get; set; } = "Admin";
    
    /// <summary>
    /// Flag indicating admin status
    /// </summary>
    /// <example>true</example>
    public bool IsAdmin { get; set; } = true;
}
