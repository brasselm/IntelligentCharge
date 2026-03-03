namespace IntelligentChargeTray.Services;

public interface IChargeThresholdService
{
    string GetStatusRaw();
    void Enable(int stopThreshold, int? startThreshold = null);
    void Disable();
    bool IsActive();
    int? GetCurrentStopThreshold();
    int? GetCurrentStartThreshold();
}
