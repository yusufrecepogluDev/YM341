# Authentication System Design Document

## Overview

Bu tasarım, Student ve Club kullanıcılarının sistemde kimlik doğrulaması yaparak giriş yapmalarını sağlayan bir authentication sistemi tanımlar. Sistem, mevcut ClupApi mimarisine uygun olarak, basit credential-based authentication yaklaşımı kullanır ve her kullanıcı tipi için ayrı endpoint'ler sağlar.

**Temel Özellikler:**
- Student ve Club için ayrı authentication endpoint'leri
- Credential-based authentication (number + password)
- IsActive durumu kontrolü
- Güvenli hata mesajları (bilgi sızdırma önleme)
- Mevcut API mimarisi ile tutarlı yapı (BaseController, DTO pattern, Service layer)

**Kapsam Dışı:**
- JWT token tabanlı authentication (gelecek iterasyonda eklenebilir)
- Session yönetimi
- Password hashing (mevcut sistemde plain text kullanılıyor)
- Rate limiting veya brute force koruması
- Multi-factor authentication

## Architecture

### Mimari Kararlar

**1. Separate Endpoints Yaklaşımı**
- **Karar:** Student ve Club için ayrı authentication endpoint'leri (/api/auth/student/login ve /api/auth/club/login)
- **Gerekçe:** 
  - Her kullanıcı tipinin farklı veri modelleri ve alanları var
  - Tip güvenliği sağlar
  - Client tarafında kullanıcı tipi seçimi kolaylaşır
  - Gelecekte farklı authentication stratejileri uygulanabilir

