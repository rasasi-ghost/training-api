using System.ComponentModel.DataAnnotations;

namespace TrainingApi.DTOs
{
    public class SetGradeRequest
    {
        [Required]
        public string EnrollmentId { get; set; } = string.Empty;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string Grade { get; set; } = string.Empty;
    }
}
