using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
namespace Telemetry.UiTests;

internal static class Cf
{
    private static readonly ConditionFactory F = new(new UIA3PropertyLibrary());
    public static ConditionBase ById(string id) => F.ByAutomationId(id);
    public static ConditionBase ByName(string name) => F.ByName(name);
}

/// <summary>
/// FlaUI smoke test: launch the WPF dashboard, ensure it loads, and (when API is available) refresh runs and queue one.
/// Build the Dashboard first: dotnet build src/Telemetry.Dashboard/Telemetry.Dashboard.csproj
/// For full flow (load runs, queue), run the API at http://localhost:5244 before running tests.
/// </summary>
public class DashboardSmokeTests
{
    private static string GetDashboardExePath()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDir, "..", "..", "..", "..", "..", "src", "Telemetry.Dashboard", "bin", "Debug", "net8.0-windows", "Telemetry.Dashboard.exe");
        var full = Path.GetFullPath(candidate);
        if (File.Exists(full)) return full;
        throw new InvalidOperationException($"Dashboard exe not found at {full}. Build the Dashboard project first.");
    }

    [Fact]
    public void Dashboard_Launches_And_Shows_Main_Window()
    {
        var exePath = GetDashboardExePath();
        using var app = Application.Launch(exePath);
        using var automation = new UIA3Automation();

        var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(window);
        Assert.Contains("Telemetry", window.Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_Refresh_And_Queue_Run_Changes_State_When_API_Available()
    {
        var exePath = GetDashboardExePath();
        using var app = Application.Launch(exePath);
        using var automation = new UIA3Automation();

        var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(window);

        var cf = new ConditionFactory(new UIA3PropertyLibrary());
        var refreshButton = window.FindFirstDescendant(Cf.ById("RefreshButton")) ?? window.FindFirstDescendant(Cf.ByName("Refresh runs"));
        Assert.NotNull(refreshButton);
        refreshButton.AsButton().Invoke();

        Thread.Sleep(2000);

        var runsGrid = window.FindFirstDescendant(Cf.ById("RunsGrid"));
        if (runsGrid != null)
        {
            var rows = runsGrid.FindAllDescendants(cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            if (rows.Length > 0)
            {
                rows[0].AsGridRow().Select();
                Thread.Sleep(500);
                var queueButton = window.FindFirstDescendant(Cf.ById("QueueButton")) ?? window.FindFirstDescendant(Cf.ByName("Queue"));
                if (queueButton?.AsButton().IsEnabled == true)
                {
                    queueButton.AsButton().Invoke();
                    Thread.Sleep(1500);
                    var detailTitle = window.FindFirstDescendant(Cf.ById("DetailTitle"));
                    var titleText = detailTitle?.Name ?? "";
                    Assert.Contains("Queued", titleText, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
