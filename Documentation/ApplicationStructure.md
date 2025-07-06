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

