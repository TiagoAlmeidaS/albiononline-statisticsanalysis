using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.DecisionEngine.Core;
using StatisticsAnalysisTool.DecisionEngine.Models;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;
using AlbionFishing.Vision.Detectors;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision.ScreenCapture;
using AlbionFishing.Vision;

namespace StatisticsAnalysisTool.ViewModels;

/// <summary>
/// ViewModel para a janela principal do bot de pesca
/// </summary>
public class FishingWindowViewModel : BaseViewModel
{
    private readonly DecisionEngine.Abstractions.IUniversalDecisionEngine _decisionEngine;
    private readonly IBobberDetector _bobberDetector;
    private readonly IVisionConfigurationService _visionConfigService;
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly ILogger<FishingWindowViewModel> _logger;
    
    // Status do bot
    private bool _isBotActive;
    private string _botStatusText = "INACTIVE";
    private System.Windows.Media.Brush _botStatusColor = System.Windows.Media.Brushes.Red;
    private string _botActionText = "Waiting for action...";
    private string _botStatusDescription = "Bot is not running";
    
    // Estatísticas
    private int _fishCaught;
    private int _trashCaught;
    private string _baitStatus = "NO";
    private string _foodStatus = "NO";
    private int _failures;
    
    // Informações do jogador
    private float _playerHealthPercentage = 100;
    private float _playerEnergyPercentage = 100;
    private long _playerSilver;
    private string _playerPosition = "0.0, 0.0";
    private double _playerPositionX;
    private double _playerPositionY;
    
    // Minimapa
    private int _waypointCount;
    private int _stationCount;
    private double _zoomLevel = 1.0;
    private PointCollection _botPath = new();
    private ObservableCollection<MapPoint> _waypoints = new();
    private ObservableCollection<MapPoint> _fishingStations = new();
    
    // Detecção de bobber
    private bool _bobberDetected;
    private string _bobberStatus = "Not detected";
    private double _bobberDetectionX;
    private double _bobberDetectionY;
    private bool _fishingLineVisible;
    private double _fishingLineStartX;
    private double _fishingLineStartY;
    private double _fishingLineEndX;
    private double _fishingLineEndY;
    private bool _fishingAreaVisible;
    private string _fishingAreaPath = "";
    private ObservableCollection<MapPoint> _activeFishingSpots = new();
    
    // Captura de tela
    private BitmapSource _screenCapture;
    private bool _isScreenCaptureActive;
    
    // Status de conexão
    private string _connectionStatusText = "Disconnected";
    private System.Windows.Media.Brush _connectionStatusColor = System.Windows.Media.Brushes.Red;
    
    // Log de atividades
    private ObservableCollection<ActivityLogEntry> _activityLog = new();
    
    public FishingWindowViewModel(
        DecisionEngine.Abstractions.IUniversalDecisionEngine decisionEngine,
        IBobberDetector bobberDetector,
        IVisionConfigurationService visionConfigService,
        IScreenCaptureService screenCaptureService,
        ILogger<FishingWindowViewModel> logger)
    {
        _decisionEngine = decisionEngine;
        _bobberDetector = bobberDetector;
        _visionConfigService = visionConfigService;
        _screenCaptureService = screenCaptureService;
        _logger = logger;
        
        // Inicializar comandos
        InitializeCommands();
        
        // Subscrever eventos
        SubscribeToEvents();
        
        // Inicializar dados
        InitializeData();
    }
    
    #region Propriedades Públicas
    
    // Status do bot
    public bool IsBotActive
    {
        get => _isBotActive;
        set => SetProperty(ref _isBotActive, value);
    }
    
    public string BotStatusText
    {
        get => _botStatusText;
        set => SetProperty(ref _botStatusText, value);
    }
    
    public System.Windows.Media.Brush BotStatusColor
    {
        get => _botStatusColor;
        set => SetProperty(ref _botStatusColor, value);
    }
    
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
    
    // Estatísticas
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
    
    // Informações do jogador
    public float PlayerHealthPercentage
    {
        get => _playerHealthPercentage;
        set => SetProperty(ref _playerHealthPercentage, value);
    }
    
    public float PlayerEnergyPercentage
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
    
