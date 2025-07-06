using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using TrainingApi.Models;

namespace TrainingApi.Services
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(IConfiguration configuration)
        {
            _configuration = configuration;

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    string credentialFilePath = Path.Combine(AppContext.BaseDirectory, _configuration["Firebase:CredentialFile"]);
                    
                    // Check if the file exists in the base directory
                    if (!File.Exists(credentialFilePath))
                    {
                        // Try to find it in the root of the project
                        credentialFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["Firebase:CredentialFile"]);
                        
                        if (!File.Exists(credentialFilePath))
                        {
                            throw new FileNotFoundException($"Firebase credential file not found at {credentialFilePath}");
                        }
                    }

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialFilePath),
                        ProjectId = _configuration["Firebase:ProjectId"]
                    });
                    Console.WriteLine("Firebase App initialized successfully.");
                }
                
                _firebaseAuth = FirebaseAuth.DefaultInstance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firebase initialization error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserRecord> CreateUserAsync(string email, string password, UserRole role)
        {
            var args = new UserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = email.Split('@')[0],
                EmailVerified = false,
                Disabled = false
            };

            var userRecord = await _firebaseAuth.CreateUserAsync(args);
            
            // Set custom claims for role-based authorization
            await _firebaseAuth.SetCustomUserClaimsAsync(userRecord.Uid, new Dictionary<string, object>
            {
                { "role", role.ToString() }
            });

            return userRecord;
        }

        public async Task<UserRecord> GetUserAsync(string uid)
        {
            return await _firebaseAuth.GetUserAsync(uid);
        }

        public async Task UpdateUserRoleAsync(string uid, UserRole role)
        {
            await _firebaseAuth.SetCustomUserClaimsAsync(uid, new Dictionary<string, object>
            {
                { "role", role.ToString() }
            });
        }

        public async Task DeleteUserAsync(string uid)
        {
            await _firebaseAuth.DeleteUserAsync(uid);
        }

        public async Task<string> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(idToken);
                Console.WriteLine($"Token verified successfully for UID: {decodedToken.Uid}");
                return decodedToken.Uid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token verification failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }

    public interface IFirebaseAuthService
    {
        Task<UserRecord> CreateUserAsync(string email, string password, UserRole role);
        Task<UserRecord> GetUserAsync(string uid);
        Task UpdateUserRoleAsync(string uid, UserRole role);
        Task DeleteUserAsync(string uid);
        Task<string> VerifyIdTokenAsync(string idToken);
    }
}
