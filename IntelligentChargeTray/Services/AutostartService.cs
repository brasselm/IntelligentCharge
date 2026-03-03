using Microsoft.Win32;

namespace IntelligentChargeTray.Services;

/// <summary>
/// Verwaltet den Windows-Autostart-Eintrag für diese Anwendung
/// über HKCU (kein Adminrecht erforderlich).
/// </summary>
public class AutostartService : IAutostartService
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string DefaultAppName = "IntelligentChargeTray";

    private readonly string _appName;

    public AutostartService() : this(DefaultAppName) { }

    internal AutostartService(string appName)
    {
        _appName = appName;
    }

    /// <summary>
    /// Gibt zurück, ob der Autostart-Eintrag vorhanden ist.
    /// </summary>
    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
        return key?.GetValue(_appName) is not null;
    }

    /// <summary>
    /// Trägt die aktuelle EXE in den Windows-Autostart ein.
    /// </summary>
    public void Enable()
    {
        string exePath = Application.ExecutablePath;
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true)
            ?? throw new InvalidOperationException("Registry-Schlüssel konnte nicht geöffnet werden.");
        key.SetValue(_appName, $"\"{exePath}\"");
    }

    /// <summary>
    /// Entfernt den Autostart-Eintrag, falls vorhanden.
    /// </summary>
    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
        key?.DeleteValue(_appName, throwOnMissingValue: false);
    }

    /// <summary>
    /// Wechselt den Autostart-Zustand.
    /// </summary>
    public void Toggle()
    {
        if (IsEnabled()) Disable();
        else Enable();
    }
}
