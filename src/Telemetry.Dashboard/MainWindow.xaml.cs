using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Telemetry.Dashboard;

public partial class MainWindow : Window
{
    private ApiClient? _client;
    private RunDto? _selectedRun;
    private readonly ObservableCollection<RunRow> _runs = new();
    private readonly ObservableCollection<TimelineRow> _timeline = new();

    public MainWindow()
    {
        InitializeComponent();
        BaseUrlBox.Text = "http://localhost:5244";
        RunsGrid.ItemsSource = _runs;
        TimelineList.ItemsSource = _timeline;
    }

    private string BaseUrl => BaseUrlBox.Text.Trim();

    private void EnsureClient()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            StatusText.Text = "Enter API base URL.";
            return;
        }
        try
        {
            _client = new ApiClient(BaseUrl);
        }
        catch (Exception ex)
        {
            StatusText.Text = "Invalid URL: " + ex.Message;
            _client = null;
        }
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        EnsureClient();
        if (_client == null) return;
        StatusText.Text = "Loading...";
        try
        {
            var list = await _client.GetRunsAsync(50);
            _runs.Clear();
            foreach (var r in list)
                _runs.Add(new RunRow(r));
            StatusText.Text = $"Loaded {list.Count} runs.";
            _selectedRun = null;
            UpdateSelectionState();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Error: " + ex.Message;
            _runs.Clear();
        }
    }

    private async void RunsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RunsGrid.SelectedItem is not RunRow row)
        {
            _selectedRun = null;
            UpdateSelectionState();
            return;
        }
        _selectedRun = row.Run;
        DetailTitle.Text = $"Run {_selectedRun.Id:N} — {_selectedRun.CurrentState}";
        _timeline.Clear();
        if (_client == null) return;
        try
        {
            var tl = await _client.GetTimelineAsync(_selectedRun.Id);
            if (tl != null)
            {
                foreach (var evt in tl.Events)
                    _timeline.Add(new TimelineRow(evt));
            }
        }
        catch
        {
            _timeline.Add(new TimelineRow(null, "Failed to load timeline."));
        }
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        if (_selectedRun == null)
        {
            QueueButton.IsEnabled = StartButton.IsEnabled = CancelButton.IsEnabled = false;
            return;
        }
        var s = _selectedRun.CurrentState;
        QueueButton.IsEnabled = s == "Created";
        StartButton.IsEnabled = s == "Queued";
        CancelButton.IsEnabled = s is "Created" or "Queued" or "Running";
    }

    private async void QueueButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_client == null || _selectedRun == null) return;
        await TransitionAsync(() => _client.QueueAsync(_selectedRun.Id));
    }

    private async void StartButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_client == null || _selectedRun == null) return;
        await TransitionAsync(() => _client.StartAsync(_selectedRun.Id));
    }

    private async void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_client == null || _selectedRun == null) return;
        await TransitionAsync(() => _client.CancelAsync(_selectedRun.Id));
    }

    private async System.Threading.Tasks.Task TransitionAsync(Func<System.Threading.Tasks.Task<RunDto?>> action)
    {
        if (_selectedRun == null) return;
        StatusText.Text = "Updating...";
        try
        {
            var updated = await action();
            if (updated != null)
            {
                _selectedRun = updated;
                DetailTitle.Text = $"Run {_selectedRun.Id:N} — {_selectedRun.CurrentState}";
                UpdateSelectionState();
                var idx = _runs.ToList().FindIndex(r => r.Run.Id == updated.Id);
                if (idx >= 0)
                {
                    _runs[idx] = new RunRow(updated);
                    RunsGrid.SelectedIndex = idx;
                }
                StatusText.Text = "Updated.";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = "Error: " + ex.Message;
        }
    }

    private sealed class RunRow
    {
        public string Id => Run.Id.ToString("N")[..8];
        public string SampleId => Run.SampleId;
        public string CurrentState => Run.CurrentState;
        public DateTime CreatedAt => Run.CreatedAt;
        public RunDto Run { get; }
        public RunRow(RunDto run) => Run = run;
    }

    private sealed class TimelineRow
    {
        public string Display { get; }
        public TimelineRow(RunTimelineEventDto? e, string? fallback = null)
        {
            Display = e != null
                ? $"{e.Timestamp:yyyy-MM-dd HH:mm:ss} {e.EventType} {e.Actor ?? ""}"
                : (fallback ?? "");
        }
    }
}
