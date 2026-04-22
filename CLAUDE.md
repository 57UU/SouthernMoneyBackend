# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SouthernMoneyBackend is a .NET 10 ASP.NET Core Web API with a three-layer architecture:
- **SouthernMoneyBackend** (API layer) - Controllers, middleware, Program.cs entry point
- **Service** (Business logic layer) - Services handling business logic
- **Database** (Data access layer) - EF Core DbContext, entity definitions, repositories

## Build & Run Commands

```bash
# Run the API
dotnet run --project SouthernMoneyBackend/SouthernMoneyBackend.csproj

# Database migrations (from repo root)
dotnet ef migrations add <MigrationName> \
  --project Database/Database.csproj \
  --startup-project SouthernMoneyBackend/SouthernMoneyBackend.csproj \
  --output-dir Migrations

dotnet ef database update \
  --project Database/Database.csproj \
  --startup-project SouthernMoneyBackend/SouthernMoneyBackend.csproj
```

## Architecture

### Dependency Injection Setup (Program.cs)
- **Repositories** (scoped): UserRepository, PostRepository, ImageRepository, ProductRepository, TransactionRepository, UserAssetRepository, ProductCategoryRepository, UserFavoriteCategoryRepository, NotificationRepository
- **Services** (scoped): UserService, PostService, ImageBedService, AdminService, ProductService, TransactionService, UserAssetService, ProductCategoryService, UserFavoriteCategoryService, NotificationService

### Data Flow
Controller → Service → Repository → DbContext → Database

### Database Layer (Database/)
- **Context.cs** - AppDbContext with all DbSet<T> properties and EF Core fluent configuration
- **Definitions.cs** - All entity classes (User, Post, Image, Product, TransactionRecord, etc.)
- **Repositories/** - Data access repositories

### Authentication (Critical)
- **Auth is DISABLED in development environment** - JWT validation does not occur
- **Auth is ENABLED in production** - All non-login endpoints require valid JWT
- Middleware order: ExceptionHandler → AuthMiddleware → Authorization → MapControllers
- Auth middleware stores user info in `HttpContext.Items["UserId"]`, `HttpContext.Items["Username"]`, `HttpContext.Items["IsAdmin"]`
- Authorization attributes: `[AuthorizeUser]`, `[AuthorizeAdmin]`, `[AuthorizeRole("RoleName")]`

### API Response Format
All endpoints return `ApiResponse<T>`:
```json
{
    "Success": true,
    "Message": null,
    "Data": { },
    "Timestamp": "2023-10-01T12:00:00Z"
}
```

### Database Support
- PostgreSQL when `--use-pg` flag is passed or via `appsettings.Secrets.json`
- SQLite fallback for development

## Key Implementation Details

### JWT Authentication
- Access token expires in 1 hour
- Refresh token expires in 7 days
- Refresh token has `token_type: "refresh"` claim
- Roles: "Admin", "User"
- `JwtUtils.cs` in Service project handles token generation/validation

### User Authorization Attributes (Middleware/)
- `AuthorizeUserAttribute` - Requires logged-in user
- `AuthorizeAdminAttribute` - Requires admin role
- `AuthorizeRoleAttribute` - Custom role requirement
- `[AllowAnonymous]` - Overrides controller-level auth

### Current User Access
```csharp
var userId = (long)HttpContext.Items["UserId"];
var isAdmin = (bool)HttpContext.Items["IsAdmin"];
```

### Entity Relationships
- Post ↔ Image: Many-to-many via PostImage join table
- Post ↔ Tag: Many-to-many via PostTags join table
- User → Product: One-to-many (uploader)
- User → UserAsset: One-to-one
- TransactionRecord → User: Buyer foreign key

## Swagger
Available at `/swagger` in development environment for API exploration.
