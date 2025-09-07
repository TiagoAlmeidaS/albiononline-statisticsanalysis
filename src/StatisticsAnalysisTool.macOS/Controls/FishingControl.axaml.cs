using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Fishing control with Albion Online visual style for bot fishing functionality
/// </summary>
public partial class FishingControl : UserControl
{
    private bool _isBotRunning = false;
    private readonly ObservableCollection<ActivityLogEntry> _activityLog = new();

    public FishingControl()
    {
        InitializeComponent();
        SetupFishingControl();
    }

    private void SetupFishingControl()
    {
        // Initialize fishing control
        ActivityLogListBox.ItemsSource = _activityLog;
        
        // Add initial log entries
        AddLogEntry("Bot initialized", DateTime.Now, "Info");
        AddLogEntry("Ready to start fishing", DateTime.Now, "Info");
    }

    #region Event Handlers

    private void StopBot_Click(object? sender, RoutedEventArgs e)
    {
        if (_isBotRunning)
        {
            _isBotRunning = false;
            UpdateBotStatus(false);
            AddLogEntry("Bot stopped", DateTime.Now, "Info");
        }
    }

    private void StartBot_Click(object? sender, RoutedEventArgs e)
    {
        if (!_isBotRunning)
        {
            _isBotRunning = true;
            UpdateBotStatus(true);
            AddLogEntry("Bot started", DateTime.Now, "Info");
        }
    }

    private void OpenSettings_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement settings opening
        AddLogEntry("Settings requested", DateTime.Now, "Info");
    }

    private void ZoomIn_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement map zoom in
        AddLogEntry("Map zoomed in", DateTime.Now, "Info");
    }

    private void ZoomOut_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement map zoom out
        AddLogEntry("Map zoomed out", DateTime.Now, "Info");
    }

    private void ResetZoom_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement map zoom reset
        AddLogEntry("Map zoom reset", DateTime.Now, "Info");
    }

    private void CaptureScreen_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement screen capture
        AddLogEntry("Screen capture taken", DateTime.Now, "Info");
    }

    private void TestDetection_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement detection test
        AddLogEntry("Detection test performed", DateTime.Now, "Info");
    }

    private void ClearLog_Click(object? sender, RoutedEventArgs e)
    {
        _activityLog.Clear();
        AddLogEntry("Activity log cleared", DateTime.Now, "Info");
    }

    #endregion

    #region Private Methods

    private void UpdateBotStatus(bool isRunning)
    {
        // Update status indicator
        if (isRunning)
        {
            StatusIndicator.Background = Avalonia.Media.Brushes.Green;
        }
        else
        {
            StatusIndicator.Background = Avalonia.Media.Brushes.Red;
        }
    }

    private void AddLogEntry(string message, DateTime timestamp, string type)
    {
        var entry = new ActivityLogEntry
        {
            Timestamp = timestamp,
            Message = message,
            Type = type
        };
        
        _activityLog.Add(entry);
        
        // Keep only last 100 entries
        if (_activityLog.Count > 100)
        {
            _activityLog.RemoveAt(0);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the fish caught counter
    /// </summary>
    /// <param name="count">Number of fish caught</param>
    public void UpdateFishCaught(int count)
    {
        FishCaughtText.Text = count.ToString();
    }

    /// <summary>
    /// Updates the trash counter
    /// </summary>
    /// <param name="count">Number of trash items</param>
    public void UpdateTrash(int count)
    {
        TrashText.Text = count.ToString();
    }

    /// <summary>
    /// Updates the bait status
    /// </summary>
    /// <param name="hasBait">Whether player has bait</param>
    public void UpdateBaitStatus(bool hasBait)
    {
        BaitStatusText.Text = hasBait ? "YES" : "NO";
    }

    /// <summary>
    /// Updates the food status
    /// </summary>
    /// <param name="hasFood">Whether player has food</param>
    public void UpdateFoodStatus(bool hasFood)
    {
        FoodStatusText.Text = hasFood ? "YES" : "NO";
    }

    /// <summary>
    /// Updates the failures counter
    /// </summary>
    /// <param name="count">Number of failures</param>
    public void UpdateFailures(int count)
    {
        FailuresText.Text = count.ToString();
    }

    /// <summary>
    /// Updates the player's health percentage
    /// </summary>
    /// <param name="percentage">Health percentage (0-100)</param>
    public void UpdateHealthPercentage(double percentage)
    {
        HealthProgressBar.Value = Math.Max(0, Math.Min(100, percentage));
    }

    /// <summary>
    /// Updates the player's energy percentage
    /// </summary>
    /// <param name="percentage">Energy percentage (0-100)</param>
    public void UpdateEnergyPercentage(double percentage)
    {
        EnergyProgressBar.Value = Math.Max(0, Math.Min(100, percentage));
    }

    /// <summary>
    /// Updates the player's silver amount
    /// </summary>
    /// <param name="silver">Silver amount</param>
    public void UpdateSilver(long silver)
    {
        SilverText.Text = silver.ToString("N0");
    }

    /// <summary>
    /// Updates the bobber detection status
    /// </summary>
    /// <param name="detected">Whether bobber is detected</param>
    public void UpdateBobberDetection(bool detected)
    {
        if (detected)
        {
            BobberIndicator.Background = Avalonia.Media.Brushes.Green;
            BobberStatusText.Text = "Detected";
        }
        else
        {
            BobberIndicator.Background = Avalonia.Media.Brushes.Red;
            BobberStatusText.Text = "Not Detected";
        }
    }

    /// <summary>
    /// Updates the connection status
    /// </summary>
    /// <param name="connected">Whether connected</param>
    public void UpdateConnectionStatus(bool connected)
    {
        if (connected)
        {
            ConnectionIndicator.Background = Avalonia.Media.Brushes.Green;
            ConnectionStatusText.Text = "Connected";
        }
        else
        {
            ConnectionIndicator.Background = Avalonia.Media.Brushes.Red;
            ConnectionStatusText.Text = "Disconnected";
        }
    }

    #endregion
}

/// <summary>
/// Represents an activity log entry
/// </summary>
public class ActivityLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}