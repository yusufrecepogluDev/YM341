# Implementation Plan

## Overview
This plan implements a credential-based authentication system for Student and Club users. The implementation follows the existing ClupApi architecture patterns (BaseController, DTO pattern, Service layer) and provides separate endpoints for each user type.

## Tasks

- [x] 1. Create authentication DTOs
  - Create `ClupApi/DTOs/AuthenticationDtos.cs` file with all request and response DTOs
  - Implement `StudentLoginRequestDto` with validation attributes for StudentNumber and StudentPassword
  - Implement `ClubLoginRequestDto` with validation attributes for ClubNumber and ClubPassword
  - Implement `StudentLoginResponseDto` with StudentID, StudentName, StudentSurname, StudentMail, StudentNumber, and IsActive fields
  - Implement `ClubLoginResponseDto` with ClubID, ClubName, ClubNumber, and IsActive fields
  - Ensure password fields are not included in response DTOs
  - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 2.3, 5.1, 5.2, 5.3, 5.4_

- [x] 2. Create authentication service interface and implementation
  - [x] 2.1 Create service interface
    - Create `ClupApi/Services/IAuthenticationService.cs` interface
    - Define `AuthenticateStudentAsync(StudentLoginRequestDto request)` method returning `Task<StudentLoginResponseDto?>`
    - Define `AuthenticateClubAsync(ClubLoginRequestDto request)` method returning `Task<ClubLoginResponseDto?>`
    - _Requirements: 1.1, 2.1, 3.2_

  - [x] 2.2 Implement authentication service
    - Create `ClupApi/Services/AuthenticationService.cs` implementing `IAuthenticationService`
    - Inject `AppDbContext` via constructor
    - Implement `AuthenticateStudentAsync` method:
      - Query Students DbSet by StudentNumber using async LINQ
      - Validate StudentPassword matches (plain text comparison)
      - Check IsActive is true
      - Return null if authentication fails (any condition)
      - Map Student entity to StudentLoginResponseDto if successful
    - Implement `AuthenticateClubAsync` method:
      - Query Clubs DbSet by ClubNumber using async LINQ
      - Validate ClubPassword matches (plain text comparison)
      - Check IsActive is true
      - Return null if authentication fails (any condition)
      - Map Club entity to ClubLoginResponseDto if successful
    - Ensure no detailed error information is returned (security requirement)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 2.4, 3.2, 4.1, 4.2, 4.3, 4.4, 5.1, 5.2, 5.3_

- [x] 3. Create authentication controller
  - Create `ClupApi/Controllers/AuthenticationController.cs` extending `BaseController`
  - Set route to `[Route("api/auth")]`
  - Inject `IAuthenticationService` via constructor
  - Implement `StudentLogin` endpoint:
    - Route: `[HttpPost("student/login")]`
    - Accept `StudentLoginRequestDto` from body
    - Validate ModelState and return 400 Bad Request with validation errors if invalid
    - Call `AuthenticateStudentAsync` from service
    - Return 200 OK with user data wrapped in ApiResponse if successful
    - Return 401 Unauthorized with generic error message "Geçersiz kimlik bilgileri" if service returns null
    - Use BaseController helper methods for responses
  - Implement `ClubLogin` endpoint:
    - Route: `[HttpPost("club/login")]`
    - Accept `ClubLoginRequestDto` from body
    - Validate ModelState and return 400 Bad Request with validation errors if invalid
    - Call `AuthenticateClubAsync` from service
    - Return 200 OK with user data wrapped in ApiResponse if successful
    - Return 401 Unauthorized with generic error message "Geçersiz kimlik bilgileri" if service returns null
    - Use BaseController helper methods for responses
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 3.3, 4.1, 4.2, 4.3, 5.1, 5.2, 5.3, 5.4_

- [x] 4. Register authentication service in dependency injection
  - Update `ClupApi/Program.cs` to register `IAuthenticationService` and `AuthenticationService`
  - Add `builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();` after existing repository registrations
  - _Requirements: 3.2_

- [ ]* 5. Write unit tests for authentication service
  - Create test file `ClupApi.Tests/AuthenticationServiceTests.cs`
  - Write test for successful student authentication with valid credentials
  - Write test for successful club authentication with valid credentials
  - Write test for failed authentication with invalid student number
  - Write test for failed authentication with invalid club number
  - Write test for failed authentication with incorrect password
  - Write test for failed authentication when IsActive is false
  - Write test to verify correct entity-to-DTO mapping
  - Use in-memory database or mocking for DbContext
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 2.4, 4.4, 5.1, 5.2, 5.3_

- [ ]* 6. Write unit tests for authentication controller
  - Create test file `ClupApi.Tests/AuthenticationControllerTests.cs`
  - Write test for student login returning 200 OK with valid credentials
  - Write test for club login returning 200 OK with valid credentials
  - Write test for student login returning 401 Unauthorized with invalid credentials
  - Write test for club login returning 401 Unauthorized with invalid credentials
  - Write test for 400 Bad Request with invalid ModelState
  - Write test to verify response does not contain password fields
  - Write test to verify generic error message on authentication failure
  - Mock `IAuthenticationService` for controller tests
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 3.3, 4.1, 4.2, 4.3, 5.3, 5.4_

- [ ]* 7. Write integration tests for authentication endpoints
  - Create test file `ClupApi.Tests/Integration/AuthenticationIntegrationTests.cs`
  - Set up test database with sample Student and Club records (active and inactive)
  - Write end-to-end test for student login flow with database
  - Write end-to-end test for club login flow with database
  - Write test for concurrent login requests handling
  - Write test for database connection error handling
  - Clean up test data after tests complete
  - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 2.3, 3.2_
