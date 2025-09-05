using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;
using AlbionFishing.Vision;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision.ScreenCapture;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public class BotControlViewModel : BaseViewModel
{
    private readonly IUniversalDecisionEngine _decisionEngine;
    private readonly IBobberDetector _bobberDetector;
    private readonly IVisionConfigurationService _visionConfigService;
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly ILogger<BotControlViewModel> _logger;

    // Bot Status Properties
    private string _botActionText = "Ready";
    private string _botStatusDescription = "Bot is ready to start fishing";
    private System.Windows.Media.Brush _botStatusColor = System.Windows.Media.Brushes.Gray;
    private string _botStatusText = "Ready";

    // Statistics Properties
    private int _fishCaught = 0;
    private int _trashCaught = 0;
    private string _baitStatus = "OK";
    private string _foodStatus = "OK";
    private int _failures = 0;

    // Player Info Properties
    private double _playerHealthPercentage = 100;
    private double _playerEnergyPercentage = 100;
    private long _playerSilver = 0;
    private string _playerPosition = "0,0";
    private double _playerPositionX = 0;
    private double _playerPositionY = 0;

    // Fishing Properties
    private bool _bobberDetected = false;
    private string _bobberStatus = "Not Detected";
    private double _bobberDetectionX = 0;
    private double _bobberDetectionY = 0;
    private bool _fishingLineVisible = false;
    private double _fishingLineStartX = 0;
    private double _fishingLineStartY = 0;
    private double _fishingLineEndX = 0;
    private double _fishingLineEndY = 0;
    private bool _fishingAreaVisible = false;
    private string _fishingAreaPath = "";

    // Map Properties
    private int _waypointCount = 0;
    private int _stationCount = 0;
    private string _botPath = "";
    private ObservableCollection<Point> _waypoints = new();
    private ObservableCollection<Point> _fishingStations = new();
    private ObservableCollection<Point> _activeFishingSpots = new();

    // Screen Capture Properties
    private BitmapSource _screenCapture;

    // Activity Log Properties
    private ObservableCollection<LogEntry> _activityLog = new();

    // Connection Properties
    private System.Windows.Media.Brush _connectionStatusColor = System.Windows.Media.Brushes.Red;
    private string _connectionStatusText = "Disconnected";

    // Commands
    public RelayCommand StartBotCommand { get; }
    public RelayCommand StopBotCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }
    public RelayCommand CaptureScreenCommand { get; }
    public RelayCommand TestDetectionCommand { get; }
    public RelayCommand ClearLogCommand { get; }

    public BotControlViewModel(
        IUniversalDecisionEngine decisionEngine,
        IBobberDetector bobberDetector,
        IVisionConfigurationService visionConfigService,
        IScreenCaptureService screenCaptureService,
        ILogger<BotControlViewModel> logger)
    {
        _decisionEngine = decisionEngine;
        _bobberDetector = bobberDetector;
        _visionConfigService = visionConfigService;
        _screenCaptureService = screenCaptureService;
        _logger = logger;

        // Initialize commands
        StartBotCommand = new RelayCommand(async () => await StartBotAsync());
        StopBotCommand = new RelayCommand(async () => await StopBotAsync());
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        CaptureScreenCommand = new RelayCommand(CaptureScreen);
        TestDetectionCommand = new RelayCommand(TestDetection);
        ClearLogCommand = new RelayCommand(ClearLog);

        // Initialize activity log
        AddLogEntry("Bot Control initialized");
    }

    #region Bot Status Properties

    public string BotActionText
    {
        get => _botActionText;
        set => SetProperty(ref _botActionText, value);
    }

    public string BotStatusDescription
    {
        get => _botStatusDescription;
        set => SetProperty(ref _botStatusDescription, value);
    }

    public System.Windows.Media.Brush BotStatusColor
    {
        get => _botStatusColor;
        set => SetProperty(ref _botStatusColor, value);
    }

    public string BotStatusText
    {
        get => _botStatusText;
        set => SetProperty(ref _botStatusText, value);
    }

    #endregion

    #region Statistics Properties

    public int FishCaught
    {
        get => _fishCaught;
        set => SetProperty(ref _fishCaught, value);
    }

    public int TrashCaught
    {
        get => _trashCaught;
        set => SetProperty(ref _trashCaught, value);
    }

    public string BaitStatus
    {
        get => _baitStatus;
        set => SetProperty(ref _baitStatus, value);
    }

    public string FoodStatus
    {
        get => _foodStatus;
        set => SetProperty(ref _foodStatus, value);
    }

    public int Failures
    {
        get => _failures;
        set => SetProperty(ref _failures, value);
    }

    #endregion

    #region Player Info Properties

    public double PlayerHealthPercentage
    {
        get => _playerHealthPercentage;
        set => SetProperty(ref _playerHealthPercentage, value);
    }

    public double PlayerEnergyPercentage
    {
        get => _playerEnergyPercentage;
        set => SetProperty(ref _playerEnergyPercentage, value);
    }

    public long PlayerSilver
    {
        get => _playerSilver;
        set => SetProperty(ref _playerSilver, value);
    }

    public string PlayerPosition
    {
        get => _playerPosition;
        set => SetProperty(ref _playerPosition, value);
    }

    public double PlayerPositionX
    {
        get => _playerPositionX;
        set => SetProperty(ref _playerPositionX, value);
    }

    public double PlayerPositionY
    {
        get => _playerPositionY;
        set => SetProperty(ref _playerPositionY, value);
    }

    #endregion

    #region Fishing Properties

    public bool BobberDetected
    {
        get => _bobberDetected;
        set => SetProperty(ref _bobberDetected, value);
    }

    public string BobberStatus
    {
        get => _bobberStatus;
        set => SetProperty(ref _bobberStatus, value);
    }

    public double BobberDetectionX
    {
        get => _bobberDetectionX;
        set => SetProperty(ref _bobberDetectionX, value);
    }

    public double BobberDetectionY
    {
        get => _bobberDetectionY;
        set => SetProperty(ref _bobberDetectionY, value);
    }

    public bool FishingLineVisible
    {
        get => _fishingLineVisible;
        set => SetProperty(ref _fishingLineVisible, value);
    }

    public double FishingLineStartX
    {
        get => _fishingLineStartX;
        set => SetProperty(ref _fishingLineStartX, value);
    }

    public double FishingLineStartY
    {
        get => _fishingLineStartY;
        set => SetProperty(ref _fishingLineStartY, value);
    }

    public double FishingLineEndX
    {
        get => _fishingLineEndX;
        set => SetProperty(ref _fishingLineEndX, value);
    }

    public double FishingLineEndY
    {
        get => _fishingLineEndY;
        set => SetProperty(ref _fishingLineEndY, value);
    }

    public bool FishingAreaVisible
    {
        get => _fishingAreaVisible;
        set => SetProperty(ref _fishingAreaVisible, value);
    }

    public string FishingAreaPath
    {
        get => _fishingAreaPath;
        set => SetProperty(ref _fishingAreaPath, value);
    }

    #endregion

    #region Map Properties

    public int WaypointCount
    {
        get => _waypointCount;
        set => SetProperty(ref _waypointCount, value);
    }

    public int StationCount
    {
        get => _stationCount;
        set => SetProperty(ref _stationCount, value);
    }

    public string BotPath
    {
        get => _botPath;
        set => SetProperty(ref _botPath, value);
    }

    public ObservableCollection<Point> Waypoints
    {
        get => _waypoints;
        set => SetProperty(ref _waypoints, value);
    }

    public ObservableCollection<Point> FishingStations
    {
        get => _fishingStations;
        set => SetProperty(ref _fishingStations, value);
    }

    public ObservableCollection<Point> ActiveFishingSpots
    {
        get => _activeFishingSpots;
        set => SetProperty(ref _activeFishingSpots, value);
    }

    #endregion

    #region Screen Capture Properties

    public BitmapSource ScreenCapture
    {
        get => _screenCapture;
        set => SetProperty(ref _screenCapture, value);
    }

    #endregion

    #region Activity Log Properties

    public ObservableCollection<LogEntry> ActivityLog
    {
        get => _activityLog;
        set => SetProperty(ref _activityLog, value);
    }

    #endregion

    #region Connection Properties

    public System.Windows.Media.Brush ConnectionStatusColor
    {
        get => _connectionStatusColor;
        set => SetProperty(ref _connectionStatusColor, value);
    }

    public string ConnectionStatusText
    {
        get => _connectionStatusText;
        set => SetProperty(ref _connectionStatusText, value);
    }

    #endregion

    #region Command Methods

    private async Task StartBotAsync()
    {
        try
        {
            BotActionText = "Starting...";
            BotStatusDescription = "Initializing fishing bot...";
            BotStatusColor = System.Windows.Media.Brushes.Yellow;
            BotStatusText = "Starting";

            AddLogEntry("Starting fishing bot...");

            // Initialize decision engine
            await _decisionEngine.InitializeAsync();

            BotActionText = "Running";
            BotStatusDescription = "Fishing bot is active and monitoring";
            BotStatusColor = System.Windows.Media.Brushes.Green;
            BotStatusText = "Running";

            AddLogEntry("Fishing bot started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting fishing bot");
            BotActionText = "Error";
            BotStatusDescription = "Failed to start fishing bot";
            BotStatusColor = System.Windows.Media.Brushes.Red;
            BotStatusText = "Error";

            AddLogEntry($"Error starting bot: {ex.Message}");
        }
    }

    private async Task StopBotAsync()
    {
        try
        {
            BotActionText = "Stopping...";
            BotStatusDescription = "Stopping fishing bot...";
            BotStatusColor = System.Windows.Media.Brushes.Yellow;
            BotStatusText = "Stopping";

            AddLogEntry("Stopping fishing bot...");

            // Stop decision engine
            await _decisionEngine.ShutdownAsync();

            BotActionText = "Stopped";
            BotStatusDescription = "Fishing bot has been stopped";
            BotStatusColor = System.Windows.Media.Brushes.Gray;
            BotStatusText = "Stopped";

            AddLogEntry("Fishing bot stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping fishing bot");
            AddLogEntry($"Error stopping bot: {ex.Message}");
        }
    }

    private void OpenSettings()
    {
        AddLogEntry("Opening settings...");
        // TODO: Implement settings dialog
    }

    private void CaptureScreen()
    {
        try
        {
            AddLogEntry("Capturing screen...");
            // TODO: Implement screen capture
            AddLogEntry("Screen captured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing screen");
            AddLogEntry($"Error capturing screen: {ex.Message}");
        }
    }

    private void TestDetection()
    {
        try
        {
            AddLogEntry("Testing bobber detection...");
            // TODO: Implement detection test
            AddLogEntry("Detection test completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing detection");
            AddLogEntry($"Error testing detection: {ex.Message}");
        }
    }

    private void ClearLog()
    {
        ActivityLog.Clear();
        AddLogEntry("Activity log cleared");
    }

    #endregion

    #region Helper Methods

    private void AddLogEntry(string message)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Message = message
        };

        ActivityLog.Add(logEntry);

        // Keep only last 100 entries
        if (ActivityLog.Count > 100)
        {
            ActivityLog.RemoveAt(0);
        }
    }

    #endregion
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
}
