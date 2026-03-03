using Microsoft.Win32;
using IntelligentChargeTray.Services;

namespace IntelligentChargeTray.Tests;

/// <summary>
/// Integrationstests für AutostartService gegen HKCU.
/// Verwendet einen eigenen Test-Registry-Wert, der nach jedem Test bereinigt wird.
/// </summary>
public class AutostartServiceTests : IDisposable
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string TestAppName = "IntelligentChargeTray_Test";

    // Service mit isoliertem Test-AppName
    private readonly AutostartService _sut = new(TestAppName);

    public void Dispose()
    {
        // Test-Eintrag immer bereinigen, unabhängig vom Testergebnis
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
        key?.DeleteValue(TestAppName, throwOnMissingValue: false);
    }

    [Fact]
    public void IsEnabled_WhenNoEntry_ReturnsFalse()
    {
        Assert.False(_sut.IsEnabled());
    }

    [Fact]
    public void Enable_WritesRegistryValue()
    {
        _sut.Enable();

        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
        Assert.NotNull(key?.GetValue(TestAppName));
    }

    [Fact]
    public void IsEnabled_AfterEnable_ReturnsTrue()
    {
        _sut.Enable();

        Assert.True(_sut.IsEnabled());
    }

    [Fact]
    public void Disable_RemovesRegistryValue()
    {
        _sut.Enable();
        _sut.Disable();

        Assert.False(_sut.IsEnabled());
    }

    [Fact]
    public void Disable_WhenNotEnabled_DoesNotThrow()
    {
        // Sollte keine Exception werfen, auch wenn der Wert nicht existiert
        var ex = Record.Exception(() => _sut.Disable());
        Assert.Null(ex);
    }

    [Fact]
    public void Toggle_WhenDisabled_Enables()
    {
        _sut.Toggle();

        Assert.True(_sut.IsEnabled());
    }

    [Fact]
    public void Toggle_WhenEnabled_Disables()
    {
        _sut.Enable();
        _sut.Toggle();

        Assert.False(_sut.IsEnabled());
    }
}
