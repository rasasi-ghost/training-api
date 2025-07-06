using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace TrainingApi.Models
{
    [FirestoreData]
    public class Course
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public string Title { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public string Description { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public string TeacherId { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public string TeacherName { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public int MaxEnrollment { get; set; }
        
        [FirestoreProperty]
        public DateTime StartDate { get; set; }
        
        [FirestoreProperty]
        public DateTime EndDate { get; set; }
        
        [FirestoreProperty]
        public List<Lecture> Lectures { get; set; } = new List<Lecture>();
        
        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [FirestoreProperty]
        public DateTime? UpdatedAt { get; set; }
        
        [FirestoreProperty]
        public bool IsActive { get; set; } = true;
    }

    [FirestoreData]
    public class Lecture
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [FirestoreProperty]
        public string Title { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public string Description { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public DateTime StartTime { get; set; }
        
        [FirestoreProperty]
        public DateTime EndTime { get; set; }
        
        [FirestoreProperty]
        public string Location { get; set; } = string.Empty;
    }
}
