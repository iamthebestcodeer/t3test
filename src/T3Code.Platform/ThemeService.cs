using T3Code.Core.Abstractions;
using T3Code.Core.Models;

namespace T3Code.Platform;

public sealed class ThemeService : IThemeService
{
    private Theme _currentTheme = Theme.System;

    public Theme CurrentTheme => _currentTheme;

    public event Action<Theme>? ThemeChanged;

    public Task SetThemeAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        if (_currentTheme == theme) return Task.CompletedTask;
        _currentTheme = theme;
        ThemeChanged?.Invoke(theme);
        return Task.CompletedTask;
    }

    public void RaiseThemeChanged(Theme theme)
    {
        if (_currentTheme == theme) return;
        _currentTheme = theme;
        ThemeChanged?.Invoke(theme);
    }
}
