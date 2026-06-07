

# Scalable .NET 10 CRM API

## About

This project is a scalable CRM API built with .NET 10, featuring secure authentication, role-based authorization, distributed caching, and production-ready deployment. It is designed with modern backend engineering principles including performance optimization, security best practices, and cloud-native architecture.

---

## Key Features

* JWT authentication with access and refresh token support
* Role-based authorization (Admin, Manager, User, etc.)
* Redis distributed caching for high-performance data access
* Refresh token storage with secure rotation strategy
* Rate limiting for authentication and sensitive endpoints
* Structured logging and centralized error handling
* Clean architecture principles (separation of concerns)
* RESTful API design

---

## Tech Stack

* .NET 10 Web API
* Entity Framework Core
* PostgreSQL via Supabase and localhost
* Redis (distributed caching & rate limiting)
* JWT (System.IdentityModel.Tokens.Jwt)
* Docker (optional deployment support)
* CI/CD pipelines (GitHub Actions or similar)
* Cloud hosting via Render

---

## Architecture Overview

The system follows a layered architecture:

* Controllers: Handle HTTP requests
* Services: Business logic layer
* Data Access Layer: Entity Framework Core
* Cache Layer: Redis via IDistributedCache / StackExchange.Redis
* Authentication Layer: JWT + Refresh Token system

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

* Build and test on every push
* Automatic deployment to Render
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

* Controllers: API endpoints
* Services: Business logic implementation
* Models: Data transfer objects and entities
* Data: DbContext and EF Core configuration
* Caching: Redis services and helpers
* Auth: JWT and refresh token logic
* Middleware: Exception handling and rate limiting

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

### Run Locally

```bash
dotnet restore
dotnet build
dotnet watch or dotnet run
```

---

### Database Migration

```bash
dotnet ef database update
```

---

## API Endpoints Overview

* Auth

  * POST /api/auth/login
  * POST /api/auth/register
  * POST /api/auth/refresh-token

* Users

  * GET /api/users
  * GET /api/users/{id}
  * PUT /api/users/{id}

* Admin

  * Role-protected management endpoints

---

## Future Improvements

* Event-driven architecture using message queues
* Full Redis-based session management
* Microservices decomposition
* API gateway integration
* Advanced audit logging system
* Multi-factor authentication (MFA)

---

## License

This project is intended for educational and production learning purposes.
