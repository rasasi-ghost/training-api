using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using TrainingApi.Models;

namespace TrainingApi.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _enrollmentsCollection = "enrollments";

        public EnrollmentRepository(IConfiguration configuration)
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

        public async Task<Enrollment> GetEnrollmentAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_enrollmentsCollection).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Enrollment>();
            }

            return null;
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseAsync(string courseId)
        {
            Query query = _firestoreDb.Collection(_enrollmentsCollection)
                .WhereEqualTo("CourseId", courseId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Enrollment> enrollments = new List<Enrollment>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                enrollments.Add(documentSnapshot.ConvertTo<Enrollment>());
            }

            return enrollments;
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStudentAsync(string studentId)
        {
            Query query = _firestoreDb.Collection(_enrollmentsCollection)
                .WhereEqualTo("StudentId", studentId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Enrollment> enrollments = new List<Enrollment>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                enrollments.Add(documentSnapshot.ConvertTo<Enrollment>());
            }

            return enrollments;
        }

        public async Task<Enrollment> GetEnrollmentByCourseAndStudentAsync(string courseId, string studentId)
        {
            Query query = _firestoreDb.Collection(_enrollmentsCollection)
                .WhereEqualTo("CourseId", courseId)
                .WhereEqualTo("StudentId", studentId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Count > 0)
            {
                return querySnapshot.Documents[0].ConvertTo<Enrollment>();
            }

            return null;
        }

        public async Task<string> CreateEnrollmentAsync(Enrollment enrollment)
        {
            enrollment.Id = Guid.NewGuid().ToString();
            enrollment.RequestDate = DateTime.UtcNow;

            DocumentReference docRef = _firestoreDb.Collection(_enrollmentsCollection).Document(enrollment.Id);
            await docRef.SetAsync(enrollment);

            return enrollment.Id;
        }

        public async Task UpdateEnrollmentStatusAsync(string id, EnrollmentStatus status)
        {
            var enrollment = await GetEnrollmentAsync(id);
            if (enrollment == null)
            {
                throw new Exception("Enrollment not found");
            }

            enrollment.Status = status;
            if (status == EnrollmentStatus.Approved || status == EnrollmentStatus.Rejected)
            {
                enrollment.ApprovalDate = DateTime.UtcNow;
            }

            DocumentReference docRef = _firestoreDb.Collection(_enrollmentsCollection).Document(id);
            await docRef.SetAsync(enrollment, SetOptions.MergeAll);
        }

        public async Task DeleteEnrollmentAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_enrollmentsCollection).Document(id);
            await docRef.DeleteAsync();
        }
        
         public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            DocumentReference docRef = _firestoreDb.Collection(_enrollmentsCollection).Document(enrollment.Id);
            await docRef.SetAsync(enrollment, SetOptions.MergeAll);
        }
    }

    public interface IEnrollmentRepository
    {
        Task<Enrollment> GetEnrollmentAsync(string id);
        Task<List<Enrollment>> GetEnrollmentsByCourseAsync(string courseId);
        Task<List<Enrollment>> GetEnrollmentsByStudentAsync(string studentId);
        Task<Enrollment> GetEnrollmentByCourseAndStudentAsync(string courseId, string studentId);
        Task<string> CreateEnrollmentAsync(Enrollment enrollment);
        Task UpdateEnrollmentStatusAsync(string id, EnrollmentStatus status);
        Task DeleteEnrollmentAsync(string id);
        Task UpdateEnrollmentAsync(Enrollment enrollment);

    }
}
