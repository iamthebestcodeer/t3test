using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Messages;

namespace T3Code.Transport.Client;

public sealed class Subscription<T> : ISubscription<T>, IAsyncDisposable
{
    private readonly Action _unsubscribe;
    private volatile bool _isActive = true;

    public bool IsActive => _isActive;

    public Subscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isActive) return;
        _isActive = false;
        try
        {
            _unsubscribe();
        }
        catch
        {
            // Swallow unsubscribe errors
        }
        await ValueTask.CompletedTask;
    }
}
