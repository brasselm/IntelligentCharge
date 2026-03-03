using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace IntelligentChargeTray.Services;

/// <summary>
/// Wrapper für ChargeThreshold.exe.
/// Erwartet die EXE im gleichen Verzeichnis wie diese Anwendung.
/// </summary>
public class ChargeThresholdService : IChargeThresholdService
{
    private readonly string _exePath;

    public ChargeThresholdService()
        : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChargeThreshold.exe")) { }

    /// <param name="exePath">Pfad zur ChargeThreshold.exe (für Tests überschreibbar).</param>
    internal ChargeThresholdService(string exePath)
    {
        _exePath = exePath;
        if (!File.Exists(_exePath))
        {
            TryExtractEmbeddedExe(_exePath);
        }
    }

    private void TryExtractEmbeddedExe(string targetPath)
    {
        var asm = Assembly.GetExecutingAssembly();
        var resource = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("ChargeThreshold.exe", StringComparison.OrdinalIgnoreCase));
        if (resource == null)
            throw new FileNotFoundException("ChargeThreshold.exe nicht gefunden und auch nicht als eingebettete Ressource vorhanden.");

        using var rs = asm.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException("Eingebettete Ressource konnte nicht geöffnet werden: " + resource);

        var dir = Path.GetDirectoryName(targetPath) ?? AppDomain.CurrentDomain.BaseDirectory;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using var fs = File.Create(targetPath);
        rs.CopyTo(fs);
    }

    /// <summary>
    /// Gibt die Rohausgabe von "ChargeThreshold.exe status" zurück.
    /// </summary>
    public string GetStatusRaw()
    {
        return RunAndCapture("status");
    }

    /// <summary>
    /// Aktiviert das Ladelimit.
    /// </summary>
    public void Enable(int stopThreshold, int? startThreshold = null)
    {
        string args = startThreshold.HasValue
            ? $"on {stopThreshold} {startThreshold.Value}"
            : $"on {stopThreshold}";
        Run(args);
    }

    /// <summary>
    /// Deaktiviert das Ladelimit.
    /// </summary>
    public void Disable()
    {
        Run("off");
    }

    /// <summary>
    /// Ermittelt, ob aktuell ein Ladelimit aktiv ist.
    /// </summary>
    public bool IsActive() => ParseIsActive(GetStatusRaw());

    /// <summary>
    /// Liefert den aktuellen Stop-Schwellenwert oder null, wenn nicht aktiv bzw. nicht parsbar.
    /// </summary>

    public int? GetCurrentStopThreshold() => ParseStopThreshold(GetStatusRaw());
    
    /// <summary>
    /// Liefert den aktuellen Start-Schwellenwert oder null, wenn nicht aktiv bzw. nicht parsbar.
    /// </summary>
    public int? GetCurrentStartThreshold() => ParseStartThreshold(GetStatusRaw());

    // ── Parsing (internal static → direkt in Unit-Tests testbar) ─────────────

    /// <summary>
    /// Parsed die Rohausgabe von "status" auf aktiven Zustand.
    /// </summary>
    internal static bool ParseIsActive(string rawOutput)
    {
        // Aktiv:   "Charge threshold for Battery #1: Start at 60%, Stop at 80%."
        // Inaktiv: "Charge threshold for Battery #1: OFF."
        return Regex.IsMatch(rawOutput, @"Stop at \d+%", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Extrahiert den Stop-Schwellenwert aus der Statusausgabe.
    /// </summary>
    internal static int? ParseStopThreshold(string rawOutput)
    {
        // Beispiel: "Stop at 80%"
        var match = Regex.Match(rawOutput, @"Stop at (\d+)%", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
            return value;
        return null;
    }

    /// <summary>
    /// Extrahiert den Start-Schwellenwert aus der Statusausgabe.
    /// </summary>
    internal static int? ParseStartThreshold(string rawOutput)
    {
        // Beispiel: "Start at 60%"
        var match = Regex.Match(rawOutput, @"Start at (\d+)%", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
            return value;
        return null;
    }

    // ── Hilfsmethoden ────────────────────────────────────────────────────────

    private void Run(string arguments)
    {
        var psi = BuildProcessStartInfo(arguments);
        psi.RedirectStandardOutput = false;
        using var process = Process.Start(psi);
        process?.WaitForExit();
    }

    private string RunAndCapture(string arguments)
    {
        var psi = BuildProcessStartInfo(arguments);
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Prozess konnte nicht gestartet werden.");
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    private ProcessStartInfo BuildProcessStartInfo(string arguments) =>
        new(_exePath, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };

}
