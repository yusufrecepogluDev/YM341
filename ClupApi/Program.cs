using ClupApi;
using ClupApi.Models;
using ClupApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository dependency injection
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

// Controller�lar� JSON format�nda d�zenli ��kt� ile ekle
builder.Services.AddControllers()
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

    // Swagger testleri i�in geni� izinli profil
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
        Description = "Kampus kul�p etkinlik y�netim API'si"
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

app.UseHttpsRedirection();

// Swagger testleri i�in geni� eri�im
if (app.Environment.IsDevelopment())
    app.UseCors("AllowSwagger");
else
    app.UseCors("AllowClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