    // Minimapa
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
    
    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, value);
    }
    
    public PointCollection BotPath
    {
        get => _botPath;
        set => SetProperty(ref _botPath, value);
    }
    
    public ObservableCollection<MapPoint> Waypoints
    {
        get => _waypoints;
        set => SetProperty(ref _waypoints, value);
    }
    
    public ObservableCollection<MapPoint> FishingStations
    {
        get => _fishingStations;
        set => SetProperty(ref _fishingStations, value);
    }
    
    // Detecção de bobber
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
    
    public ObservableCollection<MapPoint> ActiveFishingSpots
    {
        get => _activeFishingSpots;
        set => SetProperty(ref _activeFishingSpots, value);
    }
    
    // Captura de tela
    public BitmapSource ScreenCapture
    {
        get => _screenCapture;
        set => SetProperty(ref _screenCapture, value);
    }
    
    public bool IsScreenCaptureActive
    {
        get => _isScreenCaptureActive;
        set => SetProperty(ref _isScreenCaptureActive, value);
    }
    
    // Status de conexão
    public string ConnectionStatusText
    {
        get => _connectionStatusText;
        set => SetProperty(ref _connectionStatusText, value);
    }
    
    public System.Windows.Media.Brush ConnectionStatusColor
    {
        get => _connectionStatusColor;
        set => SetProperty(ref _connectionStatusColor, value);
    }
    
    // Log de atividades
    public ObservableCollection<ActivityLogEntry> ActivityLog
    {
        get => _activityLog;
        set => SetProperty(ref _activityLog, value);
    }
    
    #endregion
    
    #region Comandos
    
    public RelayCommand StartBotCommand { get; private set; }
    public RelayCommand StopBotCommand { get; private set; }
    public RelayCommand OpenSettingsCommand { get; private set; }
    
    #endregion
    
    #region Métodos Privados
    
    private void InitializeCommands()
    {
        StartBotCommand = new RelayCommand(async () => await StartBotAsync());
        StopBotCommand = new RelayCommand(async () => await StopBotAsync());
        OpenSettingsCommand = new RelayCommand(OpenSettings);
    }
    
    private void SubscribeToEvents()
    {
        // Subscrever eventos do motor de decisão
        _decisionEngine.DecisionMade += OnDecisionMade;
        _decisionEngine.ContextChanged += OnContextChanged;
        // BehaviorExecuted não existe na interface atual
        
        // Subscrever eventos de detecção
        if (_bobberDetector != null)
        {
            // Assumindo que o detector tem eventos
            // _bobberDetector.BobberDetected += OnBobberDetected;
        }
    }
    
    private void InitializeData()
    {
        // Inicializar dados do minimapa
        InitializeMinimapData();
        
        // Inicializar pontos de pesca
        InitializeFishingSpots();
        
        // Inicializar log
        AddActivityLog("Sistema inicializado", "Info");
    }
    
    private void InitializeMinimapData()
    {
        // Simular dados do minimapa
        BotPath = new PointCollection
        {
            new System.Windows.Point(50, 50),
            new System.Windows.Point(100, 80),
            new System.Windows.Point(150, 60),
            new System.Windows.Point(200, 100)
        };
        
        Waypoints.Clear();
        for (int i = 0; i < 10; i++)
        {
            Waypoints.Add(new MapPoint
            {
                X = 50 + (i * 15),
                Y = 50 + (i % 3) * 20,
                Type = "Waypoint"
            });
        }
        
        FishingStations.Clear();
        for (int i = 0; i < 3; i++)
        {
            FishingStations.Add(new MapPoint
            {
                X = 80 + (i * 60),
                Y = 70 + (i * 10),
                Type = "FishingStation"
            });
        }
        
        WaypointCount = Waypoints.Count;
        StationCount = FishingStations.Count;
    }
    
    private void InitializeFishingSpots()
    {
        ActiveFishingSpots.Clear();
        for (int i = 0; i < 5; i++)
        {
            ActiveFishingSpots.Add(new MapPoint
            {
                X = 100 + (i * 30),
                Y = 150 + (i * 20),
                Type = "FishingSpot"
            });
        }
    }
    
    private async Task StartBotAsync()
    {
        try
        {
            AddActivityLog("Iniciando bot de pesca...", "Info");
            
            IsBotActive = true;
            BotStatusText = "ACTIVE";
            BotStatusColor = System.Windows.Media.Brushes.Green;
            BotActionText = "FISHING";
            BotStatusDescription = "Bot is actively fishing";
            
            // Iniciar captura de tela
            StartScreenCapture();
            
            // Iniciar detecção de bobber
            await StartBobberDetection();
            
            AddActivityLog("Bot iniciado com sucesso", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar bot");
            AddActivityLog($"Erro ao iniciar bot: {ex.Message}", "Error");
        }
    }
    
    private async Task StopBotAsync()
    {
        try
        {
            AddActivityLog("Parando bot de pesca...", "Info");
            
            IsBotActive = false;
            BotStatusText = "INACTIVE";
            BotStatusColor = System.Windows.Media.Brushes.Red;
            BotActionText = "Waiting for action...";
            BotStatusDescription = "Bot is not running";
            
            // Parar captura de tela
            StopScreenCapture();
            
            // Parar detecção de bobber
            await StopBobberDetection();
            
            AddActivityLog("Bot parado", "Info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar bot");
            AddActivityLog($"Erro ao parar bot: {ex.Message}", "Error");
        }
    }
    
    private void OpenSettings()
    {
        try
        {
            // Abrir janela de configurações
            AddActivityLog("Abrindo configurações...", "Info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao abrir configurações");
            AddActivityLog($"Erro ao abrir configurações: {ex.Message}", "Error");
        }
    }
    
    public void StartScreenCapture()
    {
        try
        {
            IsScreenCaptureActive = true;
            // Implementar captura de tela contínua
            _ = Task.Run(async () =>
            {
                while (IsScreenCaptureActive)
                {
                    try
                    {
                        var capture = _screenCaptureService.CaptureScreen();
                        if (capture != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ScreenCapture = ConvertToBitmapSource(capture);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro na captura de tela");
                    }
                    
                    await Task.Delay(100); // 10 FPS
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar captura de tela");
        }
    }
    
    public void StopScreenCapture()
    {
        IsScreenCaptureActive = false;
    }
    
    private async Task StartBobberDetection()
    {
        try
        {
            // Implementar detecção contínua do bobber
            _ = Task.Run(async () =>
            {
                while (IsBotActive)
                {
                    try
                    {
                        if (ScreenCapture != null)
                        {
                            var result = _bobberDetector.DetectInArea(
                                new Rectangle(0, 0, (int)ScreenCapture.Width, (int)ScreenCapture.Height), 
                                0.7);
                            
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                BobberDetected = result.Detected;
                                BobberStatus = result.Detected ? "Detected" : "Not detected";
                                
                                if (result.Detected)
                                {
                                    BobberDetectionX = result.PositionX - 10;
                                    BobberDetectionY = result.PositionY - 10;
                                    FishingLineVisible = true;
                                    FishingLineStartX = result.PositionX;
                                    FishingLineStartY = result.PositionY;
                                    FishingLineEndX = result.PositionX;
                                    FishingLineEndY = result.PositionY + 50;
                                }
                                else
                                {
                                    FishingLineVisible = false;
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro na detecção do bobber");
                    }
                    
                    await Task.Delay(200); // 5 FPS
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar detecção do bobber");
        }
    }
    
    private async Task StopBobberDetection()
    {
        // Parar detecção do bobber
        await Task.CompletedTask;
    }
    
    public void CaptureScreen()
    {
        try
        {
            var capture = _screenCaptureService.CaptureScreen();
            if (capture != null)
            {
                ScreenCapture = ConvertToBitmapSource(capture);
                AddActivityLog("Captura de tela realizada", "Info");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao capturar tela");
            AddActivityLog($"Erro na captura: {ex.Message}", "Error");
        }
    }
    
    public void TestBobberDetection()
    {
        try
        {
            if (ScreenCapture != null)
            {
                var result = _bobberDetector.DetectInArea(
                    new Rectangle(0, 0, (int)ScreenCapture.Width, (int)ScreenCapture.Height), 
                    0.7);
                
                BobberDetected = result.Detected;
                BobberStatus = result.Detected ? $"Detected (Score: {result.Score:F2})" : "Not detected";
                
                AddActivityLog($"Teste de detecção: {BobberStatus}", result.Detected ? "Success" : "Info");
            }
            else
            {
                AddActivityLog("Nenhuma captura de tela disponível", "Warning");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no teste de detecção");
            AddActivityLog($"Erro no teste: {ex.Message}", "Error");
        }
    }
    
    public void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel * 1.2, 5.0);
        AddActivityLog($"Zoom aumentado: {ZoomLevel:F1}x", "Info");
    }
    
    public void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel / 1.2, 0.5);
        AddActivityLog($"Zoom diminuído: {ZoomLevel:F1}x", "Info");
    }
    
    public void ResetZoom()
    {
        ZoomLevel = 1.0;
        AddActivityLog("Zoom resetado", "Info");
    }
    
    public void ClearActivityLog()
    {
        ActivityLog.Clear();
    }
    
    private void AddActivityLog(string message, string type)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ActivityLog.Add(new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Type = type
            });
            
            // Manter apenas os últimos 100 registros
            if (ActivityLog.Count > 100)
            {
                ActivityLog.RemoveAt(0);
            }
        });
    }
    
    private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
    {
        try
        {
            var hBitmap = bitmap.GetHbitmap();
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            
            // Limpar recursos
            DeleteObject(hBitmap);
            
            return bitmapSource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter bitmap");
            return null;
        }
    }
    
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
    
    #endregion
    
    #region Event Handlers
    
    private void OnDecisionMade(object sender, DecisionMadeEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AddActivityLog($"Decisão tomada: {e.Decision.Action}", "Info");
        });
    }
    
    private void OnContextChanged(object sender, ContextChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Atualizar informações do jogador
            PlayerHealthPercentage = e.NewContext.PlayerHealth;
            PlayerEnergyPercentage = e.NewContext.PlayerEnergy;
            PlayerSilver = e.NewContext.TryGetValue<int>("PlayerSilver", out var silver) ? silver : 0;
            PlayerPosition = $"{e.NewContext.PlayerPosition.X:F1}, {e.NewContext.PlayerPosition.Y:F1}";
            PlayerPositionX = e.NewContext.PlayerPosition.X;
            PlayerPositionY = e.NewContext.PlayerPosition.Y;
        });
    }
    
    private void OnBehaviorExecuted(object sender, BehaviorExecutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AddActivityLog($"Comportamento executado: {e.BehaviorName}", "Info");
        });
    }
    
    #endregion
}

#region Classes de Dados

public class MapPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class ActivityLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

#endregion
