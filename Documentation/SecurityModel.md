# Training API Security Model

This document details the security architecture of the Training API, including authentication, authorization, and security best practices implemented in the system.

## Table of Contents
- [Authentication Flow](#authentication-flow)
- [Authorization Model](#authorization-model)
- [Firebase Integration](#firebase-integration)
- [JWT Token Handling](#jwt-token-handling)
- [Role-Based Access Control](#role-based-access-control)
- [Security Best Practices](#security-best-practices)
- [Environment-Specific Configurations](#environment-specific-configurations)

## Authentication Flow

The Training API uses Firebase Authentication as its identity provider, with a custom JWT authentication flow:

1. **User Registration**:
   - User submits registration data to the API (`/api/auth/register/student` or `/api/auth/register/teacher`)
   - API creates a Firebase user account via Firebase Admin SDK
   - API stores additional user profile data in its database
   - For teachers, account is created with "pending" approval status

2. **User Login (Client-Side)**:
   - User authenticates with Firebase Authentication (typically in the frontend)
   - Firebase issues a JWT (ID token) to the authenticated user

3. **API Authentication**:
   - Client includes the Firebase JWT in the Authorization header of API requests
   - Format: `Authorization: Bearer {token}`
   - FirebaseAuthenticationHandler validates the token
   - User claims including role are extracted and added to the ClaimsPrincipal

4. **Admin Login**:
   - Admins use a separate login endpoint (`/api/auth/admin/login`)
   - Requires both Firebase authentication and an admin verification code
   - Provides an additional security layer for administrative access

5. **Session Management**:
   - Firebase JWTs have a 1-hour expiration by default
   - Client is responsible for token refresh using Firebase SDK
   - Each successful login updates the LastLogin timestamp

## Authorization Model

Authorization is implemented using ASP.NET Core's policy-based authorization system:

1. **Role-Based Authorization**:
   - Controllers or actions are decorated with `[Authorize(Roles = "...")]` attributes
   - Example: `[Authorize(Roles = "Admin")]` restricts access to administrators only
   - Multiple roles can be specified when needed

2. **User Roles**:
   - **Student**: Can browse courses and manage their enrollments
   - **Teacher**: Can manage their courses and student enrollments after approval
   - **Admin**: Has system-wide administrative privileges

3. **Resource-Based Authorization**:
   - Beyond role checks, controllers perform ownership validation
   - Example: Teachers can only modify their own courses
   - Example: Students can only view their own enrollments

4. **Teacher Approval Process**:
   - Teachers register normally but start with "Pending" status
   - Administrators must approve teacher accounts
   - Unapproved teachers cannot access teacher functionality

## Firebase Integration

Firebase provides the identity and authentication infrastructure:

1. **Firebase Admin SDK**:
   - The API uses Firebase Admin SDK for server-side operations
   - Configured in FirebaseService and FirebaseAuthService

2. **User Management Operations**:
   - Create users: `CreateUserAsync`
   - Verify tokens: `VerifyIdTokenAsync`
   - Delete users: `DeleteUserAsync`
   - Set custom claims: `UpdateUserRoleAsync`

3. **Custom Claims**:
   - User roles are stored as Firebase custom claims
   - Claims are included in the JWT
   - Example: `{ "role": "Teacher" }`

4. **Configuration**:
   - Firebase project settings are stored in appsettings.json
   - Service account credentials are securely managed
   - Different Firebase projects can be used for different environments

## JWT Token Handling

Custom JWT handling is implemented to work with Firebase tokens:

1. **FirebaseAuthenticationHandler**:
   - Extends JwtBearerHandler
   - Intercepts authentication to handle Firebase-specific token validation
   - Extracts and validates Firebase ID tokens
   - Populates user claims from token and Firebase user record

2. **Token Validation Parameters**:
   - Validate token issuer (Firebase)
   - Validate audience (Firebase project ID)
   - Validate token lifetime
   - Firebase handles signature validation

3. **Error Handling**:
   - Authentication failures are logged
   - Appropriate HTTP status codes are returned
   - Minimal error information is exposed to prevent information leakage

## Role-Based Access Control

Access control is enforced at multiple levels:

1. **Controller-Level Restrictions**:
   ```csharp
   [Authorize(Roles = "Admin")]
   public class AdminController : ControllerBase
   { ... }
   ```

2. **Action-Level Checks**:
   ```csharp
   // Check if the authenticated user is the owner of the resource
   if (course.TeacherId != teacherId)
   {
       return Forbid();
   }
   ```

3. **Role-Specific Endpoints**:
   - `/api/student/...` - Student operations
   - `/api/teacher/...` - Teacher operations
   - `/api/admin/...` - Admin operations

4. **Special Workflows**:
   - Teacher approval workflow
   - Admin verification code for admin login

## Security Best Practices

The API implements several security best practices:

1. **HTTPS Enforcement**:
   - All API communication is encrypted via HTTPS
   - HTTP requests are redirected to HTTPS

2. **Cross-Origin Resource Sharing (CORS)**:
   - Configured to control which domains can access the API
   - Can be restricted to specific origins in production

3. **Input Validation**:
   - All client input is validated before processing
   - Model validation using data annotations
   - Custom validation in controller actions

4. **Error Handling**:
   - Detailed errors in development
   - Sanitized error responses in production
   - Prevents information leakage

5. **Logging**:
   - Authentication events are logged
   - Security-relevant actions are tracked
   - Helps with auditing and troubleshooting

6. **Separation of Concerns**:
   - Authentication logic is separated from business logic
   - Repository pattern isolates data access

## Environment-Specific Configurations

Security configurations can be adjusted per environment:

1. **Development Environment**:
   - Detailed error information
   - Swagger UI enabled
   - Relaxed CORS policy

2. **Production Environment**:
   - Minimal error details
   - Swagger disabled
   - Strict CORS policy
   - Secure cookie settings

3. **Configuration Management**:
   - Sensitive values stored in environment variables or secure stores
   - Different Firebase projects for development and production
   - Environment-specific settings in appsettings.{Environment}.json

## Security Considerations for Deployment

When deploying the API to production, consider these additional security measures:

1. **API Gateway**:
   - Use an API gateway for additional security layers
   - Rate limiting to prevent abuse
   - IP filtering if appropriate

2. **Monitoring**:
   - Implement security monitoring
   - Set up alerts for suspicious activities
   - Regular security audits

3. **Updates**:
   - Keep dependencies updated
   - Apply security patches promptly
   - Regular security reviews

4. **Data Protection**:
   - Ensure sensitive data is encrypted at rest
   - Apply appropriate backup and recovery procedures
   - Implement data retention policies
