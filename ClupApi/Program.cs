using ClupApi;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    });



// Repository dependency injection
builder.Services.AddScoped<ClupApi.Repositories.IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ClupApi.Repositories.IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();

// Service dependency injection
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

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
    });

// CORS (Blazor Server eri�imi i�in)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("https://localhost:7065") // Blazor Server URL�si
              .AllowAnyHeader()
              .AllowAnyMethod();
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


app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

// Swagger testleri i�in geni� eri�im
if (app.Environment.IsDevelopment())
    app.UseCors("AllowSwagger");
else
    app.UseCors("AllowClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
