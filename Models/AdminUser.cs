using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace TrainingApi.Models
{
    /// <summary>
    /// Administrator user model
    /// </summary>
    [FirestoreData]
    public class AdminUser : User
    {
        // Parameterless constructor required by Firestore
        public AdminUser() { }

        /// <summary>
        /// Indicates if this admin has privileges to create other admins
        /// </summary>
        /// <example>false</example>
        [FirestoreProperty]
        public bool IsSuperAdmin { get; set; } = false;

        [FirestoreProperty]
        public string AccessLevel { get; set; } = string.Empty;
    }
}