**2. Service Layer Pattern**
- **Karar:** Authentication logic'i AuthenticationService içinde implement edilecek
- **Gerekçe:**
  - Mevcut mimari ile tutarlılık (ActivityService, AnnouncementService pattern'i)
  - Business logic'in controller'dan ayrılması
  - Test edilebilirlik
  - Kod tekrarının önlenmesi

**3. Repository Pattern Kullanılmayacak**
- **Karar:** Authentication için ayrı repository oluşturulmayacak, doğrudan DbContext kullanılacak
- **Gerekçe:**
  - Authentication basit CRUD operasyonları gerektirmiyor
  - Student ve Club zaten mevcut DbContext'te tanımlı
  - Gereksiz abstraction katmanı eklenmemiş olur
  - Mevcut kodda da tüm entity'ler için repository yok (sadece Activity ve Announcement için var)

**4. Plain Text Password Comparison**
- **Karar:** Şifreler plain text olarak karşılaştırılacak (hashing yok)
- **Gerekçe:**
  - Mevcut sistemde Student ve Club modelleri zaten plain text password kullanıyor
  - Password hashing eklemek tüm mevcut verilerin migration'ını gerektirir
  - Bu özellik kapsamı dışında (güvenlik iyileştirmesi ayrı bir spec olabilir)

### Sistem Bileşenleri

```
┌─────────────────────────────────────────────────────────────┐
│                         Client Layer                         │
│                  (Blazor/External Clients)                   │
└────────────────────────┬────────────────────────────────────┘
                         │
                         │ HTTP POST
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    API Controller Layer                      │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         AuthenticationController                      │  │
│  │  - POST /api/auth/student/login                      │  │
│  │  - POST /api/auth/club/login                         │  │
│  │  (extends BaseController)                            │  │
│  └──────────────────────┬───────────────────────────────┘  │
└─────────────────────────┼───────────────────────────────────┘
                          │
                          │ Calls
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                     Service Layer                            │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         AuthenticationService                         │  │
│  │  - AuthenticateStudentAsync()                        │  │
│  │  - AuthenticateClubAsync()                           │  │
│  └──────────────────────┬───────────────────────────────┘  │
└─────────────────────────┼───────────────────────────────────┘
                          │
                          │ Queries
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                     Data Access Layer                        │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              AppDbContext                             │  │
│  │  - Students DbSet                                    │  │
│  │  - Clubs DbSet                                       │  │
│  └──────────────────────┬───────────────────────────────┘  │
└─────────────────────────┼───────────────────────────────────┘
                          │
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                      Database Layer                          │
│                    (SQL Server)                              │
│  - Student Table                                             │
│  - Club Table                                                │
└──────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### 1. DTOs (Data Transfer Objects)

#### StudentLoginRequestDto
```csharp
public class StudentLoginRequestDto
{
    [Required(ErrorMessage = "Öğrenci numarası zorunludur")]
    public long StudentNumber { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur")]
    [MaxLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir")]
    public string StudentPassword { get; set; }
}
```

#### ClubLoginRequestDto
```csharp
public class ClubLoginRequestDto
{
    [Required(ErrorMessage = "Kulüp numarası zorunludur")]
    public long ClubNumber { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur")]
    [MaxLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir")]
    public string ClubPassword { get; set; }
}
```

#### StudentLoginResponseDto
```csharp
public class StudentLoginResponseDto
{
    public int StudentID { get; set; }
    public string StudentName { get; set; }
    public string StudentSurname { get; set; }
    public string StudentMail { get; set; }
    public long StudentNumber { get; set; }
    public bool IsActive { get; set; }
}
```

#### ClubLoginResponseDto
```csharp
public class ClubLoginResponseDto
{
    public int ClubID { get; set; }
    public string ClubName { get; set; }
    public long ClubNumber { get; set; }
    public bool IsActive { get; set; }
}
```

### 2. Service Interface and Implementation

#### IAuthenticationService
```csharp
public interface IAuthenticationService
{
    Task<StudentLoginResponseDto?> AuthenticateStudentAsync(StudentLoginRequestDto request);
    Task<ClubLoginResponseDto?> AuthenticateClubAsync(ClubLoginRequestDto request);
}
```

#### AuthenticationService
Service katmanı aşağıdaki sorumlulukları üstlenir:
- Kullanıcı numarasına göre veritabanında arama
- Şifre doğrulama
- IsActive durumu kontrolü
- Entity'den DTO'ya dönüşüm
- Null döndürme (authentication başarısız olduğunda)

**Önemli:** Service katmanı güvenlik nedeniyle başarısız authentication durumunda detaylı hata bilgisi döndürmez. Sadece null döner ve controller katmanı genel bir hata mesajı üretir.

### 3. Controller

#### AuthenticationController
```csharp
[ApiController]
[Route("api/auth")]
public class AuthenticationController : BaseController
{
    private readonly IAuthenticationService _authService;

    // POST /api/auth/student/login
    [HttpPost("student/login")]
    public async Task<IActionResult> StudentLogin([FromBody] StudentLoginRequestDto request)
    
    // POST /api/auth/club/login
    [HttpPost("club/login")]
    public async Task<IActionResult> ClubLogin([FromBody] ClubLoginRequestDto request)
}
```

Controller sorumlulukları:
- Request validation (ModelState kontrolü)
- Service çağrısı
- Başarılı durumda 200 OK + user data
- Başarısız durumda 401 Unauthorized + genel hata mesajı
- BaseController helper metodlarını kullanma

## Data Models

Mevcut modeller kullanılacak, değişiklik gerekmez:

### Student Model (Mevcut)
```csharp
public class Student
{
    public int StudentID { get; set; }
    public string StudentName { get; set; }
    public string StudentSurname { get; set; }
    public long StudentNumber { get; set; }  // Unique index mevcut
    public string StudentMail { get; set; }
    public string StudentPassword { get; set; }  // Plain text
    public string? StudentStatus { get; set; }
    public bool IsActive { get; set; }
    // Navigation properties...
}
```

### Club Model (Mevcut)
```csharp
public class Club
{
    public int ClubID { get; set; }
    public string ClubName { get; set; }
    public long ClubNumber { get; set; }  // Unique index mevcut
    public string ClubPassword { get; set; }  // Plain text
    public bool IsActive { get; set; }
    // Navigation properties...
}
```

**Not:** StudentNumber ve ClubNumber için unique index'ler zaten AppDbContext'te tanımlı.

## Error Handling

### Güvenlik Odaklı Hata Yönetimi

**Prensip:** Başarısız authentication durumlarında, saldırganlara bilgi sızdırmamak için genel hata mesajları kullanılır.

### Hata Senaryoları

| Senaryo | HTTP Status | Response Message |
|---------|-------------|------------------|
| Geçersiz model (validation error) | 400 Bad Request | Validation hata detayları |
| Kullanıcı bulunamadı | 401 Unauthorized | "Geçersiz kimlik bilgileri" |
| Şifre yanlış | 401 Unauthorized | "Geçersiz kimlik bilgileri" |
| IsActive = false | 401 Unauthorized | "Geçersiz kimlik bilgileri" |
| Başarılı login | 200 OK | User data |

**Önemli:** Kullanıcı bulunamadı, şifre yanlış ve inactive user durumları aynı hata mesajını döner. Bu, saldırganların sistemde hangi kullanıcıların var olduğunu öğrenmesini engeller.

### Response Format

Mevcut API response pattern'i kullanılacak (ApiResponse wrapper):

**Başarılı Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "studentID": 1,
    "studentName": "Ahmet",
    "studentSurname": "Yılmaz",
    "studentMail": "ahmet@example.com",
    "studentNumber": 20210001,
    "isActive": true
  },
  "errors": null
}
```

**Başarısız Response (401):**
```json
{
  "success": false,
  "message": "Geçersiz kimlik bilgileri",
  "data": null,
  "errors": ["Lütfen bilgilerinizi kontrol edip tekrar deneyin"]
}
```

**Validation Error (400):**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": ["Öğrenci numarası zorunludur", "Şifre zorunludur"]
}
```

## Testing Strategy

### Unit Tests

**AuthenticationService Tests:**
- ✓ Valid credentials ile başarılı student authentication
- ✓ Valid credentials ile başarılı club authentication
- ✓ Geçersiz student number ile null dönmesi
- ✓ Geçersiz club number ile null dönmesi
- ✓ Yanlış password ile null dönmesi
- ✓ IsActive=false olan user için null dönmesi
- ✓ Entity'den DTO'ya doğru mapping

**AuthenticationController Tests:**
- ✓ Valid request ile 200 OK response
- ✓ Invalid credentials ile 401 Unauthorized response
- ✓ Invalid model state ile 400 Bad Request response
- ✓ Service null döndüğünde genel hata mesajı
- ✓ Response'da password bilgisinin olmaması

### Integration Tests

- ✓ End-to-end student login flow (database ile)
- ✓ End-to-end club login flow (database ile)
- ✓ Concurrent login requests handling
- ✓ Database connection error handling

### Test Data Requirements

Test veritabanında aşağıdaki test kullanıcıları olmalı:
- Active student (IsActive=true)
- Inactive student (IsActive=false)
- Active club (IsActive=true)
- Inactive club (IsActive=false)

## Security Considerations

### Mevcut Güvenlik Durumu

**Zayıf Noktalar (Bu Spec Kapsamı Dışı):**
- Şifreler plain text olarak saklanıyor (hashing yok)
- HTTPS kullanımı zorunlu olmalı (production'da)
- Rate limiting yok (brute force saldırılarına açık)
- Account lockout mekanizması yok

**Bu Spec'te Uygulanan Güvenlik Önlemleri:**
- ✓ Genel hata mesajları (information disclosure önleme)
- ✓ Response'larda password bilgisi döndürülmüyor
- ✓ Input validation (SQL injection önleme)
- ✓ IsActive kontrolü

### Gelecek İyileştirmeler (Ayrı Spec'ler)

1. **Password Hashing:** BCrypt veya PBKDF2 ile password hashing
2. **JWT Authentication:** Token-based authentication sistemi
3. **Rate Limiting:** Brute force koruması
4. **Audit Logging:** Login attempt'lerin loglanması
5. **Password Policy:** Minimum şifre gereksinimleri

## API Endpoints

### Student Login
```
POST /api/auth/student/login
Content-Type: application/json

Request Body:
{
  "studentNumber": 20210001,
  "studentPassword": "password123"
}

Success Response (200 OK):
{
  "success": true,
  "message": "Login successful",
  "data": {
    "studentID": 1,
    "studentName": "Ahmet",
    "studentSurname": "Yılmaz",
    "studentMail": "ahmet@example.com",
    "studentNumber": 20210001,
    "isActive": true
  }
}

Error Response (401 Unauthorized):
{
  "success": false,
  "message": "Geçersiz kimlik bilgileri",
  "errors": ["Lütfen bilgilerinizi kontrol edip tekrar deneyin"]
}
```

### Club Login
```
POST /api/auth/club/login
Content-Type: application/json

Request Body:
{
  "clubNumber": 1001,
  "clubPassword": "clubpass123"
}

Success Response (200 OK):
{
  "success": true,
  "message": "Login successful",
  "data": {
    "clubID": 1,
    "clubName": "Bilgisayar Kulübü",
    "clubNumber": 1001,
    "isActive": true
  }
}

Error Response (401 Unauthorized):
{
  "success": false,
  "message": "Geçersiz kimlik bilgileri",
  "errors": ["Lütfen bilgilerinizi kontrol edip tekrar deneyin"]
}
```

## Dependencies and Configuration

### NuGet Packages
Yeni paket gerekmez. Mevcut paketler yeterli:
- Microsoft.EntityFrameworkCore
- Microsoft.AspNetCore.Mvc

### Dependency Injection
Program.cs'e eklenecek:
```csharp
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
```

### Configuration
appsettings.json'da değişiklik gerekmez. Mevcut connection string kullanılacak.

## Implementation Notes

### Kod Organizasyonu
```
ClupApi/
├── Controllers/
│   └── AuthenticationController.cs (YENİ)
├── DTOs/
│   └── AuthenticationDtos.cs (YENİ)
├── Services/
│   ├── IAuthenticationService.cs (YENİ)
│   └── AuthenticationService.cs (YENİ)
├── Models/
│   ├── Student.cs (MEVCUT - değişiklik yok)
│   └── Club.cs (MEVCUT - değişiklik yok)
└── Program.cs (GÜNCELLEME - DI registration)
```

### Mevcut Kod ile Entegrasyon
- BaseController'dan inherit edilecek
- Mevcut ApiResponse pattern'i kullanılacak
- Mevcut validation attribute'ları kullanılacak
- Mevcut CORS policy'leri geçerli olacak

### Performance Considerations
- StudentNumber ve ClubNumber için unique index'ler mevcut (hızlı lookup)
- Async/await pattern kullanılacak
- Single database query per authentication attempt
- No caching (stateless authentication)
