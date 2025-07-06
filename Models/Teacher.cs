using System.Collections.Generic;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TrainingApi.Models
{
    [FirestoreData]
    public class Teacher : User
    {
        // Parameterless constructor required by Firestore
        public Teacher() { }
        
        [FirestoreProperty]
        public string Department { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public string Qualification { get; set; } = string.Empty;
        
        [FirestoreProperty]
        public List<string> Courses { get; set; } = new List<string>();
        
        [FirestoreProperty(ConverterType = typeof(TeacherApprovalStatusConverter))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TeacherApprovalStatus ApprovalStatus { get; set; } = TeacherApprovalStatus.Pending;

        [FirestoreProperty]
        public string Specialty { get; set; }

        [FirestoreProperty]
        public List<string> CourseIds { get; set; } = new List<string>();
    }

    public enum TeacherApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
