using System;
using Google.Cloud.Firestore;
using TrainingApi.Utilities;

namespace TrainingApi.Models
{
    [FirestoreData]
    public class Enrollment
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CourseId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string StudentId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string StudentName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string StatusString { get; set; } = nameof(EnrollmentStatus.Pending);

        [FirestoreProperty]
        public EnrollmentStatus Status
        {
            get => EnumConverter.ParseEnum<EnrollmentStatus>(StatusString, EnrollmentStatus.Pending);
            set => StatusString = value.ToString();
        }

        [FirestoreProperty]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? ApprovalDate { get; set; }
        
        [FirestoreProperty]
        public string Grade { get; set; } = string.Empty;

        [FirestoreProperty]
        public string TeacherId { get; set; } = string.Empty;
    }

    public enum EnrollmentStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
