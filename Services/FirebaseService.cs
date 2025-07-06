using System;
using System.IO;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace TrainingApi.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly IConfiguration _configuration;
        private FirestoreDb _firestoreDb;

        public FirebaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            try
            {
                string projectId = _configuration["Firebase:ProjectId"];
                string credentialFilePath = ResolveCredentialFilePath();

                // Set the environment variable for Google Application Credentials
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);

                // Initialize Firebase Admin SDK if not already initialized
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialFilePath),
                        ProjectId = projectId
                    });
                    Console.WriteLine("Firebase Admin SDK initialized successfully.");
                }

                // Initialize Firestore
                _firestoreDb = FirestoreDb.Create(projectId);
                Console.WriteLine($"Firestore initialized for project: {projectId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firebase initialization error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }


        private string ResolveCredentialFilePath()
        {
            string credentialFileName = _configuration["Firebase:CredentialFile"];

            // ✅ 1. Check the hardcoded path Render uses for the secret file
            string renderPath = "/firebase-credentials.json";
            if (File.Exists(renderPath))
                return renderPath;

            // ✅ 2. Fallbacks for local dev
            string baseDirPath = Path.Combine(AppContext.BaseDirectory, credentialFileName);
            if (File.Exists(baseDirPath))
                return baseDirPath;

            string currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), credentialFileName);
            if (File.Exists(currentDirPath))
                return currentDirPath;

            string parentDirPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, credentialFileName);
            if (File.Exists(parentDirPath))
                return parentDirPath;

            throw new FileNotFoundException(
                $"Firebase credential file '{credentialFileName}' not found. Searched: {renderPath}, {baseDirPath}, {currentDirPath}, {parentDirPath}");
        }


        public FirestoreDb GetFirestoreDb() => _firestoreDb;
    }

    public interface IFirebaseService
    {
        FirestoreDb GetFirestoreDb();
    }
}
