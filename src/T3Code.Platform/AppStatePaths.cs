namespace T3Code.Platform;

public sealed class AppStatePaths
{
    public string DataRoot { get; }
    public string LogsDir { get; }
    public string SettingsPath { get; }
    public string KeybindingsPath { get; }
    public string BackupDir { get; }

    public AppStatePaths(string? customRoot = null)
    {
        DataRoot = customRoot
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "T3Code");
        LogsDir = Path.Combine(DataRoot, "logs");
        SettingsPath = Path.Combine(DataRoot, "settings.json");
        KeybindingsPath = Path.Combine(DataRoot, "keybindings.json");
        BackupDir = Path.Combine(DataRoot, "backups");
    }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(DataRoot);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(BackupDir);
    }
}
