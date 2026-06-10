# Scalable .NET 10 CRM API

[![CI/CD Pipeline](https://github.com/seigodwin/crm-api/actions/workflows/main.yml/badge.svg)](https://github.com/seigodwin/crm-api/actions/workflows/main.yml)

## About

This project is a scalable CRM API built with .NET 10, featuring secure authentication, role-based authorization, distributed caching, and production-ready deployment. It is designed with modern backend engineering principles including performance optimization, security best practices, and cloud-native architecture.

---

## Key Features

* JWT authentication with access and refresh token support
* Role-based authorization (Admin, Manager, User, etc.)
* Redis distributed caching for high-performance data access
* Refresh token storage with secure rotation strategy
* Rate limiting for authentication and sensitive endpoints
* **Robust Automated Testing Suite:** Comprehensive unit testing isolated via mocking to ensure codebase reliability.
* Structured logging and centralized error handling
* Clean architecture principles (separation of concerns)
* RESTful API design

---

## Tech Stack

### Backend & Core
* .NET 10 Web API
* Entity Framework Core
* PostgreSQL via Supabase and localhost
* Redis (distributed caching & rate limiting)
* JWT (`System.IdentityModel.Tokens.Jwt`)

### Testing Suite
* **xUnit:** Core test framework for structuring and executing unit tests.
* **FluentAssertions:** Natural-language, highly readable assertion engine.
* **Moq:** Dependency mocking framework utilized to isolate business logic during testing.

### DevOps & Cloud
* Docker (optional deployment support)
* CI/CD pipelines (GitHub Actions automated build & test execution)
* Cloud hosting via Render

---

## Architecture Overview

The system follows a layered architecture:

* Controllers: Handle HTTP requests
* Services: Business logic layer
* Data Access Layer: Entity Framework Core
* Cache Layer: Redis via `IDistributedCache` / `StackExchange.Redis`
* Authentication Layer: JWT + Refresh Token system
* **Testing Layer:** Isolated test projects mirroring the application structure to validate services and utilities independently.

---

## Authentication Flow

1. User logs in with credentials
2. Server validates credentials
3. Access token (JWT) is generated
4. Refresh token is generated and stored securely
5. Access token is used for API requests
6. Refresh token is used to obtain new access tokens

---

## Redis Usage

Redis is used for:

* Caching frequently accessed data
* Rate limiting (login, password reset, OTP requests)
* Refresh token storage (optional or hybrid with database)
* Session tracking and security enforcement

---

## Rate Limiting Strategy

The API implements Redis-based rate limiting to protect sensitive endpoints:

* Login attempts
* Password reset requests
* OTP/email verification endpoints

Each request is tracked using a time-based counter stored in Redis with automatic expiration.

---

## Security Features

* Password hashing using ASP.NET Identity
* JWT signing with secure symmetric key
* Token validation with issuer and audience checks
* Refresh token rotation and reuse protection
* Rate limiting for brute-force prevention
* Secure secret management via environment variables

---

## CI/CD Pipeline

The project includes automated deployment workflows:

* **Automated QA:** Runs `dotnet test` on every push and pull request to ensure zero regressions before deployment.
* Automatic deployment to Render upon successful test runs.
* Environment-based configuration handling
* Secure handling of secrets via CI/CD variables

---

## Cloud Deployment

* Backend hosted on Render
* Database hosted on Supabase (PostgreSQL)
* Redis instance used for caching and rate limiting
* Environment variables configured for production and staging

---

## Project Structure

* `Controllers/`: API endpoints
* `Services/`: Business logic implementation
* `Models/`: Data transfer objects and entities
* `Data/`: DbContext and EF Core configuration
* `Caching/`: Redis services and helpers
* `Auth/`: JWT and refresh token logic
* `Middleware/`: Exception handling and rate limiting
* **`CRMApi.Tests/`:** Unit tests utilizing xUnit, FluentAssertions, and Moq to validate business logic in isolation.

---

## Setup Instructions

### Prerequisites

* .NET 10 SDK
* Redis instance (local or cloud)
* PostgreSQL database (Supabase recommended)

---

### Configuration

Update `.env` copy .env variables from `.env.example`:

* Database connection string
* Redis connection string
* JWT secret key
* JWT issuer and audience

---

### Run Locally & Testing

#### Running the Application
```bash
dotnet restore
dotnet build
dotnet watch # or dotnet run
