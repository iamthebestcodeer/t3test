using T3Code.Core.Models;

namespace T3Code.Core.Abstractions;

public interface IConnectionStore
{
    ConnectionStateSnapshot State { get; }
    ConnectionInfo? ConnectionInfo { get; }
    event Action<ConnectionStateSnapshot>? StateChanged;

    void UpdateState(ConnectionStateSnapshot state);
    void SetConnectionInfo(ConnectionInfo info);
}
