# Creating Admin Users

This document explains how to create administrator users for the Training API. Admin users have full access to the system, including user management, course approval, and other administrative functions.

## Methods to Create Admin Users

There are two primary ways to create admin users:

1. Using the Command-Line Tool (recommended for initial setup)
2. Using the Admin API (when you already have an admin account)

## 1. Using the Command-Line Tool

The project includes a command-line tool for creating the first admin user directly, without requiring an existing admin account.

### Prerequisites

- Access to Firebase project credentials
- .NET SDK installed
- Project files checked out locally

### Steps

1. Navigate to the project directory:
   ```bash
   cd /Users/macair/Desktop/hw/training-api
   ```

2. Run the admin creation tool with appropriate parameters:
   ```bash
   dotnet run --project Tools/AdminTools.csproj -- --email admin@example.com --password SecureP@ss123 --name "Admin User" --super
   ```

   Parameters:
   - `--email`: Email address for the admin (required)
   - `--password`: Password for the admin account (required)
   - `--name`: Display name for the admin (optional)
   - `--super`: Flag to make this a super admin who can create other admins (optional)

3. The tool will:
   - Create a Firebase Authentication account
   - Set the appropriate custom claims for admin role
   - Create a record in the Firestore database
   - Display the results including the new user's UID

Example output:
