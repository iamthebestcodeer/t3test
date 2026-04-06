using T3Code.Core.Models;

namespace T3Code.Core.Abstractions;

public interface IDialogService
{
    Task<string?> PickFolderAsync(CancellationToken cancellationToken = default);
    Task<bool> ConfirmAsync(string message, CancellationToken cancellationToken = default);
}

public interface IExternalLinkService
{
    bool IsAllowed(string url);
    Task OpenExternalAsync(string url, CancellationToken cancellationToken = default);
}

public interface IThemeService
{
    Theme CurrentTheme { get; }
    event Action<Theme>? ThemeChanged;
    Task SetThemeAsync(Theme theme, CancellationToken cancellationToken = default);
}
