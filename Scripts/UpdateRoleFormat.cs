using System;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using TrainingApi.Models;
using TrainingApi.Utilities;

namespace TrainingApi.Scripts
{
    public class UpdateRoleFormat
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _usersCollection = "users";

        public UpdateRoleFormat(IConfiguration configuration)
        {
            string projectId = configuration["Firebase:ProjectId"];
            string credentialFilePath = configuration["Firebase:CredentialFile"];
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);
            _firestoreDb = FirestoreDb.Create(projectId);
        }

        public async Task StandardizeRoleFormat()
        {
            Console.WriteLine("Starting role format standardization...");
            
            Query query = _firestoreDb.Collection(_usersCollection);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            
            int updatedCount = 0;
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                Dictionary<string, object> userData = documentSnapshot.ToDictionary();
                if (userData.ContainsKey("Role"))
                {
                    UserRole role = EnumConverter.ParseUserRole(userData["Role"]);
                    
                    // Standardize to string format
                    string standardizedRole = role.ToString();
                    
                    // Only update if the format is different
                    if (userData["Role"].ToString() != standardizedRole)
                    {
                        DocumentReference docRef = _firestoreDb.Collection(_usersCollection).Document(documentSnapshot.Id);
                        await docRef.UpdateAsync("Role", standardizedRole);
                        updatedCount++;
                        Console.WriteLine($"Updated role format for user {documentSnapshot.Id} from {userData["Role"]} to {standardizedRole}");
                    }
                }
            }
            
            Console.WriteLine($"Completed standardization. Updated {updatedCount} documents.");
        }
    }
}
