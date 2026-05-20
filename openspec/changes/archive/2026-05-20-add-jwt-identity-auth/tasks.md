## 1. Dependencies and configuration

- [x] 1.1 Add NuGet packages: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.Authentication.JwtBearer`
- [x] 1.2 Add `Jwt` section to `appsettings.json` (Issuer, Audience, ExpirationMinutes; Key placeholder comment)
- [x] 1.3 Add `Jwt:Key` and `ConnectionStrings:DefaultConnection` to `appsettings.Development.json` (dev-only secret)
- [x] 1.4 Add SQLite connection string and ensure `.gitignore` excludes `*.db` if applicable

## 2. Data and models

- [x] 2.1 Create `Models/ApplicationUser.cs` extending `IdentityUser`
- [x] 2.2 Create `Data/ApplicationDbContext.cs` with `IdentityDbContext<ApplicationUser>`
- [x] 2.3 Create `Models/AuthDtos.cs` (RegisterRequest, LoginRequest, AuthResponse, RegisterResponse)
- [x] 2.4 Add initial EF Core migration and verify `dotnet ef` workflow documented in change notes if needed

## 3. JWT service

- [x] 3.1 Create `Services/IJwtTokenService.cs` and `Services/JwtTokenService.cs` to generate tokens from `ApplicationUser` and `Jwt` options
- [x] 3.2 Register `IJwtTokenService` in DI

## 4. Identity and authentication wiring

- [x] 4.1 Configure `AddDbContext`, `AddIdentity`, password options, and `AddAuthentication().AddJwtBearer()` in `Program.cs` using configuration
- [x] 4.2 Add `UseAuthentication()` and `UseAuthorization()` in correct pipeline order
- [x] 4.3 Apply database migrations on startup in Development only

## 5. API endpoints

- [x] 5.1 Create `Endpoints/AuthEndpoints.cs` with `MapAuthEndpoints` for `POST /auth/register` and `POST /auth/login`
- [x] 5.2 Map auth endpoints in `Program.cs` (no authorization required)
- [x] 5.3 Add `.RequireAuthorization()` to `GET /weatherforecast`
- [x] 5.4 Configure OpenAPI bearer security scheme for Development

## 6. Verification

- [x] 6.1 Run `dotnet build` on the solution
- [x] 6.2 Update `BookingSystemAI.http` with register, login, and authorized weather forecast examples
- [x] 6.3 Manually verify: register → login → weather with bearer returns 200; weather without token returns 401
