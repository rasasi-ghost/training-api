using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using TrainingApi.Models;
using TrainingApi.Utilities;

namespace TrainingApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _usersCollection = "users";

        public UserRepository(IConfiguration configuration)
        {
            try
            {
                string projectId = configuration["Firebase:ProjectId"];

                // Find the credential file path
                string credentialFilePath = Path.Combine(AppContext.BaseDirectory, configuration["Firebase:CredentialFile"]);

                // Check if the file exists in the base directory
                if (!File.Exists(credentialFilePath))
                {
                    // Try to find it in the root of the project
                    credentialFilePath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Firebase:CredentialFile"]);

                    if (!File.Exists(credentialFilePath))
                    {
                        throw new FileNotFoundException($"Firebase credential file not found at {credentialFilePath}");
                    }
                }

                // Set the environment variable for Google Application Credentials
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);

                _firestoreDb = FirestoreDb.Create(projectId);
                Console.WriteLine($"Successfully connected to Firestore for project: {projectId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Firestore: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_usersCollection).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> userData = snapshot.ToDictionary();
                UserRole role = Enum.Parse<UserRole>(userData["Role"].ToString());

                switch (role)
                {
                    case UserRole.Admin:
                        return snapshot.ConvertTo<AdminUser>();
                    case UserRole.Teacher:
                    return snapshot.ConvertTo<Teacher>();
                    case UserRole.Student:
                        return snapshot.ConvertTo<Student>();
                    default:
                        return snapshot.ConvertTo<User>();
                }
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            Query query = _firestoreDb.Collection(_usersCollection);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<User> users = new List<User>();

            try
            {
                foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
                {
                    try
                    {
                        Dictionary<string, object> userData = documentSnapshot.ToDictionary();
                        
                        // Get the role first to determine which type to create
                        UserRole role = UserRole.User;
                        if (userData.ContainsKey("Role"))
                        {
                            role = EnumConverter.ParseUserRole(userData["Role"]);
                        }
                        
                        // Create the appropriate user type based on role
                        User user;
                        try
                        {
                            switch (role)
                            {
                                case UserRole.Admin:
                                    user = documentSnapshot.ConvertTo<AdminUser>();
                                    break;
                                case UserRole.Teacher:
                                    user = documentSnapshot.ConvertTo<Teacher>();
                                    break;
                                case UserRole.Student:
                                    user = documentSnapshot.ConvertTo<Student>();
                                    break;
                                default:
                                    user = documentSnapshot.ConvertTo<User>();
                                    break;
                            }
                            
                            users.Add(user);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error converting user with role {role}: {ex.Message}. Falling back to manual conversion.");
                            
                            // Manual conversion as fallback
                            user = CreateUserByRole(role);
                            user.Id = documentSnapshot.Id;
                            user.Email = GetStringProperty(userData, "Email");
                            user.DisplayName = GetStringProperty(userData, "DisplayName");
                            user.Role = role;
                            user.CreatedAt = GetDateTimeProperty(userData, "CreatedAt", DateTime.UtcNow);
                            user.UpdatedAt = GetDateTimeProperty(userData, "UpdatedAt", DateTime.UtcNow);
                            user.LastLogin = GetDateTimeProperty(userData, "LastLogin", DateTime.UtcNow);
                            
                            users.Add(user);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing user document: {ex.Message}");
                        // Continue to next document instead of failing the entire operation
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                throw;
            }
        }

        private User CreateUserByRole(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:
                    return new AdminUser();
                case UserRole.Teacher:
                    return new Teacher();
                case UserRole.Student:
                    return new Student();
                default:
                    return new User();
            }
        }

        private string GetStringProperty(Dictionary<string, object> data, string key)
        {
            return data.ContainsKey(key) ? data[key]?.ToString() ?? string.Empty : string.Empty;
        }

        private DateTime GetDateTimeProperty(Dictionary<string, object> data, string key, DateTime defaultValue)
        {
            if (data.ContainsKey(key) && data[key] is Timestamp timestamp)
            {
                return timestamp.ToDateTime();
            }
            return defaultValue;
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            Query query = _firestoreDb.Collection(_usersCollection).WhereEqualTo("Role", UserRole.Teacher.ToString());
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Teacher> teachers = new List<Teacher>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                teachers.Add(documentSnapshot.ConvertTo<Teacher>());
            }

            return teachers;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            Query query = _firestoreDb.Collection(_usersCollection).WhereEqualTo("Role", UserRole.Student.ToString());
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Student> students = new List<Student>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                students.Add(documentSnapshot.ConvertTo<Student>());
            }

            return students;
        }

        public async Task CreateUserAsync(User user)
        {
            // Ensure the UpdatedAt is set to current time
            user.UpdatedAt = DateTime.UtcNow;
            
            DocumentReference docRef = _firestoreDb.Collection(_usersCollection).Document(user.Id);
            await docRef.SetAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            // Always update the UpdatedAt timestamp
            user.UpdatedAt = DateTime.UtcNow;
            
            DocumentReference docRef = _firestoreDb.Collection(_usersCollection).Document(user.Id);
            await docRef.SetAsync(user, SetOptions.MergeAll);
        }

        public async Task DeleteUserAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_usersCollection).Document(id);
            await docRef.DeleteAsync();
        }
    }

    public interface IUserRepository
    {
        Task<User> GetUserAsync(string id);
        Task<List<User>> GetAllUsersAsync();
        Task<List<Teacher>> GetAllTeachersAsync();
        Task<List<Student>> GetAllStudentsAsync();
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string id);
    }
}
