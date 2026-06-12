# Scalable .NET 10 CRM API

[![CI/CD Pipeline](https://github.com/seigodwin/crm-api/actions/workflows/main.yml/badge.svg)](https://github.com/seigodwin/crm-api/actions/workflows/main.yml)
[![Deployed to Render](https://img.shields.io/badge/Deployed%20to-Render-black?style=flat&logo=render&logoColor=white)](https://crm-api.onrender.com)

## About

This project is a scalable CRM API built with .NET 10, featuring secure authentication, role-based authorization, distributed caching, and production-ready deployment. It is designed with modern backend engineering principles including performance optimization, security best practices, and cloud-native architecture.

---

## Key Features

* **JWT Authentication:** Access and refresh token support with secure token rotation strategy.
* **Role-Based Authorization:** Fine-grained access control (Admin, Manager, User, etc.).
* **Redis Distributed Caching:** High-performance data access and session tracking.
* **Rate Limiting:** Redis-backed brute-force prevention on sensitive endpoints.
* **Structured Logging:** Centralized logging with context enrichment, optimized for deep observability.
* **Robust Automated Testing Suite:** Comprehensive unit testing isolated via mocking to ensure codebase reliability.
* **Clean Architecture Principles:** Strict separation of concerns and robust error-handling middleware.
* **RESTful API Design:** Predictable, resource-oriented endpoint architecture.

---

## Tech Stack

### Backend & Core
* .NET 10 Web API
* Entity Framework Core
* PostgreSQL via Supabase and localhost
* Redis (distributed caching & rate limiting)
* JWT (`System.IdentityModel.Tokens.Jwt`)

### Logging & Diagnostics
* **Serilog:** Rich, asynchronous structured logging abstraction.
* **Seq:** Centralized developer dashboard for real-time log ingestion, querying, and analysis.

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

* **Controllers:** Handle HTTP requests and entry-point validation.
* **Services:** Core business logic layer.
* **Data Access Layer:** Entity Framework Core interacting with PostgreSQL.
* **Cache Layer:** Redis abstraction via `IDistributedCache` / `StackExchange.Redis`.
* **Logging & Auth Layer:** Serilog pipeline alongside JWT + Refresh Token logic.
* **Testing Layer:** Isolated test projects mirroring the application structure to validate components independently.

---

## Authentication Flow

1. User logs in with credentials.
2. Server validates credentials.
3. Access token (JWT) is generated.
4. Refresh token is generated and stored securely.
5. Access token is used for API requests.
6. Refresh token is used to obtain new access tokens.

---

## Redis Usage

Redis is used for:

* Caching frequently accessed data.
* Rate limiting (login, password reset, OTP requests).
* Refresh token storage (optional or hybrid with database).
* Session tracking and security enforcement.

---

## Rate Limiting Strategy

The API implements Redis-based rate limiting to protect sensitive endpoints:

* Login attempts
* Password reset requests
* OTP/email verification endpoints

Each request is tracked using a time-based counter stored in Redis with automatic expiration.

---

## Structured Logging Architecture

The API uses **Serilog** configured with asynchronous processing pipelines to guarantee non-blocking logging execution. 

* **Local Development:** Logs are simultaneously streamed to the system terminal and formatted as rich, queryable JSON dispatched via background threads to a local **Seq** Docker container.
* **Production (Render):** System logs are written directly to standard output (`Console`), where Render's native log aggregators stream live operational metrics without forcing relational database writes.

---

## Security Features

* Password hashing using ASP.NET Identity.
* JWT signing with secure symmetric keys.
* Token validation with explicit issuer and audience checks.
* Refresh token rotation and reuse protection.
* Rate limiting for brute-force prevention.
* Secure secret management via environment variables.

---

## CI/CD Pipeline

The project includes automated deployment workflows:

* **Automated QA:** Runs `dotnet test` on every push and pull request to ensure zero regressions before deployment.
* Automatic deployment to Render upon successful test runs.
* Environment-based configuration handling.
* Secure handling of secrets via CI/CD variables.

---

## Cloud Deployment

* Backend hosted on Render.
* Database hosted on Supabase (PostgreSQL).
* Redis instance used for caching and rate limiting.
* Environment variables configured for production and staging.

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
* Docker Desktop (for running background services)
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

#### 1. Spin up Required Infrastructure (Redis & Seq)
To initialize your local caching, rate-limiting, and centralized logging UI dashboard, launch the service infrastructure via Docker Desktop or run:

```bash
# Start Seq Logger Dashboard
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:5341 -p 8081:80 datalust/seq:latest


# Start Redis Cache Server (if not running locally)
docker run --name redis-crm -d -p 6379:6379 redis:alpine
