using ClupApi;
using ClupApi.Middleware;
using ClupApi.Repositories;
using ClupApi.Repositories.Interfaces;
using ClupApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
    
    // Performance optimizations
    options.EnableSensitiveDataLogging(false);
    options.EnableServiceProviderCaching();
    
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy =>
        policy.RequireClaim("userType", "student"));

    options.AddPolicy("ClubOnly", policy =>
        policy.RequireClaim("userType", "club"));

    options.AddPolicy("StudentOrClub", policy =>
        policy.RequireClaim("userType", "student", "club"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };

        // Add detailed logging for JWT authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var authHeader = context.Request.Headers["Authorization"].ToString();
                logger.LogInformation("JWT OnMessageReceived - Authorization header: {Header}", 
                    string.IsNullOrEmpty(authHeader) ? "EMPTY" : authHeader.Substring(0, Math.Min(50, authHeader.Length)));
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Exception}", context.Exception.Message);
                logger.LogError("Token: {Token}", context.Request.Headers["Authorization"].ToString());
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JWT Token validated successfully for user: {User}", 
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Challenge triggered: {Error} - {ErrorDescription}", 
                    context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });



// Repository dependency injection
builder.Services.AddScoped<ClupApi.Repositories.IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ClupApi.Repositories.IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IClubMembershipRepository, ClubMembershipRepository>();
builder.Services.AddScoped<IActivityParticipationRepository, ActivityParticipationRepository>();

// Service dependency injection
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
builder.Services.AddSingleton<ISecurityLogger, SecurityLogger>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Background Service - Süresi dolmuş etkinlik ve duyuruları temizler
builder.Services.AddHostedService<CleanupBackgroundService>();

// ChatService with HttpClient configuration (Requirement 5.1, 5.2)
builder.Services.AddHttpClient<IChatService, ChatService>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var timeoutSeconds = configuration.GetValue<int>("N8nSettings:TimeoutSeconds", 30);
    
    // Configure HttpClient timeout
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    
    // Add default headers if needed
    client.DefaultRequestHeaders.Add("User-Agent", "ClupApi-ChatService/1.0");
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Connection pooling

// Controller�lar� JSON format�nda d�zenli ��kt� ile ekle
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Otomatik model validation yanıtını özelleştir
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToArray();

            var response = ClupApi.Models.ApiResponse.ValidationErrorResponse(errors);
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// CORS (Blazor Server erişimi için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7193",  // Blazor Server HTTPS
            "http://localhost:5278",   // Blazor Server HTTP
            "https://localhost:7065",  // Legacy Blazor Server HTTPS
            "http://localhost:7065")   // Legacy Blazor Server HTTP
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // JWT token için gerekli
    });

    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Swagger dok�mantasyonu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClupApi",
        Version = "v1",
        Description = "Kampus kulüp etkinlik yönetim API'si"
    });
});

var app = builder.Build();

// Validate N8n settings on startup (Requirement 5.1, 5.2)
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var configuration = app.Services.GetRequiredService<IConfiguration>();

var n8nWebhookUrl = configuration["N8nSettings:WebhookUrl"];
if (string.IsNullOrWhiteSpace(n8nWebhookUrl))
{
    logger.LogWarning("N8n webhook URL is not configured. Chat functionality will not work properly.");
    logger.LogWarning("Please configure 'N8nSettings:WebhookUrl' in appsettings.json");
}
else if (!n8nWebhookUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
{
    logger.LogError("N8n webhook URL must use HTTPS protocol. Current URL: {WebhookUrl}", n8nWebhookUrl);
    logger.LogError("Chat service will fail to initialize. Please update the configuration.");
}
else
{
    logger.LogInformation("N8n webhook URL configured: {WebhookUrl}", n8nWebhookUrl);
}

// Middleware pipeline

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClupApi v1");
        c.RoutePrefix = string.Empty; // Swagger ana sayfada a��ls�n
    });
}

app.UseHttpsRedirection();

// CORS must be before Static Files for cross-origin image requests
app.UseCors("AllowClient");

// Static files middleware (for serving uploaded images)
app.UseStaticFiles();

// Security headers middleware
app.UseSecurityHeaders();

// Rate limiting middleware (before authentication)
app.UseRateLimiting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
