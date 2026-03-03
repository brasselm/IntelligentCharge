using NSubstitute;
using NSubstitute.ExceptionExtensions;
using IntelligentChargeTray.Services;

namespace IntelligentChargeTray.Tests;

/// <summary>
/// Unit-Tests für TrayApplicationContext.
/// Nutzt gemockte Services – kein echter Prozess oder Registry-Zugriff.
/// WinForms-Komponenten werden im STA-Thread erstellt (xunit default auf Windows).
/// </summary>
[Collection("STA")]
public class TrayApplicationContextTests : IDisposable
{
    private readonly IChargeThresholdService _chargeService = Substitute.For<IChargeThresholdService>();
    private readonly IAutostartService _autostartService = Substitute.For<IAutostartService>();
    private readonly TrayApplicationContext _sut;

    public TrayApplicationContextTests()
    {
        // Standardverhalten: Ladelimit inaktiv, Autostart aus
        _chargeService.IsActive().Returns(false);
        _chargeService.GetCurrentStopThreshold().Returns((int?)null);
        _autostartService.IsEnabled().Returns(false);

        _sut = new TrayApplicationContext(_chargeService, _autostartService);
    }

    public void Dispose() => _sut.Dispose();

    // ── RefreshStatus ─────────────────────────────────────────────────────────

    [Fact]
    public void RefreshStatus_WhenInactive_QueriesService()
    {
        _chargeService.IsActive().Returns(false);

        _sut.RefreshStatus();

        _chargeService.Received().IsActive();
    }

    [Fact]
    public void RefreshStatus_WhenActive_QueriesThreshold()
    {
        _chargeService.IsActive().Returns(true);
        _chargeService.GetCurrentStopThreshold().Returns(80);

        _sut.RefreshStatus();

        _chargeService.Received().GetCurrentStopThreshold();
    }

    [Fact]
    public void RefreshStatus_WhenInactive_DoesNotQueryThreshold()
    {
        _chargeService.IsActive().Returns(false);

        _sut.RefreshStatus();

        _chargeService.DidNotReceive().GetCurrentStopThreshold();
    }

    [Fact]
    public void RefreshStatus_WhenServiceThrows_DoesNotCrash()
    {
        _chargeService.IsActive().Throws(new InvalidOperationException("ChargeThreshold.exe nicht gefunden"));

        // Keine Exception erwartet – Fehler wird intern behandelt
        var ex = Record.Exception(() => _sut.RefreshStatus());
        Assert.Null(ex);
    }

    // ── OnEnableClick ─────────────────────────────────────────────────────────

    [Fact]
    public void OnEnableClick_WithStop80_CallsEnableWithCorrectArgs()
    {
        _sut.OnEnableClick(80, null);

        _chargeService.Received(1).Enable(80, null);
    }

    [Fact]
    public void OnEnableClick_WithStop80Start60_CallsEnableWithBothArgs()
    {
        _sut.OnEnableClick(80, 60);

        _chargeService.Received(1).Enable(80, 60);
    }

    [Fact]
    public void OnEnableClick_WhenServiceThrows_DoesNotCrash()
    {
        _chargeService.When(s => s.Enable(Arg.Any<int>(), Arg.Any<int?>()))
                      .Throw(new InvalidOperationException("Fehler"));

        var ex = Record.Exception(() => _sut.OnEnableClick(80, null));
        Assert.Null(ex);
    }

    // ── OnDisableClick ────────────────────────────────────────────────────────

    [Fact]
    public void OnDisableClick_CallsServiceDisable()
    {
        _sut.OnDisableClick();

        _chargeService.Received(1).Disable();
    }

    [Fact]
    public void OnDisableClick_WhenServiceThrows_DoesNotCrash()
    {
        _chargeService.When(s => s.Disable()).Throw(new InvalidOperationException("Fehler"));

        var ex = Record.Exception(() => _sut.OnDisableClick());
        Assert.Null(ex);
    }

    // ── OnAutostartClick ──────────────────────────────────────────────────────

    [Fact]
    public void OnAutostartClick_CallsAutostartToggle()
    {
        _sut.OnAutostartClick();

        _autostartService.Received(1).Toggle();
    }

    [Fact]
    public void OnAutostartClick_WhenServiceThrows_DoesNotCrash()
    {
        _autostartService.When(s => s.Toggle()).Throw(new Exception("Registry-Fehler"));

        var ex = Record.Exception(() => _sut.OnAutostartClick());
        Assert.Null(ex);
    }

    // ── RefreshAutostart ──────────────────────────────────────────────────────

    [Fact]
    public void RefreshAutostart_QueriesAutostartService()
    {
        _sut.RefreshAutostart();

        _autostartService.Received().IsEnabled();
    }
}
