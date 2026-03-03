namespace IntelligentChargeTray.Services;

public interface IAutostartService
{
    bool IsEnabled();
    void Enable();
    void Disable();
    void Toggle();
}
