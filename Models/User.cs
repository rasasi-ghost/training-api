using System;
using Google.Cloud.Firestore;
using TrainingApi.Utilities;

namespace TrainingApi.Models
{
    /// <summary>
    /// Base user model with common properties
    /// </summary>
    [FirestoreData]
    public class User
    {
        // Parameterless constructor required by Firestore
        public User() { }

        /// <summary>
        /// Unique identifier
        /// </summary>
        /// <example>firebase-user-id-123</example>
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        /// <example>user@example.com</example>
        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        /// <example>John Doe</example>
        [FirestoreProperty]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// User role
        /// </summary>
        /// <example>Student</example>
        [FirestoreProperty(ConverterType = typeof(FirestoreUserRoleConverter))]
        public UserRole Role { get; set; }

        /// <summary>
        /// Account creation timestamp
        /// </summary>
        /// <example>2023-01-15T08:30:00Z</example>
        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        /// <example>2023-06-20T14:45:00Z</example>
        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// LastLogin timestamp
        /// </summary>
        /// <example>2023-06-20T14:45:00Z</example>
        [FirestoreProperty]
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// User roles in the system
    /// </summary>
    [FirestoreData]
    public enum UserRole
    {
        /// <summary>
        /// Administrator with full system access
        /// </summary>
        Admin = 0,

        /// <summary>
        /// Teacher who can create and manage courses
        /// </summary>
        Teacher = 1,

        /// <summary>
        /// Student who can enroll in courses
        /// </summary>
        Student = 2,

        /// <summary>
        /// Regular user with limited access
        /// </summary>
        User = 3
    }

    // Custom converter for Role property to handle different formats in Firestore
    public class FirestoreUserRoleConverter : IFirestoreConverter<UserRole>
    {
        public UserRole FromFirestore(object value)
        {
            return EnumConverter.ParseUserRole(value);
        }

        public object ToFirestore(UserRole value)
        {
            // Store as string to ensure consistent serialization
            return value.ToString();
        }
    }
}
