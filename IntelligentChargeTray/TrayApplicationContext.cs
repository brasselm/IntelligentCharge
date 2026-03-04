using System.Windows.Forms;
using IntelligentChargeTray.Models;
using IntelligentChargeTray.Services;

namespace IntelligentChargeTray;

/// <summary>
/// Haupt-ApplicationContext: verwaltet das System-Tray-Icon und das Kontextmenü.
/// </summary>
public class TrayApplicationContext : ApplicationContext
{
    private readonly IChargeThresholdService _chargeService;
    private readonly IAutostartService _autostartService;
    private readonly IBatteryStateService _batteryStateService;

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;

    // Menü-Einträge, die dynamisch aktualisiert werden
    private readonly ToolStripMenuItem _batteryStatusItem;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _enable80Item;
    private readonly ToolStripMenuItem _enable8060Item;
    private readonly ToolStripMenuItem _disableItem;
    private readonly ToolStripMenuItem _autostartItem;

    // Status-Polling-Timer (optional, alle 30 Sekunden)
    private readonly System.Windows.Forms.Timer _pollingTimer;

    public TrayApplicationContext(IChargeThresholdService chargeService, IAutostartService autostartService, IBatteryStateService batteryStateService)
    {
        _chargeService = chargeService;
        _autostartService = autostartService;
        _batteryStateService = batteryStateService;

        // ── Menü aufbauen ────────────────────────────────────────────────────

        _batteryStatusItem = new ToolStripMenuItem("Batterie: …")
        {
            Enabled = false   // nur als Label, nicht klickbar
        };

        _statusItem = new ToolStripMenuItem("Status: …")
        {
            Enabled = false   // nur als Label, nicht klickbar
        };

        _enable80Item = new ToolStripMenuItem("Aktivieren (75% - 80%)");
        _enable80Item.Click += (_, _) => OnEnableClick(80, null);

        _enable8060Item = new ToolStripMenuItem("Aktivieren (60% - 80%)");
        _enable8060Item.Click += (_, _) => OnEnableClick(80, 60);

        _disableItem = new ToolStripMenuItem("Deaktivieren");
        _disableItem.Click += (_, _) => OnDisableClick();

        _autostartItem = new ToolStripMenuItem("Autostart");
        _autostartItem.Click += (_, _) => OnAutostartClick();

        var quitItem = new ToolStripMenuItem("Beenden");
        quitItem.Click += (_, _) => OnQuitClick();

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.AddRange([
            _batteryStatusItem,
            new ToolStripSeparator(),
            _statusItem,
            new ToolStripSeparator(),
            _enable80Item,
            _enable8060Item,
            _disableItem,
            new ToolStripSeparator(),
            _autostartItem,
            new ToolStripSeparator(),
            quitItem
        ]);

        // ── Tray-Icon ────────────────────────────────────────────────────────

        _notifyIcon = new NotifyIcon
        {
            Text = "IntelligentCharge",
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        // Status beim Start einmalig laden
        RefreshStatus();

        // ── Polling-Timer ────────────────────────────────────────────────────

        _pollingTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _pollingTimer.Tick += (_, _) => RefreshStatus();
        _pollingTimer.Start();
    }

    // ── Event-Handler ─────────────────────────────────────────────────────────

    internal void OnEnableClick(int stop, int? start)
    {
        try
        {
            _chargeService.Enable(stop, start);
        }
        catch (Exception ex)
        {
            ShowError($"Fehler beim Aktivieren: {ex.Message}");
        }
        RefreshStatus();
    }

    internal void OnDisableClick()
    {
        try
        {
            _chargeService.Disable();
        }
        catch (Exception ex)
        {
            ShowError($"Fehler beim Deaktivieren: {ex.Message}");
        }
        RefreshStatus();
    }

    internal void OnAutostartClick()
    {
        try
        {
            _autostartService.Toggle();
        }
        catch (Exception ex)
        {
            ShowError($"Fehler beim Autostart: {ex.Message}");
        }
        RefreshAutostart();
    }

    private void OnQuitClick()
    {
        _pollingTimer.Stop();
        _notifyIcon.Visible = false;
        Application.Exit();
    }

    // ── Status-Aktualisierung ─────────────────────────────────────────────────

    /// <summary>
    /// Liest den aktuellen Status und aktualisiert Icon, Tooltip und Kontextmenü.
    /// </summary>
    internal void RefreshStatus()
    {
        bool active;
        int? threshold;
        int? startThreshold;

        try
        {
            active = _chargeService.IsActive();
            threshold = active ? _chargeService.GetCurrentStopThreshold() : null;
            startThreshold = active ? _chargeService.GetCurrentStartThreshold() : null;
        }
        catch (Exception ex)
        {
            _statusItem.Text = "Status: Fehler";
            _notifyIcon.Text = "IntelligentCharge – Fehler";
            ShowError($"ChargeThreshold.exe konnte nicht ausgeführt werden:\n{ex.Message}");
            return;
        }

        // Menü-Label
        _statusItem.Text = active
            ? $"Status: Aktiv{(threshold.HasValue ? $" ({startThreshold}% - {threshold}%)" : "")}"
            : "Status: Inaktiv";

        // Checkmarks
        _enable80Item.Checked = active && threshold == 80 && startThreshold == 75;
        _enable8060Item.Checked = active && threshold == 80 && startThreshold == 60;
        _disableItem.Checked = !active;

        // Batterie-Status
        RefreshBattery();

        // Tooltip (max. 63 Zeichen)
        string tooltip = active
            ? $"Ladelimit aktiv{(threshold.HasValue ? $": {startThreshold}% - {threshold}%" : "")}"
            : "Kein Ladelimit";
        _notifyIcon.Text = tooltip.Length > 63 ? tooltip[..63] : tooltip;

        // Icon wechseln
        _notifyIcon.Icon = active ? LoadIcon("icon_on") : LoadIcon("icon_off");

        RefreshAutostart();
    }

    internal void RefreshAutostart()
    {
        try
        {
            _autostartItem.Checked = _autostartService.IsEnabled();
        }
        catch
        {
            // Autostart-Status nicht kritisch
        }
    }

    internal void RefreshBattery()
    {
        try
        {
            var info = _batteryStateService.GetBatteryState();
            _batteryStatusItem.Text = FormatBatteryInfo(info);
        }
        catch
        {
            _batteryStatusItem.Text = "Batterie: Fehler";
        }
    }

    /// <summary>
    /// Formatiert einen <see cref="BatteryInfo"/> als lesbaren Menütext.
    /// </summary>
    internal static string FormatBatteryInfo(BatteryInfo info)
    {
        if (info.ChargeStatus == BatteryChargeStatus.NoSystemBattery)
            return "Batterie: Kein Akku";

        string level;
        if (info.ChargeLevel == float.MaxValue)
        {
            // Spezieller Wert: Ladezustand unbekannt
            level = "--";
        }
        else if (info.ChargeLevel is >= 0f and <= 1f)
        {
            level = $"{(int)(info.ChargeLevel * 100)}%";
        }
        else
        {
            // Ungültiger Wert außerhalb des erwarteten Bereichs
            level = "--";
        }

        string chargeState = info.ChargeStatus != BatteryChargeStatus.Unknown
                             && info.ChargeStatus.HasFlag(BatteryChargeStatus.Charging)
            ? "Ladend"
            : info.PowerLineStatus == PowerLineStatus.Online
                ? "Netzbetrieb"
                : "Akkubetrieb";

        string plan = info.ActivePowerPlanName is { Length: > 0 } name ? $" · {name}" : "";

        return $"Batterie: {level} · {chargeState}{plan}";
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────────

    private static Icon LoadIcon(string name)
    {
        string resourceName = $"IntelligentChargeTray.Resources.{name}.ico";
        var stream = typeof(TrayApplicationContext).Assembly.GetManifestResourceStream(resourceName);
        if (stream is not null)
            return new Icon(stream);

        return name == "icon_on" ? SystemIcons.Shield : SystemIcons.Information;
    }

    private static void ShowError(string message) =>
        MessageBox.Show(message, "IntelligentCharge – Fehler",
            MessageBoxButtons.OK, MessageBoxIcon.Error);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pollingTimer.Dispose();
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
        base.Dispose(disposing);
    }
}
