using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingApi.Models;
using TrainingApi.Repositories;

namespace TrainingApi.Controllers
{
    /// <summary>
    /// Controller for student operations including course browsing and enrollment
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    [Produces("application/json")]
    public class StudentController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;

        public StudentController(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all available courses that are active
        /// </summary>
        /// <returns>List of active courses</returns>
        /// <response code="200">Returns the list of available courses</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a student)</response>
        [HttpGet("courses")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<Course>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAvailableCourses()
        {
            var courses = await _courseRepository.GetActiveCoursesAsync();
            return Ok(courses);
        }

        /// <summary>
        /// Get detailed information about a specific course
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <returns>Course details</returns>
        /// <response code="200">Returns the course details</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a student)</response>
        /// <response code="404">Course not found</response>
        [HttpGet("courses/{id}")]
        [ProducesResponseType(typeof(Course), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCourseDetails(string id)
        {
            var course = await _courseRepository.GetCourseAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            
            return Ok(course);
        }

        /// <summary>
        /// Get all enrollments of the student
        /// </summary>
        /// <returns>List of enrollments</returns>
        /// <response code="200">Returns the list of enrollments</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a student)</response>
        [HttpGet("enrollments")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<Enrollment>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetStudentEnrollments()
        {
            string studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentAsync(studentId);
            return Ok(enrollments);
        }

        /// <summary>
        /// Request enrollment in a specific course
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <returns>Enrollment request confirmation</returns>
        /// <response code="200">Enrollment request submitted</response>
        /// <response code="400">Invalid request or already enrolled</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a student)</response>
        /// <response code="404">Course not found</response>
        [HttpPost("courses/{courseId}/enroll")]
        [ProducesResponseType(typeof(EnrollmentResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EnrollInCourse(string courseId)
        {
            try
            {
                string studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var course = await _courseRepository.GetCourseAsync(courseId);
                if (course == null)
                {
                    return NotFound(new { error = "Course not found" });
                }
                
                if (!course.IsActive)
                {
                    return BadRequest(new { error = "Course is not active" });
                }
                
                // Check if already enrolled
                var existingEnrollment = await _enrollmentRepository.GetEnrollmentByCourseAndStudentAsync(courseId, studentId);
                if (existingEnrollment != null)
                {
                    return BadRequest(new { error = "Already enrolled in this course", status = existingEnrollment.Status });
                }
                
                // Check enrollment capacity
                var courseEnrollments = await _enrollmentRepository.GetEnrollmentsByCourseAsync(courseId);
                var approvedEnrollments = courseEnrollments.Count(e => e.Status == EnrollmentStatus.Approved);
                if (approvedEnrollments >= course.MaxEnrollment)
                {
                    return BadRequest(new { error = "Course has reached maximum enrollment capacity" });
                }
                
                var student = await _userRepository.GetUserAsync(studentId) as Student;
                
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    StudentName = student.DisplayName,
                    Status = EnrollmentStatus.Pending
                };
                
                var enrollmentId = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
                
                return Ok(new { id = enrollmentId, message = "Enrollment request submitted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get the status of a specific enrollment
        /// </summary>
        /// <param name="enrollmentId">Enrollment ID</param>
        /// <returns>Enrollment status and course details</returns>
        /// <response code="200">Returns the enrollment status and course details</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not the owner of the enrollment)</response>
        /// <response code="404">Enrollment not found</response>
        [HttpGet("enrollments/{enrollmentId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEnrollmentStatus(string enrollmentId)
        {
            string studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var enrollment = await _enrollmentRepository.GetEnrollmentAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }
            
            if (enrollment.StudentId != studentId)
            {
                return Forbid();
            }
            
            var course = await _courseRepository.GetCourseAsync(enrollment.CourseId);
            
            return Ok(new
            {
                enrollment,
                course
            });
        }

        /// <summary>
        /// Get all courses the student is enrolled in
        /// </summary>
        /// <returns>List of enrolled courses</returns>
        /// <response code="200">Returns the list of enrolled courses</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a student)</response>
        [HttpGet("courses/enrolled")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<Course>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetEnrolledCourses()
        {
            string studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var student = await _userRepository.GetUserAsync(studentId) as Student;
            if (student == null)
            {
                return NotFound();
            }
            
            var enrolledCourses = await Task.WhenAll(
                student.EnrolledCourses.Select(courseId => _courseRepository.GetCourseAsync(courseId)));
            
            // Filter out null values (in case a course was deleted)
            var courses = enrolledCourses.Where(c => c != null).ToList();
            
            return Ok(courses);
        }
    }

    /// <summary>
    /// Response for a successful enrollment request
    /// </summary>
    public class EnrollmentResponse
    {
        /// <summary>
        /// Enrollment ID
        /// </summary>
        /// <example>enrollment-123</example>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Confirmation message
        /// </summary>
        /// <example>Enrollment request submitted</example>
        public string Message { get; set; } = string.Empty;
    }
}
