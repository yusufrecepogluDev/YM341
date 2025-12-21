using Microsoft.JSInterop;

namespace KampusEtkinlik.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    bool IsDarkMode { get; }
    Task InitializeAsync();
    Task ToggleThemeAsync();
    Task SetThemeAsync(string theme);
    event Action<string>? OnThemeChanged;
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";
    private const string StorageKey = "theme-preference";

    public event Action<string>? OnThemeChanged;

    public string CurrentTheme => _currentTheme;
    public bool IsDarkMode => _currentTheme == "dark";

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var storedTheme = await _jsRuntime.InvokeAsync<string?>("themeInterop.getTheme");
            if (!string.IsNullOrEmpty(storedTheme) && (storedTheme == "light" || storedTheme == "dark"))
            {
                _currentTheme = storedTheme;
            }
            else
            {
                _currentTheme = "light";
            }
            await ApplyThemeAsync();
        }
        catch
        {
            _currentTheme = "light";
        }
    }

    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme == "light" ? "dark" : "light";
        await SetThemeAsync(newTheme);
    }

    public async Task SetThemeAsync(string theme)
    {
        if (theme != "light" && theme != "dark")
            theme = "light";

        _currentTheme = theme;
        await ApplyThemeAsync();
        await SaveThemeAsync();
        OnThemeChanged?.Invoke(_currentTheme);
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("themeInterop.setTheme", _currentTheme);
        }
        catch { }
    }

    private async Task SaveThemeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("themeInterop.saveTheme", _currentTheme);
        }
        catch { }
    }
}
