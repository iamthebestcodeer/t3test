using T3Code.Core.Abstractions;
using T3Code.Core.Models;
using T3Code.Transport.Client;

namespace T3Code.Features;

public sealed class GitService : IAsyncDisposable
{
    private readonly ITransportClient _transport;
    private readonly IGitStore _store;
    private ISubscription<GitStatus>? _statusSubscription;
    private volatile bool _disposed;

    public GitService(ITransportClient transport, IGitStore store)
    {
        _transport = transport;
        _store = store;
    }

    public async Task<IReadOnlyList<GitBranch>> ListBranchesAsync(
        string cwd,
        CancellationToken cancellationToken = default)
    {
        return await _transport.RequestListAsync<GitBranch>(
            "git.listBranches",
            new { cwd },
            cancellationToken);
    }

    public async Task<GitWorktree> CreateWorktreeAsync(
        string cwd,
        string branch,
        string? newBranch = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            ["cwd"] = cwd,
            ["branch"] = branch,
        };
        if (newBranch != null) parameters["newBranch"] = newBranch;

        return await _transport.RequestAsync<GitWorktree>(
            "git.createWorktree",
            parameters,
            cancellationToken);
    }

    public async Task RemoveWorktreeAsync(
        string cwd,
        string path,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "git.removeWorktree",
            new { cwd, path },
            cancellationToken);
    }

    public async Task CreateBranchAsync(
        string cwd,
        string name,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "git.createBranch",
            new { cwd, name },
            cancellationToken);
    }

    public async Task CheckoutAsync(
        string cwd,
        string branch,
        CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "git.checkout",
            new { cwd, branch },
            cancellationToken);
    }

    public async Task InitAsync(string cwd, CancellationToken cancellationToken = default)
    {
        await _transport.RequestAsync<object>(
            "git.init",
            new { cwd },
            cancellationToken);
    }

    public async Task<PullRequestRef> ResolvePullRequestAsync(
        string cwd,
        string @ref,
        CancellationToken cancellationToken = default)
    {
        return await _transport.RequestAsync<PullRequestRef>(
            "git.resolvePullRequest",
            new { cwd, @ref },
            cancellationToken);
    }

    public async Task SubscribeToStatusAsync(
        string cwd,
        Action<GitStatus> handler,
        CancellationToken cancellationToken = default)
    {
        if (_statusSubscription != null)
        {
            await _statusSubscription.DisposeAsync();
        }

        _statusSubscription = await _transport.SubscribeAsync<GitStatus>(
            "git.onStatus",
            status =>
            {
                _store.UpdateStatus(status);
                handler(status);
            },
            new { cwd },
            cancellationToken);
    }

    public async Task<GitStatus> RefreshStatusAsync(
        string cwd,
        CancellationToken cancellationToken = default)
    {
        var status = await _transport.RequestAsync<GitStatus>(
            "git.refreshStatus",
            new { cwd },
            cancellationToken);
        _store.UpdateStatus(status);
        return status;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_statusSubscription != null)
        {
            await _statusSubscription.DisposeAsync();
        }
    }
}
