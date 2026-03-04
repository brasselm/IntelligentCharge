using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelligentChargeTray.Models;

namespace IntelligentChargeTray.Services;

/// <summary>
/// Liest den Batterie- und Energieplanzustand über die Windows-API.
/// Batterieinformationen kommen von <see cref="SystemInformation.PowerStatus"/>,
/// der aktive Energieplan wird per P/Invoke aus powrprof.dll ermittelt.
/// </summary>
public class BatteryStateService : IBatteryStateService
{
    // GUID des Windows-Energiesparplans
    private static readonly Guid PowerSaverGuid = new("a1841308-3541-4fab-bc81-f71556f20b4a");

    [DllImport("powrprof.dll", SetLastError = false)]
    private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr ActivePolicyGuid);

    [DllImport("powrprof.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    private static extern uint PowerReadFriendlyName(
        IntPtr RootPowerKey,
        ref Guid SchemeGuid,
        IntPtr SubGroupOfPowerSettingsGuid,
        IntPtr PowerSettingGuid,
        System.Text.StringBuilder Buffer,
        ref uint BufferSize);

    [DllImport("kernel32.dll", SetLastError = false)]
    private static extern IntPtr LocalFree(IntPtr hMem);

    public BatteryInfo GetBatteryState()
    {
        var ps = SystemInformation.PowerStatus;
        var (isPowerSave, planName) = GetActivePowerPlanInfo();

        return new BatteryInfo
        {
            ChargeLevel = ps.BatteryLifePercent,
            ChargeStatus = ps.BatteryChargeStatus,
            PowerLineStatus = ps.PowerLineStatus,
            LifeRemainingSeconds = ps.BatteryLifeRemaining,
            IsPowerSavePlan = isPowerSave,
            ActivePowerPlanName = planName
        };
    }

    private static (bool IsPowerSave, string? PlanName) GetActivePowerPlanInfo()
    {
        try
        {
            uint result = PowerGetActiveScheme(IntPtr.Zero, out IntPtr guidPtr);
            if (result != 0 || guidPtr == IntPtr.Zero)
                return (false, null);

            try
            {
                var guid = Marshal.PtrToStructure<Guid>(guidPtr);
                bool isPowerSave = guid == PowerSaverGuid;

                var buffer = new System.Text.StringBuilder(512);
                uint bufferSize = (uint)buffer.Capacity; // Anzahl Zeichen im Puffer (Unicode wird vom Marshaller gehandhabt)
                uint readResult = PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize);
                if (readResult != 0)
                    return (isPowerSave, null);

                string? name = buffer.Length > 0 ? buffer.ToString() : null;
                return (isPowerSave, name);
            }
            finally
            {
                LocalFree(guidPtr);
            }
        }
        catch
        {
            return (false, null);
        }
    }
}
