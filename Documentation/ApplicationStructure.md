# Training API Application Structure

This document provides an overview of the Training API application architecture, including its components, organization, and the relationships between different parts of the system.

## Table of Contents
- [Overview](#overview)
- [Project Structure](#project-structure)
- [Key Components](#key-components)
- [Data Flow](#data-flow)
- [Repository Pattern](#repository-pattern)
- [API Endpoints Organization](#api-endpoints-organization)

## Overview

The Training API is a RESTful service built with ASP.NET Core that provides backend functionality for a learning management system. It allows for course management, student enrollments, and user administration with different role-based permissions.

The application follows a layered architecture:
- **Controllers Layer**: Handles HTTP requests and responses
- **Service Layer**: Contains business logic and operations
- **Repository Layer**: Manages data access and persistence
- **Models Layer**: Defines data structures and entities

## Project Structure

## Key Components

### Controllers
Controllers handle HTTP requests and define the API endpoints. Each controller is responsible for a specific domain:

- **AuthController**: Manages user registration and authentication
- **UserController**: Handles user profile operations
- **AdminController**: Provides admin-specific functionality
- **TeacherController**: Handles teacher operations including course management
- **StudentController**: Manages student-specific operations like enrollment
- **SetupController**: Handles initial system setup

### Models
Models represent the data structures used in the application:

- **User**: Base class for all user types with common properties
- **Student**: Extends User with student-specific properties
- **Teacher**: Extends User with teacher-specific properties
- **AdminUser**: Extends User with admin-specific properties
- **Course**: Represents a course with its details and lectures
- **Lecture**: Represents a lecture session within a course
- **Enrollment**: Represents a student's enrollment in a course

### Repositories
Repositories abstract the data access layer:

- **IUserRepository/UserRepository**: Manages user data
- **ICourseRepository/CourseRepository**: Manages course data
- **IEnrollmentRepository/EnrollmentRepository**: Manages enrollment data

### Services
Services implement business logic and external integrations:

- **FirebaseService**: Provides Firebase initialization and configuration
- **FirebaseAuthService**: Handles Firebase authentication operations

## Data Flow

1. **HTTP Request**: Client sends an HTTP request to an API endpoint
2. **Authentication**: Request is authenticated by the FirebaseAuthenticationHandler
3. **Controller**: Appropriate controller receives the request
4. **Authorization**: Controller verifies the user has permission for the requested operation
5. **Service/Repository**: Controller calls service or repository methods to perform business logic
6. **Data Access**: Repository handles data retrieval or modification
7. **Response**: Controller formats and returns the HTTP response

## Repository Pattern

The application uses the Repository pattern to abstract data access:

1. **Interface Definition**: Each repository has an interface that defines available operations
2. **Implementation**: Concrete repository classes implement these interfaces
3. **Dependency Injection**: Controllers receive repository instances via constructor injection
4. **Testing**: This pattern facilitates unit testing by allowing repository mocking

Example:
```csharp
public interface IUserRepository 
{
    Task<User> GetUserAsync(string id);
    Task<string> CreateUserAsync(User user);
    // ...other methods...
}

public class UserRepository : IUserRepository 
{
    // Implementation...
}

