# Training API Test Scenarios

This document outlines the test scenarios for the Training API controllers. These scenarios should be used to validate the functionality of the API endpoints during development and QA testing.

## Table of Contents
- [Auth Controller](#auth-controller)
- [User Controller](#user-controller)
- [Admin Controller](#admin-controller)
- [Teacher Controller](#teacher-controller)
- [Student Controller](#student-controller)
- [Setup Controller](#setup-controller)

## Auth Controller

### Student Registration
1. **Valid Registration**
   - Request: POST `/api/auth/register/student` with valid email, password, display name, and year
   - Expected: 200 OK with user ID and success message
   - Verify: User exists in database with Student role

2. **Invalid Registration - Missing Fields**
   - Request: POST `/api/auth/register/student` with missing email or password
   - Expected: 400 Bad Request with error message

3. **Invalid Registration - Duplicate Email**
   - Request: POST `/api/auth/register/student` with an email that already exists
   - Expected: 400 Bad Request with error message about duplicate email

### Teacher Registration
1. **Valid Registration**
   - Request: POST `/api/auth/register/teacher` with valid email, password, display name, department, and qualification
   - Expected: 200 OK with user ID and success message about pending approval
   - Verify: User exists in database with Teacher role and Pending approval status

2. **Invalid Registration - Missing Fields**
   - Request: POST `/api/auth/register/teacher` with missing department or qualification
   - Expected: 400 Bad Request with error message

### User Login
1. **Valid Student Login**
   - Request: POST `/api/auth/login` with valid Firebase ID token for a student account
   - Expected: 200 OK with user object and role "Student"
   - Verify: LastLogin timestamp is updated

2. **Valid Teacher Login**
   - Request: POST `/api/auth/login` with valid Firebase ID token for a teacher account
   - Expected: 200 OK with user object and role "Teacher"
   - Verify: LastLogin timestamp is updated

3. **Invalid Login - Bad Token**
   - Request: POST `/api/auth/login` with invalid or expired token
   - Expected: 400 Bad Request with error message

4. **User Not Found**
   - Request: POST `/api/auth/login` with valid token for non-existent user
   - Expected: 404 Not Found with error message

### Admin Login
1. **Valid Admin Login**
   - Request: POST `/api/auth/admin/login` with valid Firebase ID token and correct admin code
   - Expected: 200 OK with user object, role "Admin", and isAdmin flag
   - Verify: LastLogin timestamp is updated

2. **Invalid Admin Login - Wrong Code**
   - Request: POST `/api/auth/admin/login` with valid token but incorrect admin code
   - Expected: 400 Bad Request with error about invalid verification code

3. **Invalid Admin Login - Not Admin**
   - Request: POST `/api/auth/admin/login` with valid token for a non-admin user
   - Expected: 403 Forbidden

## User Controller

### Get Current User
1. **Get Student Profile**
   - Request: GET `/api/user/me` with valid auth token for a student
   - Expected: 200 OK with user profile and role information
   - Verify: Response contains student-specific fields

2. **Get Teacher Profile**
   - Request: GET `/api/user/me` with valid auth token for a teacher
   - Expected: 200 OK with user profile, role, approval status, and isApproved flag
   - Verify: Response contains teacher-specific fields

3. **Get Admin Profile**
   - Request: GET `/api/user/me` with valid auth token for an admin
   - Expected: 200 OK with user profile and role information
   - Verify: Response contains admin-specific fields

4. **Invalid Token**
   - Request: GET `/api/user/me` with invalid or expired token
   - Expected: 401 Unauthorized

5. **User Not Found**
   - Request: GET `/api/user/me` with valid token for deleted user
   - Expected: 404 Not Found

## Admin Controller

### User Management
1. **Get All Users**
   - Request: GET `/api/admin/users` with valid admin token
   - Expected: 200 OK with list of all users
   - Verify: List contains users of all roles

2. **Get User By ID**
   - Request: GET `/api/admin/users/{id}` with valid admin token and existing user ID
   - Expected: 200 OK with user details
   - Verify: Contains complete user information

3. **Create User**
   - Request: POST `/api/admin/users` with valid admin token and new user data
   - Expected: 200 OK with created user
   - Verify: User exists in database with specified role

4. **Update User Role**
   - Request: PUT `/api/admin/users/{id}/role` with valid admin token and role update
   - Expected: 200 OK with updated user
   - Verify: User's role is changed in database

5. **Delete User**
   - Request: DELETE `/api/admin/users/{id}` with valid admin token
   - Expected: 200 OK with success message
   - Verify: User is removed from database

6. **Access Denied**
   - Request: Any admin endpoint with non-admin token
   - Expected: 403 Forbidden

### Teacher Approval
1. **Get Pending Teachers**
   - Request: GET `/api/admin/pending-teachers` with valid admin token
   - Expected: 200 OK with list of teachers with pending approval
   - Verify: All returned teachers have Pending status

2. **Approve Teacher**
   - Request: PUT `/api/admin/teachers/{id}/approve` with valid admin token
   - Expected: 200 OK with success message
   - Verify: Teacher's approval status is updated to Approved

3. **Reject Teacher**
   - Request: PUT `/api/admin/teachers/{id}/reject` with valid admin token
   - Expected: 200 OK with success message
   - Verify: Teacher's approval status is updated to Rejected

## Teacher Controller

### Approval Status
1. **Get Approval Status - Pending**
   - Request: GET `/api/teacher/approval-status` with valid token for pending teacher
   - Expected: 200 OK with status "Pending" and appropriate message
   - Verify: Status matches teacher's current status

2. **Get Approval Status - Approved**
   - Request: GET `/api/teacher/approval-status` with valid token for approved teacher
   - Expected: 200 OK with status "Approved" and appropriate message
   - Verify: Status matches teacher's current status

3. **Get Approval Status - Rejected**
   - Request: GET `/api/teacher/approval-status` with valid token for rejected teacher
   - Expected: 200 OK with status "Rejected" and appropriate message
   - Verify: Status matches teacher's current status

### Course Management
1. **Get Teacher Courses - Approved Teacher**
   - Request: GET `/api/teacher/courses` with valid token for approved teacher
   - Expected: 200 OK with list of teacher's courses
   - Verify: All returned courses belong to the authenticated teacher

2. **Get Teacher Courses - Unapproved Teacher**
   - Request: GET `/api/teacher/courses` with valid token for pending/rejected teacher
   - Expected: 400 Bad Request with message about pending approval
   - Verify: Response includes current approval status

3. **Get Course Details**
   - Request: GET `/api/teacher/courses/{id}` with valid token for approved teacher and owned course
   - Expected: 200 OK with course details
   - Verify: Course belongs to the authenticated teacher

4. **Access Denied - Not Owner**
   - Request: GET `/api/teacher/courses/{id}` with valid token but for a course not owned by teacher
   - Expected: 403 Forbidden

5. **Create Course**
   - Request: POST `/api/teacher/courses` with valid token for approved teacher and course data
   - Expected: 200 OK with course ID and created course
   - Verify: Course exists in database with teacher as owner

6. **Update Course**
   - Request: PUT `/api/teacher/courses/{id}` with valid token and course updates
   - Expected: 200 OK with updated course
   - Verify: Course details are updated in database

### Lecture Management
1. **Add Lecture**
   - Request: POST `/api/teacher/courses/{courseId}/lectures` with valid token and lecture data
   - Expected: 200 OK with created lecture
   - Verify: Lecture is added to the course

2. **Update Lecture**
   - Request: PUT `/api/teacher/courses/{courseId}/lectures/{lectureId}` with valid token and updates
   - Expected: 200 OK with updated lecture
   - Verify: Lecture details are updated

3. **Delete Lecture**
   - Request: DELETE `/api/teacher/courses/{courseId}/lectures/{lectureId}` with valid token
   - Expected: 200 OK with success message
   - Verify: Lecture is removed from course

### Enrollment Management
1. **Get Course Enrollments**
   - Request: GET `/api/teacher/courses/{courseId}/enrollments` with valid token
   - Expected: 200 OK with list of enrollments for the course
   - Verify: All enrollments belong to the specified course

2. **Update Enrollment Status**
   - Request: PUT `/api/teacher/enrollments/{enrollmentId}/status` with valid token and new status
   - Expected: 200 OK with success message
   - Verify: Enrollment status is updated
   - If approved, verify student's enrolled courses list is updated

3. **Set Grade**
   - Request: POST `/api/teacher/set-grade` with valid token and grade data
   - Expected: 200 OK with success message
   - Verify: Grade is updated in the enrollment record

## Student Controller

### Course Browsing
1. **Get Available Courses**
   - Request: GET `/api/student/courses` with valid student token
   - Expected: 200 OK with list of active courses
   - Verify: All returned courses have isActive=true

2. **Get Course Details**
   - Request: GET `/api/student/courses/{id}` with valid student token
   - Expected: 200 OK with course details
   - Verify: Complete course information is returned

### Enrollment Management
1. **Get Student Enrollments**
   - Request: GET `/api/student/enrollments` with valid student token
   - Expected: 200 OK with list of student's enrollments
   - Verify: All enrollments belong to the authenticated student

2. **Enroll in Course - Success**
   - Request: POST `/api/student/courses/{courseId}/enroll` with valid student token
   - Expected: 200 OK with enrollment ID and success message
   - Verify: Enrollment record is created with Pending status

3. **Enroll in Course - Already Enrolled**
   - Request: POST `/api/student/courses/{courseId}/enroll` for a course already enrolled in
   - Expected: 400 Bad Request with message about existing enrollment
   - Verify: No duplicate enrollment is created

4. **Enroll in Course - Full Course**
   - Request: POST `/api/student/courses/{courseId}/enroll` for a course at max capacity
   - Expected: 400 Bad Request with message about capacity
   - Verify: No enrollment is created

5. **Get Enrollment Status**
   - Request: GET `/api/student/enrollments/{enrollmentId}` with valid student token
   - Expected: 200 OK with enrollment and course details
   - Verify: Enrollment belongs to the authenticated student

6. **Get Enrolled Courses**
   - Request: GET `/api/student/courses/enrolled` with valid student token
   - Expected: 200 OK with list of enrolled courses
   - Verify: All courses in the student's enrolledCourses list are returned

## Setup Controller

1. **Create First Admin - Success**
   - Request: POST `/api/setup/create-first-admin` with valid setup key and admin data
   - Expected: 200 OK with user ID, admin code, and success message
   - Verify: Admin user is created with superAdmin=true
   - Verify: Cannot create another admin using this endpoint

2. **Create First Admin - Invalid Key**
   - Request: POST `/api/setup/create-first-admin` with invalid setup key
   - Expected: 400 Bad Request with error about invalid key

3. **Create First Admin - Admin Already Exists**
   - Request: POST `/api/setup/create-first-admin` after an admin has been created
   - Expected: 400 Bad Request with error message about admin already existing