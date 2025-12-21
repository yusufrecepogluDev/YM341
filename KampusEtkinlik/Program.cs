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

// Membership Cache servisi (SessionStorage için JavaScript interop kullanır)
builder.Services.AddScoped<MembershipCacheService>();

// Theme servisi (Dark/Light mode için)
builder.Services.AddScoped<IThemeService, ThemeService>();

// API servislerini DI konteynerine kaydet
builder.Services.AddHttpClient<AnnouncementService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
});
builder.Services.AddHttpClient<ActivityService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
});
builder.Services.AddHttpClient<AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
});
builder.Services.AddHttpClient<CalendarApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
});
builder.Services.AddHttpClient<ClubService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
});
builder.Services.AddHttpClient<ClubMembershipService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
});

// Chat servisi - N8n chatbot entegrasyonu için (uses IHttpClientFactory)
builder.Services.AddHttpClient("ChatClient", client =>
{
    // Base configuration for chat client
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IChatClientService, ChatClientService>();

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

app.MapRazorComponents<KampusEtkinlik.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
