using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using TrainingApi.Models;

namespace TrainingApi.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _coursesCollection = "courses";

        public CourseRepository(IConfiguration configuration)
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

        public async Task<Course> GetCourseAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_coursesCollection).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Course>();
            }

            return null;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            Query query = _firestoreDb.Collection(_coursesCollection);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Course> courses = new List<Course>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                courses.Add(documentSnapshot.ConvertTo<Course>());
            }

            return courses;
        }

        public async Task<List<Course>> GetCoursesByTeacherAsync(string teacherId)
        {
            Query query = _firestoreDb.Collection(_coursesCollection)
                .WhereEqualTo("TeacherId", teacherId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Course> courses = new List<Course>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                courses.Add(documentSnapshot.ConvertTo<Course>());
            }

            return courses;
        }

        public async Task<List<Course>> GetActiveCoursesAsync()
        {
            Query query = _firestoreDb.Collection(_coursesCollection)
                .WhereEqualTo("IsActive", true);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<Course> courses = new List<Course>();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                courses.Add(documentSnapshot.ConvertTo<Course>());
            }

            return courses;
        }

        public async Task<string> CreateCourseAsync(Course course)
        {
            course.Id = Guid.NewGuid().ToString();
            course.CreatedAt = DateTime.UtcNow;
            // Convert EndDate to UTC if it's not already
            if (course.EndDate.Kind != DateTimeKind.Utc)
            {
                course.EndDate = course.EndDate.ToUniversalTime();
            }
             if (course.StartDate.Kind != DateTimeKind.Utc)
            {
                course.StartDate = course.StartDate.ToUniversalTime();
            }
            course.UpdatedAt = DateTime.UtcNow;

            DocumentReference docRef = _firestoreDb.Collection(_coursesCollection).Document(course.Id);
            await docRef.SetAsync(course);

            return course.Id;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            course.UpdatedAt = DateTime.UtcNow;

             if (course.EndDate.Kind != DateTimeKind.Utc)
            {
                course.EndDate = course.EndDate.ToUniversalTime();
            }
             if (course.StartDate.Kind != DateTimeKind.Utc)
            {
                course.StartDate = course.StartDate.ToUniversalTime();
            }

            DocumentReference docRef = _firestoreDb.Collection(_coursesCollection).Document(course.Id);
            await docRef.SetAsync(course, SetOptions.MergeAll);
        }

        public async Task AddLectureAsync(string courseId, Lecture lecture)
        {
            var course = await GetCourseAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }
             if (course.EndDate.Kind != DateTimeKind.Utc)
            {
                course.EndDate = course.EndDate.ToUniversalTime();
            }
             if (course.StartDate.Kind != DateTimeKind.Utc)
            {
                course.StartDate = course.StartDate.ToUniversalTime();
            }

            course.Lectures.Add(lecture);
            course.UpdatedAt = DateTime.UtcNow;

            await UpdateCourseAsync(course);
        }

        public async Task UpdateLectureAsync(string courseId, Lecture lecture)
        {
            var course = await GetCourseAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var lectureIndex = course.Lectures.FindIndex(l => l.Id == lecture.Id);
            if (lectureIndex == -1)
            {
                throw new Exception("Lecture not found");
            }

            course.Lectures[lectureIndex] = lecture;
            course.UpdatedAt = DateTime.UtcNow;
             if (course.EndDate.Kind != DateTimeKind.Utc)
            {
                course.EndDate = course.EndDate.ToUniversalTime();
            }
             if (course.StartDate.Kind != DateTimeKind.Utc)
            {
                course.StartDate = course.StartDate.ToUniversalTime();
            }
            await UpdateCourseAsync(course);
        }

        public async Task DeleteLectureAsync(string courseId, string lectureId)
        {
            var course = await GetCourseAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var lectureIndex = course.Lectures.FindIndex(l => l.Id == lectureId);
            if (lectureIndex == -1)
            {
                throw new Exception("Lecture not found");
            }

            course.Lectures.RemoveAt(lectureIndex);
            course.UpdatedAt = DateTime.UtcNow;
 if (course.EndDate.Kind != DateTimeKind.Utc)
            {
                course.EndDate = course.EndDate.ToUniversalTime();
            }
             if (course.StartDate.Kind != DateTimeKind.Utc)
            {
                course.StartDate = course.StartDate.ToUniversalTime();
            }
            await UpdateCourseAsync(course);
        }

        public async Task DeleteCourseAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_coursesCollection).Document(id);
            await docRef.DeleteAsync();
        }
    }

    public interface ICourseRepository
    {
        Task<Course> GetCourseAsync(string id);
        Task<List<Course>> GetAllCoursesAsync();
        Task<List<Course>> GetCoursesByTeacherAsync(string teacherId);
        Task<List<Course>> GetActiveCoursesAsync();
        Task<string> CreateCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task AddLectureAsync(string courseId, Lecture lecture);
        Task UpdateLectureAsync(string courseId, Lecture lecture);
        Task DeleteLectureAsync(string courseId, string lectureId);
        Task DeleteCourseAsync(string id);
    }
}
