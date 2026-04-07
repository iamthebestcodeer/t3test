using T3Code.Core.Abstractions;

namespace T3Code.Platform;

public sealed class DialogService : IDialogService
{
    private readonly Func<Task<string?>> _pickFolderDelegate;
    private readonly Func<string, Task<bool>> _confirmDelegate;

    public DialogService(
        Func<Task<string?>> pickFolderDelegate,
        Func<string, Task<bool>> confirmDelegate)
    {
        _pickFolderDelegate = pickFolderDelegate;
        _confirmDelegate = confirmDelegate;
    }

    public async Task<string?> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        return await _pickFolderDelegate();
    }

    public async Task<bool> ConfirmAsync(string message, CancellationToken cancellationToken = default)
    {
        return await _confirmDelegate(message);
    }
}
