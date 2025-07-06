using System;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using TrainingApi.Models;

namespace TrainingApi.Models
{
    public class TeacherApprovalStatusConverter : IFirestoreConverter<TeacherApprovalStatus>
    {
        public object ToFirestore(TeacherApprovalStatus value)
        {
            return value.ToString();
        }

        public TeacherApprovalStatus FromFirestore(object value)
        {
            if (value == null)
                return TeacherApprovalStatus.Pending;
                
            if (value is string stringValue)
            {
                if (Enum.TryParse<TeacherApprovalStatus>(stringValue, true, out TeacherApprovalStatus result))
                {
                    return result;
                }
            }
            
            Console.WriteLine($"Failed to convert value '{value}' of type '{value?.GetType().Name}' to TeacherApprovalStatus");
            return TeacherApprovalStatus.Pending; // Default value
        }
    }
}
