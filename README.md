# Training API Platform

A comprehensive API platform for managing courses, teachers, students, and enrollments.

## Overview

Training API is a robust ASP.NET Core application designed to facilitate course management, teacher-student interactions, and educational administration. The system implements role-based access control with three primary roles: Admin, Teacher, and Student, each with specific permissions and capabilities.

## Features

- **User Management**
  - Role-based authentication with Firebase
  - Admin, Teacher, and Student roles
  - User registration and login

- **Course Management**
  - Course creation and scheduling
  - Lecture management
  - Enrollment processing

- **Role-Specific Features**
  - **Admin**: User management, teacher approval, system administration
  - **Teacher**: Course creation, lecture scheduling, enrollment approval
  - **Student**: Course browsing, enrollment requests, schedule viewing

## Architecture

The application follows a clean architecture pattern with:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Implement business logic
- **Repositories**: Manage data access
- **Models**: Define domain entities

## Technical Stack

- **Backend**: ASP.NET Core 6.0
- **Authentication**: Firebase Authentication with JWT tokens
- **Database**: Firebase Firestore
- **API Documentation**: Swagger/OpenAPI

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Firebase account with Firestore and Authentication enabled
- Firebase Admin SDK credentials

### Configuration

1. Clone the repository
2. Configure Firebase credentials in `appsettings.json`
3. Add your Firebase service account key file
4. Update the Firebase project ID in configuration

```json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "CredentialFile": "firebase-credentials.json"
  },
  "AdminSetup": {
    "InitialSetupKey": "SecureSetupKey123456"
  }
}
```

### Initial Setup

1. Run the application
2. Use the `/api/setup/create-first-admin` endpoint to create the first admin user
3. Log in as admin to manage the system

## API Endpoints

### Authentication

- **POST /api/auth/register/student**: Register a new student
- **POST /api/auth/register/teacher**: Register as a teacher (requires approval)
- **POST /api/auth/login**: Standard user login
- **POST /api/auth/admin/login**: Admin login with additional verification

### Admin Routes

- **GET /api/admin/users**: List all users
- **GET /api/admin/users/{id}**: Get user details
- **POST /api/admin/users**: Create a new user
- **PUT /api/admin/users/{id}/role**: Update user role
- **DELETE /api/admin/users/{id}**: Delete a user
- **GET /api/admin/pending-teachers**: List teacher accounts awaiting approval
- **PUT /api/admin/teachers/{id}/approve**: Approve a teacher
- **PUT /api/admin/teachers/{id}/reject**: Reject a teacher

### Teacher Routes

- **GET /api/teacher/courses**: List teacher's courses
- **GET /api/teacher/courses/{id}**: Get course details
- **POST /api/teacher/courses**: Create a new course
- **PUT /api/teacher/courses/{id}**: Update course details
- **POST /api/teacher/courses/{courseId}/lectures**: Add a lecture
- **PUT /api/teacher/courses/{courseId}/lectures/{lectureId}**: Update a lecture
- **DELETE /api/teacher/courses/{courseId}/lectures/{lectureId}**: Delete a lecture
- **GET /api/teacher/courses/{courseId}/enrollments**: List enrollments for a course
- **PUT /api/teacher/enrollments/{enrollmentId}/status**: Update enrollment status

### Student Routes

- **GET /api/student/courses**: List available courses
- **GET /api/student/courses/{id}**: Get course details
- **POST /api/student/courses/{courseId}/enroll**: Request enrollment
- **GET /api/student/enrollments**: List student's enrollment requests
- **GET /api/student/enrollments/{enrollmentId}**: Check enrollment status
- **GET /api/student/courses/enrolled**: List enrolled courses

## Data Models

### User Model

Base user model with role-specific extensions:

```csharp
public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}
```

### Course Model

```csharp
public class Course
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string TeacherId { get; set; }
    public string TeacherName { get; set; }
    public int MaxEnrollment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Lecture> Lectures { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

### Enrollment Model

```csharp
public class Enrollment
{
    public string Id { get; set; }
    public string CourseId { get; set; }
    public string StudentId { get; set; }
    public string StudentName { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
```

## Authentication Flow

1. **User Registration**:
   - User registers with email/password
   - Firebase creates account with role-specific claims
   - User data is stored in Firestore

2. **Login Process**:
   - User authenticates with Firebase
   - Backend validates Firebase JWT token
   - Role-specific authorization is enforced

3. **Role-Based Access**:
   - JWT token contains role claims
   - API endpoints check roles via [Authorize] attributes
   - Custom middleware extracts and processes Firebase claims

## Security

- Firebase JWT token authentication
- Role-based access control
- Admin-only routes and operations
- Teacher approval workflow
- Secure admin login with additional verification

## Project Structure

```
/TrainingApi
|-- Controllers/
|   |-- AdminController.cs
|   |-- AuthController.cs
|   |-- StudentController.cs
|   |-- TeacherController.cs
|   |-- SetupController.cs
|   |-- UserController.cs
|
|-- Models/
|   |-- User.cs
|   |-- AdminUser.cs
|   |-- Teacher.cs
|   |-- Student.cs
|   |-- Course.cs
|   |-- Enrollment.cs
|
|-- Repositories/
|   |-- UserRepository.cs
|   |-- CourseRepository.cs
|   |-- EnrollmentRepository.cs
|
|-- Services/
|   |-- FirebaseAuthService.cs
|
|-- Auth/
|   |-- FirebaseAuthenticationHandler.cs
|
|-- Program.cs
|-- Startup.cs
|-- appsettings.json
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Firebase Authentication for secure user management
- ASP.NET Core for robust API development
- Firestore for flexible document database storage
