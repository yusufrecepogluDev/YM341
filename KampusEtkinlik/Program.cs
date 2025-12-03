using KampusEtkinlik.Components;
using KampusEtkinlik.Services;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// LocalStorage servisi (sadece "Beni Hatırla" özelliği için)
builder.Services.AddBlazoredLocalStorage();

// Token servisi (ProtectedSessionStorage built-in olarak gelir)
builder.Services.AddScoped<TokenService>();

// API servislerini DI konteynerine kaydet
builder.Services.AddHttpClient<AnnouncementService>();
builder.Services.AddHttpClient<ActivityService>();
builder.Services.AddHttpClient<AuthenticationService>();
builder.Services.AddHttpClient<CalendarApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
