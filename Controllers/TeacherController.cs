using System;
using System.Collections.Generic;
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
    /// Controller for teacher operations including course and lecture management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    [Produces("application/json")]
    public class TeacherController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;

        public TeacherController(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
        }

        private async Task<IActionResult> CheckApprovalStatus(string teacherId)
        {
            var user = await _userRepository.GetUserAsync(teacherId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (user.Role != UserRole.Teacher)
            {
                return Forbid();
            }

            var teacher = (Teacher)user;
            if (teacher.ApprovalStatus != TeacherApprovalStatus.Approved)
            {
                return BadRequest(new
                {
                    error = "Your teacher account is pending approval",
                    status = teacher.ApprovalStatus.ToString()
                });
            }

            return null; // Approval check passed
        }

        /// <summary>
        /// Get the current teacher's approval status
        /// </summary>
        /// <returns>Current approval status</returns>
        [HttpGet("approval-status")]
        public async Task<IActionResult> GetApprovalStatus()
        {
            string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userRepository.GetUserAsync(teacherId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (user.Role != UserRole.Teacher)
            {
                return Forbid();
            }

            var teacher = (Teacher)user;
            string message = "";

            switch (teacher.ApprovalStatus)
            {
                case TeacherApprovalStatus.Pending:
                    message = "Your teacher account is pending approval";
                    break;
                case TeacherApprovalStatus.Approved:
                    message = "Your teacher account is approved";
                    break;
                case TeacherApprovalStatus.Rejected:
                    message = "Your teacher account has been rejected";
                    break;
            }

            return Ok(new { status = teacher.ApprovalStatus, message });
        }

        /// <summary>
        /// Get all courses created by the authenticated teacher
        /// </summary>
        /// <returns>List of teacher's courses</returns>
        /// <response code="200">Returns the list of courses</response>
        /// <response code="400">Teacher account not approved</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a teacher)</response>
        [HttpGet("courses")]
        [ProducesResponseType(typeof(List<Course>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTeacherCourses()
        {
            string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check approval status
            var approvalCheckResult = await CheckApprovalStatus(teacherId);
            if (approvalCheckResult != null)
            {
                return approvalCheckResult;
            }

            var courses = await _courseRepository.GetCoursesByTeacherAsync(teacherId);
            return Ok(courses);
        }

        /// <summary>
        /// Get detailed information about a specific course
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <returns>Course details</returns>
        /// <response code="200">Returns the course details</response>
        /// <response code="400">Teacher account not approved</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden (not a teacher or not your course)</response>
        /// <response code="404">Course not found</response>
        [HttpGet("courses/{id}")]
        [ProducesResponseType(typeof(Course), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCourse(string id)
        {
            string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check approval status
            var approvalCheckResult = await CheckApprovalStatus(teacherId);
            if (approvalCheckResult != null)
            {
                return approvalCheckResult;
            }

            var course = await _courseRepository.GetCourseAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            if (course.TeacherId != teacherId)
            {
                return Forbid();
            }

            return Ok(course);
        }

        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var teacher = await _userRepository.GetUserAsync(teacherId) as Teacher;

                var course = new Course
                {
                    Title = request.Title,
                    Description = request.Description,
                    TeacherId = teacherId,
                    TeacherName = teacher.DisplayName,
                    MaxEnrollment = request.MaxEnrollment,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = true
                };

                var courseId = await _courseRepository.CreateCourseAsync(course);

                // Update teacher's courses list
                teacher.Courses.Add(courseId);
                await _userRepository.UpdateUserAsync(teacher);

                return Ok(new { id = courseId, course });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(string id, [FromBody] UpdateCourseRequest request)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var course = await _courseRepository.GetCourseAsync(id);
                if (course == null)
                {
                    return NotFound();
                }

                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }

                course.Title = request.Title ?? course.Title;
                course.Description = request.Description ?? course.Description;
                course.MaxEnrollment = request.MaxEnrollment ?? course.MaxEnrollment;
                course.StartDate = request.StartDate ?? course.StartDate;
                course.EndDate = request.EndDate ?? course.EndDate;
                course.IsActive = request.IsActive ?? course.IsActive;

                await _courseRepository.UpdateCourseAsync(course);

                return Ok(course);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("courses/{courseId}/lectures")]
        public async Task<IActionResult> AddLecture(string courseId, [FromBody] LectureRequest request)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var course = await _courseRepository.GetCourseAsync(courseId);
                if (course == null)
                {
                    return NotFound();
                }

                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }


                
                var lecture = new Lecture
                {
                    Title = request.Title,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Location = request.Location
                };

                if (lecture.StartTime.Kind != DateTimeKind.Utc)
                {
                    lecture.StartTime = lecture.StartTime.ToUniversalTime();
                }
                if (lecture.EndTime.Kind != DateTimeKind.Utc)
                {
                    lecture.EndTime = lecture.EndTime.ToUniversalTime();
                }

                await _courseRepository.AddLectureAsync(courseId, lecture);

                return Ok(lecture);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("courses/{courseId}/lectures/{lectureId}")]
        public async Task<IActionResult> UpdateLecture(string courseId, string lectureId, [FromBody] LectureRequest request)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var course = await _courseRepository.GetCourseAsync(courseId);
                if (course == null)
                {
                    return NotFound();
                }

                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }

                var lecture = course.Lectures.FirstOrDefault(l => l.Id == lectureId);
                if (lecture == null)
                {
                    return NotFound(new { error = "Lecture not found" });
                }

                lecture.Title = request.Title;
                lecture.Description = request.Description;
                lecture.StartTime = request.StartTime;
                lecture.EndTime = request.EndTime;
                lecture.Location = request.Location;

                await _courseRepository.UpdateLectureAsync(courseId, lecture);

                return Ok(lecture);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("courses/{courseId}/lectures/{lectureId}")]
        public async Task<IActionResult> DeleteLecture(string courseId, string lectureId)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var course = await _courseRepository.GetCourseAsync(courseId);
                if (course == null)
                {
                    return NotFound();
                }

                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }

                await _courseRepository.DeleteLectureAsync(courseId, lectureId);

                return Ok(new { message = "Lecture deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("courses/{courseId}/enrollments")]
        public async Task<IActionResult> GetCourseEnrollments(string courseId)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var course = await _courseRepository.GetCourseAsync(courseId);
                if (course == null)
                {
                    return NotFound();
                }

                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }

                var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseAsync(courseId);
                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("enrollments/{enrollmentId}/status")]
        public async Task<IActionResult> UpdateEnrollmentStatus(string enrollmentId, [FromBody] UpdateEnrollmentStatusRequest request)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var enrollment = await _enrollmentRepository.GetEnrollmentAsync(enrollmentId);
                if (enrollment == null)
                {
                    return NotFound();
                }

                var course = await _courseRepository.GetCourseAsync(enrollment.CourseId);
                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }

                await _enrollmentRepository.UpdateEnrollmentStatusAsync(enrollmentId, request.Status);

                // If approved, update student's enrolled courses
                if (request.Status == EnrollmentStatus.Approved)
                {
                    var student = await _userRepository.GetUserAsync(enrollment.StudentId) as Student;
                    if (student != null && !student.EnrolledCourses.Contains(enrollment.CourseId))
                    {
                        student.EnrolledCourses.Add(enrollment.CourseId);
                        await _userRepository.UpdateUserAsync(student);
                    }
                }

                return Ok(new { message = $"Enrollment status updated to {request.Status}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("set-grade")]
        public async Task<IActionResult> SetGrade(SetGradeRequest request)
        {
            try
            {
                string teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check approval status
                var approvalCheckResult = await CheckApprovalStatus(teacherId);
                if (approvalCheckResult != null)
                {
                    return approvalCheckResult;
                }

                var enrollment = await _enrollmentRepository.GetEnrollmentAsync(request.EnrollmentId);
                if (enrollment == null)
                {
                    return NotFound(new { error = "Enrollment not found" });
                }

                if (enrollment.StudentId != request.StudentId)
                {
                    return BadRequest(new { error = "Student ID does not match enrollment record" });
                }

                var course = await _courseRepository.GetCourseAsync(enrollment.CourseId);
                if (course.TeacherId != teacherId)
                {
                    return Forbid();
                }

                enrollment.Grade = request.Grade;
                await _enrollmentRepository.UpdateEnrollmentAsync(enrollment);

                return Ok(new { message = "Grade updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request to create a new course
    /// </summary>
    public class CreateCourseRequest
    {
        /// <summary>
        /// Course title
        /// </summary>
        /// <example>Introduction to Programming</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Course description
        /// </summary>
        /// <example>Learn the basics of programming with Python</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of students that can enroll
        /// </summary>
        /// <example>30</example>
        public int MaxEnrollment { get; set; } = 30;

        /// <summary>
        /// Course start date
        /// </summary>
        /// <example>2023-09-01T00:00:00Z</example>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Course end date
        /// </summary>
        /// <example>2023-12-15T00:00:00Z</example>
        public DateTime EndDate { get; set; }
    }

    public class UpdateCourseRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? MaxEnrollment { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class LectureRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class UpdateEnrollmentStatusRequest
    {
        public EnrollmentStatus Status { get; set; }
    }

    public class SetGradeRequest
    {
        public string EnrollmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Grade { get; set; }
    }
}
