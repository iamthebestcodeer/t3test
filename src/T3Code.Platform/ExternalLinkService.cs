using T3Code.Core.Abstractions;
using T3Code.Core.Models;

namespace T3Code.Platform;

public sealed class ExternalLinkService : IExternalLinkService
{
    private static readonly HashSet<string> AllowedSchemes =
        new(StringComparer.OrdinalIgnoreCase) { "http", "https" };

    public bool IsAllowed(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return AllowedSchemes.Contains(uri.Scheme);
        }

        return false;
    }

    public async Task OpenExternalAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!IsAllowed(url))
        {
            throw new InvalidOperationException(
                $"External link denied. Only {string.Join(", ", AllowedSchemes)} schemes are allowed. Got: {url}");
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
        };
        System.Diagnostics.Process.Start(psi);

        await Task.CompletedTask;
    }
}
