using System.Windows.Forms;

namespace IntelligentChargeTray.Models;

/// <summary>
/// Aktueller Batterie- und Energieplanzustand des Systems.
/// </summary>
public record BatteryInfo
{
    /// <summary>Ladestand 0.0–1.0. <c>float.MaxValue</c> bedeutet unbekannt.</summary>
    public float ChargeLevel { get; init; }

    /// <summary>Ladestatus (Charging, High, Low, Critical, NoSystemBattery, Unknown).</summary>
    public BatteryChargeStatus ChargeStatus { get; init; }

    /// <summary>Stromquelle (Online = Netzbetrieb, OnBattery, Unknown).</summary>
    public PowerLineStatus PowerLineStatus { get; init; }

    /// <summary>Verbleibende Betriebszeit in Sekunden. -1 = unbekannt.</summary>
    public int LifeRemainingSeconds { get; init; }

    /// <summary>Ob der aktive Energieplan der Windows-Energiesparmodus ist.</summary>
    public bool IsPowerSavePlan { get; init; }

    /// <summary>Anzeigename des aktiven Energieplans (z.B. "Balanced", "Energiesparmodus").</summary>
    public string? ActivePowerPlanName { get; init; }
}
