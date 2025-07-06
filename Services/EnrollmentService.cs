// using System.Threading.Tasks;
// using Google.Cloud.Firestore;
// using TrainingApi.Models;

// namespace TrainingApi.Services
// {
//     public class EnrollmentService : IEnrollmentService
//     {
//         private readonly FirestoreDb _firestoreDb;
//         private readonly string _collectionName = "enrollments";

//         public EnrollmentService(FirestoreDb firestoreDb)
//         {
//             _firestoreDb = firestoreDb;
//         }

//         public async Task<Enrollment> GetEnrollmentAsync(string id)
//         {
//             DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);
//             DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

//             if (snapshot.Exists)
//             {
//                 return snapshot.ConvertTo<Enrollment>();
//             }

//             return null;
//         }

//         public async Task UpdateEnrollmentAsync(Enrollment enrollment)
//         {
//             DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(enrollment.Id);
//             await docRef.SetAsync(enrollment, SetOptions.MergeAll);
//         }
//     }
// }
