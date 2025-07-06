using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace TrainingApi.Models
{
    [FirestoreData]
    public class Student : User
    {
        // Parameterless constructor required by Firestore
        public Student() { }

        [FirestoreProperty]
        public string StudentId { get; set; } = string.Empty;

        [FirestoreProperty]
        public int Year { get; set; }

        [FirestoreProperty]
        public List<string> EnrolledCourses { get; set; } = new List<string>();

   

        [FirestoreProperty]
        public List<string> EnrolledCourseIds { get; set; } = new List<string>();
    }
}
