using IntelligentChargeTray.Models;

namespace IntelligentChargeTray.Services;

public interface IBatteryStateService
{
    /// <summary>
    /// Gibt den aktuellen Batterie- und Energieplanzustand zurück.
    /// </summary>
    BatteryInfo GetBatteryState();
}
