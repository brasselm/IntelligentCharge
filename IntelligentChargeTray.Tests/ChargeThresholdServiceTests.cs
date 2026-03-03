using IntelligentChargeTray.Services;

namespace IntelligentChargeTray.Tests;

/// <summary>
/// Tests für die statischen Parsing-Methoden von ChargeThresholdService.
/// Ausgabeformat basiert auf echten ChargeThreshold.exe-Ausgaben (outputs.txt).
/// Kein Prozess wird gestartet – reiner Unit-Test der Logik.
/// </summary>
public class ChargeThresholdServiceParsingTests
{
    // Reale Ausgaben aus outputs.txt
    private const string OutputOff     = "Charge threshold for Battery #1: OFF.";
    private const string OutputStop80Start60 = "Charge threshold for Battery #1: Start at 60%, Stop at 80%.";
    private const string OutputStop80Start75 = "Charge threshold for Battery #1: Start at 75%, Stop at 80%.";

    // ── ParseIsActive ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(OutputStop80Start60, true)]
    [InlineData(OutputStop80Start75, true)]
    [InlineData(OutputOff, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void ParseIsActive_RealOutputs_ReturnsExpected(string rawOutput, bool expected)
    {
        bool result = ChargeThresholdService.ParseIsActive(rawOutput);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseIsActive_CaseInsensitive()
    {
        Assert.True(ChargeThresholdService.ParseIsActive(
            "Charge threshold for Battery #1: start at 60%, stop at 80%."));
    }

    // ── ParseStopThreshold ────────────────────────────────────────────────────

    [Theory]
    [InlineData(OutputStop80Start60, 80)]
    [InlineData(OutputStop80Start75, 80)]
    public void ParseStopThreshold_WhenActive_ReturnsStopValue(string rawOutput, int expected)
    {
        int? result = ChargeThresholdService.ParseStopThreshold(rawOutput);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseStopThreshold_WhenOff_ReturnsNull()
    {
        int? result = ChargeThresholdService.ParseStopThreshold(OutputOff);
        Assert.Null(result);
    }

    [Fact]
    public void ParseStopThreshold_ReturnsStopNotStart()
    {
        // Stop = 80, Start = 60 → es muss 80 zurückgegeben werden, nicht 60
        int? result = ChargeThresholdService.ParseStopThreshold(OutputStop80Start60);
        Assert.Equal(80, result);
    }

    // ── ParseStartThreshold ───────────────────────────────────────────────────

    [Theory]
    [InlineData(OutputStop80Start60, 60)]
    [InlineData(OutputStop80Start75, 75)]
    public void ParseStartThreshold_WhenActive_ReturnsStartValue(string rawOutput, int expected)
    {
        int? result = ChargeThresholdService.ParseStartThreshold(rawOutput);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseStartThreshold_WhenOff_ReturnsNull()
    {
        int? result = ChargeThresholdService.ParseStartThreshold(OutputOff);
        Assert.Null(result);
    }

    [Fact]
    public void ParseStartThreshold_ReturnsStartNotStop()
    {
        // Start = 60, Stop = 80 → es muss 60 zurückgegeben werden, nicht 80
        int? result = ChargeThresholdService.ParseStartThreshold(OutputStop80Start60);
        Assert.Equal(60, result);
    }
}
