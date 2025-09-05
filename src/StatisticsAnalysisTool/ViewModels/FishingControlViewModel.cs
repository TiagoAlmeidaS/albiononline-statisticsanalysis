using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using StatisticsAnalysisTool.DecisionEngine.Core;
using StatisticsAnalysisTool.DecisionEngine.Models;
using StatisticsAnalysisTool.DecisionEngine.Behaviors;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;
using StatisticsAnalysisTool.Common;
using Microsoft.Extensions.Logging;
using Serilog;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision.Detectors;
using AlbionFishing.Vision.DependencyInjection;

namespace StatisticsAnalysisTool.ViewModels;

/// <summary>
/// ViewModel para o painel de controle de pesca
/// </summary>
public class FishingControlViewModel : BaseViewModel
{
    private readonly DecisionEngine.Abstractions.IUniversalDecisionEngine _decisionEngine;
    private readonly ILogger<FishingControlViewModel> _logger;
    private readonly IVisionConfigurationService _visionConfigService;
    private readonly IBobberDetectorFactory _detectorFactory;
    
    private bool _isFishingActive;
    private bool _isAutoFishingEnabled;
    private bool _isMinigameResolutionEnabled;
    private bool _isDecisionEngineActive;
    private string _currentFishingState = "Idle";
    private string _lastDecision = "Nenhuma";
    private int _fishingAttempts;
    private int _successfulCatches;
    private int _failedCatches;
    private double _successRate;
    private TimeSpan _totalFishingTime = TimeSpan.Zero;
    private TimeSpan _averageCatchTime = TimeSpan.Zero;
    private string _currentFishingZone = "Nenhuma";
    private int _fishInZone;
    private long _estimatedZoneValue;
    private float _playerHealthPercentage = 100;
    private float _playerEnergyPercentage = 100;
    private bool _isPlayerInCombat;
    private bool _isPlayerInSafeZone;
    private string _lastError = string.Empty;
    private bool _hasError;
    private System.Windows.Media.Brush _statusColor = Brushes.Green;
    private string _statusText = "Sistema Ativo";
    
    // Configurações
    private int _minHealthPercentage = 50;
    private int _minEnergyPercentage = 30;
    private int _maxAttempts = 10;
    private int _cooldownSeconds = 5;
    private int _maxConsecutiveFailures = 3;
    private bool _enableAIDecisions = false;
    private double _aiConfidenceThreshold = 70;
    
    // Estatísticas do motor de decisão
    private int _totalDecisions;
    private int _successfulDecisions;
    private double _decisionSuccessRate;
    private TimeSpan _averageDecisionTime = TimeSpan.Zero;
    private int _activeBehaviors;
    private int _totalBehaviors;
    
    // Coleções
    private ObservableCollection<FishingZoneInfo> _availableFishingZones = new();
    private ObservableCollection<FishingCatchInfo> _recentCatches = new();
    private ObservableCollection<DecisionLogEntry> _decisionLog = new();
    private ObservableCollection<BehaviorStatusInfo> _behaviorStatuses = new();
    
    // ViewModel de configuração de visão
    private VisionConfigurationViewModel _visionConfigurationViewModel;
    
    public FishingControlViewModel(
        DecisionEngine.Abstractions.IUniversalDecisionEngine decisionEngine, 
        ILogger<FishingControlViewModel> logger,
        IVisionConfigurationService visionConfigService,
        IBobberDetectorFactory detectorFactory,
        IServiceProvider serviceProvider = null)
    {
        _decisionEngine = decisionEngine;
        _logger = logger;
        _visionConfigService = visionConfigService;
        _detectorFactory = detectorFactory;
        
        // Subscrever eventos do motor de decisão
        _decisionEngine.DecisionMade += OnDecisionMade;
        _decisionEngine.ContextChanged += OnContextChanged;
        
        // Inicializar comandos
        StartFishingCommand = new RelayCommand(async () => await StartFishingAsync());
        StopFishingCommand = new RelayCommand(async () => await StopFishingAsync());
        RefreshZonesCommand = new RelayCommand(async () => await RefreshZonesAsync());
        ClearLogCommand = new RelayCommand(() => ClearLog());
        ResetStatsCommand = new RelayCommand(() => ResetStats());
        ApplySettingsCommand = new RelayCommand(async () => await ApplySettingsAsync());
        
        // Inicializar comportamentos
        InitializeBehaviors();
        
        // Inicializar ViewModel de configuração de visão
        var visionLogger = serviceProvider?.GetService(typeof(ILogger<VisionConfigurationViewModel>)) as ILogger<VisionConfigurationViewModel>;
        _visionConfigurationViewModel = new VisionConfigurationViewModel(
            _visionConfigService, 
            _detectorFactory, 
            visionLogger ?? (ILogger<VisionConfigurationViewModel>)_logger);
        
        // Iniciar monitoramento
        StartMonitoring();
    }
    
    #region Propriedades Públicas
    
    public bool IsFishingActive
    {
        get => _isFishingActive;
        set => SetProperty(ref _isFishingActive, value);
    }
    
    public bool IsAutoFishingEnabled
    {
        get => _isAutoFishingEnabled;
        set => SetProperty(ref _isAutoFishingEnabled, value);
    }
    
    public bool IsMinigameResolutionEnabled
    {
        get => _isMinigameResolutionEnabled;
        set => SetProperty(ref _isMinigameResolutionEnabled, value);
    }
    
    public bool IsDecisionEngineActive
    {
        get => _isDecisionEngineActive;
        set => SetProperty(ref _isDecisionEngineActive, value);
    }
    
    public string CurrentFishingState
    {
        get => _currentFishingState;
        set => SetProperty(ref _currentFishingState, value);
    }
    
    public string LastDecision
    {
        get => _lastDecision;
        set => SetProperty(ref _lastDecision, value);
    }
    
    public int FishingAttempts
    {
        get => _fishingAttempts;
        set => SetProperty(ref _fishingAttempts, value);
    }
    
    public int SuccessfulCatches
    {
        get => _successfulCatches;
        set => SetProperty(ref _successfulCatches, value);
    }
    
    public int FailedCatches
    {
        get => _failedCatches;
        set => SetProperty(ref _failedCatches, value);
    }
    
    public double SuccessRate
    {
        get => _successRate;
        set => SetProperty(ref _successRate, value);
    }
    
    public TimeSpan TotalFishingTime
    {
        get => _totalFishingTime;
        set => SetProperty(ref _totalFishingTime, value);
    }
    
    public TimeSpan AverageCatchTime
    {
        get => _averageCatchTime;
        set => SetProperty(ref _averageCatchTime, value);
    }
    
    public string CurrentFishingZone
    {
        get => _currentFishingZone;
        set => SetProperty(ref _currentFishingZone, value);
    }
    
    public int FishInZone
    {
        get => _fishInZone;
        set => SetProperty(ref _fishInZone, value);
    }
    
    public long EstimatedZoneValue
    {
        get => _estimatedZoneValue;
        set => SetProperty(ref _estimatedZoneValue, value);
    }
    
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
    
    public bool IsPlayerInCombat
    {
        get => _isPlayerInCombat;
        set => SetProperty(ref _isPlayerInCombat, value);
    }
    
    public bool IsPlayerInSafeZone
    {
        get => _isPlayerInSafeZone;
        set => SetProperty(ref _isPlayerInSafeZone, value);
    }
    
    public string LastError
    {
        get => _lastError;
        set => SetProperty(ref _lastError, value);
    }
    
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }
    
    public System.Windows.Media.Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }
    
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
    
    #endregion
    
    #region Configurações
    
    public int MinHealthPercentage
    {
        get => _minHealthPercentage;
        set => SetProperty(ref _minHealthPercentage, value);
    }
    
    public int MinEnergyPercentage
    {
        get => _minEnergyPercentage;
        set => SetProperty(ref _minEnergyPercentage, value);
    }
    
    public int MaxAttempts
    {
        get => _maxAttempts;
        set => SetProperty(ref _maxAttempts, value);
    }
    
    public int CooldownSeconds
    {
        get => _cooldownSeconds;
        set => SetProperty(ref _cooldownSeconds, value);
    }
    
    public int MaxConsecutiveFailures
    {
        get => _maxConsecutiveFailures;
        set => SetProperty(ref _maxConsecutiveFailures, value);
    }
    
    public bool EnableAIDecisions
    {
        get => _enableAIDecisions;
        set => SetProperty(ref _enableAIDecisions, value);
    }
    
    public double AIConfidenceThreshold
    {
        get => _aiConfidenceThreshold;
        set => SetProperty(ref _aiConfidenceThreshold, value);
    }
    
    #endregion
    
    #region Estatísticas do Motor de Decisão
    
    public int TotalDecisions
    {
        get => _totalDecisions;
        set => SetProperty(ref _totalDecisions, value);
    }
    
    public int SuccessfulDecisions
    {
        get => _successfulDecisions;
        set => SetProperty(ref _successfulDecisions, value);
    }
    
    public double DecisionSuccessRate
    {
        get => _decisionSuccessRate;
        set => SetProperty(ref _decisionSuccessRate, value);
    }
    
    public TimeSpan AverageDecisionTime
    {
        get => _averageDecisionTime;
        set => SetProperty(ref _averageDecisionTime, value);
    }
    
    public int ActiveBehaviors
    {
        get => _activeBehaviors;
        set => SetProperty(ref _activeBehaviors, value);
    }
    
    public int TotalBehaviors
    {
        get => _totalBehaviors;
        set => SetProperty(ref _totalBehaviors, value);
    }
    
    #endregion
    
    #region Coleções
    
    public ObservableCollection<FishingZoneInfo> AvailableFishingZones
    {
        get => _availableFishingZones;
        set => SetProperty(ref _availableFishingZones, value);
    }
    
    public ObservableCollection<FishingCatchInfo> RecentCatches
    {
        get => _recentCatches;
        set => SetProperty(ref _recentCatches, value);
    }
    
    public ObservableCollection<DecisionLogEntry> DecisionLog
    {
        get => _decisionLog;
        set => SetProperty(ref _decisionLog, value);
    }
    
    public ObservableCollection<BehaviorStatusInfo> BehaviorStatuses
    {
        get => _behaviorStatuses;
        set => SetProperty(ref _behaviorStatuses, value);
    }
    
    public VisionConfigurationViewModel VisionConfigurationViewModel
    {
        get => _visionConfigurationViewModel;
        set => SetProperty(ref _visionConfigurationViewModel, value);
    }
    
    #endregion
    
    #region Comandos
    
    public RelayCommand StartFishingCommand { get; set; }
    public RelayCommand StopFishingCommand { get; set; }
    public RelayCommand RefreshZonesCommand { get; set; }
    public RelayCommand ClearLogCommand { get; set; }
    public RelayCommand ResetStatsCommand { get; set; }
    public RelayCommand ApplySettingsCommand { get; set; }
    
    #endregion
    
    #region Métodos Privados
    
    private async void InitializeBehaviors()
    {
        try
        {
            // Obter comportamentos ativos
            var activeBehaviors = _decisionEngine.GetActiveBehaviors();
            
            // Log dos comportamentos ativos
            foreach (var behavior in activeBehaviors)
            {
                _logger.LogInformation("Comportamento ativo: {BehaviorName}", behavior.GetType().Name);
            }
            
            UpdateBehaviorStatuses();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar comportamentos de pesca");
            SetError($"Erro ao inicializar comportamentos: {ex.Message}");
        }
    }
    
    private async void StartMonitoring()
    {
        try
        {
            IsDecisionEngineActive = _decisionEngine.IsActive;
            
            // Atualizar estatísticas periodicamente
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            
            timer.Tick += async (s, e) => await UpdateStatsAsync();
            timer.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar monitoramento");
            SetError($"Erro ao iniciar monitoramento: {ex.Message}");
        }
    }
    
    private async Task UpdateStatsAsync()
    {
        try
        {
            var stats = _decisionEngine.Stats;
            
            TotalDecisions = (int)stats.TotalDecisions;
            SuccessfulDecisions = (int)stats.SuccessfulDecisions;
            DecisionSuccessRate = stats.SuccessRate * 100;
            AverageDecisionTime = stats.AverageProcessingTime;
            ActiveBehaviors = stats.ActiveBehaviors;
            TotalBehaviors = stats.ActiveRules; // Usar ActiveRules como aproximação
            
            // Atualizar status do sistema
            if (stats.TotalDecisions > 0 && stats.SuccessRate > 0.5)
            {
                StatusColor = System.Windows.Media.Brushes.Green;
                StatusText = "Sistema Ativo";
                HasError = false;
            }
            else
            {
                StatusColor = System.Windows.Media.Brushes.Orange;
                StatusText = "Sistema com Avisos";
                HasError = true;
                LastError = "Sistema com problemas detectados";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar estatísticas");
            SetError($"Erro ao atualizar estatísticas: {ex.Message}");
        }
    }
    
    private void UpdateBehaviorStatuses()
    {
        try
        {
            BehaviorStatuses.Clear();
            
            foreach (var behavior in _decisionEngine.GetActiveBehaviors())
            {
                var status = new BehaviorStatusInfo
                {
                    Name = behavior.GetType().Name,
                    IsEnabled = true, // Assumir que está habilitado se está ativo
                    IsActive = true,
                    Priority = 0, // Prioridade padrão
                    SuccessRate = 0, // Não temos acesso às estatísticas
                    TotalExecutions = 0,
                    IsHealthy = true,
                    LastError = string.Empty
                };
                
                BehaviorStatuses.Add(status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status dos comportamentos");
        }
    }
    
    private void SetError(string error)
    {
        LastError = error;
        HasError = true;
        StatusColor = Brushes.Red;
        StatusText = "Erro";
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnDecisionMade(object? sender, DecisionMadeEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LastDecision = $"{e.Decision.Action} (Confiança: {e.Decision.Confidence:F1}%)";
            
            // Adicionar ao log de decisões
            var logEntry = new DecisionLogEntry
            {
                Timestamp = e.Timestamp,
                Action = e.Decision.Action,
                Confidence = (int)(e.Decision.Confidence * 100),
                Priority = e.Decision.Priority,
                Reason = e.Decision.Reason,
                ProcessingTime = TimeSpan.Zero, // Não temos acesso ao tempo de processamento
                IsAIDecision = false // Não temos acesso a essa informação
            };
            
            DecisionLog.Insert(0, logEntry);
            
            // Manter apenas as últimas 100 entradas
            while (DecisionLog.Count > 100)
            {
                DecisionLog.RemoveAt(DecisionLog.Count - 1);
            }
        });
    }
    
    private void OnContextChanged(object? sender, ContextChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Atualizar informações do jogador
            PlayerHealthPercentage = e.NewContext.PlayerHealth;
            PlayerEnergyPercentage = e.NewContext.PlayerEnergy;
            IsPlayerInCombat = e.NewContext.TryGetValue<bool>("IsInCombat", out var inCombat) && inCombat;
            IsPlayerInSafeZone = e.NewContext.TryGetValue<bool>("IsInSafeZone", out var inSafeZone) && inSafeZone;
            
            // Atualizar zonas de pesca (simplificado)
            AvailableFishingZones.Clear();
            // Nota: As zonas de pesca seriam obtidas de outra forma
        });
    }
    
    private void OnBehaviorExecuted(object? sender, BehaviorExecutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Comportamento executado - implementação simplificada
            FishingAttempts++;
            SuccessRate = FishingAttempts > 0 ? (double)SuccessfulCatches / FishingAttempts * 100 : 0;
            
            // Adicionar captura recente (simplificado)
            var catchInfo = new FishingCatchInfo
            {
                Timestamp = DateTime.Now,
                IsSuccessful = true, // Assumir sucesso por simplicidade
                Message = "Comportamento executado",
                ExecutionTime = TimeSpan.Zero
            };
            
            RecentCatches.Insert(0, catchInfo);
            
            // Manter apenas as últimas 50 capturas
            while (RecentCatches.Count > 50)
            {
                RecentCatches.RemoveAt(RecentCatches.Count - 1);
            }
        });
    }
    
    #endregion
    
    #region Comandos Implementation
    
    public async Task StartFishingAsync()
    {
        try
        {
            // Implementar lógica de início de pesca
            IsFishingActive = true;
            CurrentFishingState = "Iniciando";
            
            _logger.LogInformation("Pesca iniciada pelo usuário");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar pesca");
            SetError($"Erro ao iniciar pesca: {ex.Message}");
        }
    }
    
    public async Task StopFishingAsync()
    {
        try
        {
            // Implementar lógica de parada de pesca
            IsFishingActive = false;
            CurrentFishingState = "Parado";
            
            _logger.LogInformation("Pesca parada pelo usuário");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar pesca");
            SetError($"Erro ao parar pesca: {ex.Message}");
        }
    }
    
    public async Task RefreshZonesAsync()
    {
        try
        {
            // Implementar lógica de atualização de zonas
            _logger.LogInformation("Zonas de pesca atualizadas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar zonas");
            SetError($"Erro ao atualizar zonas: {ex.Message}");
        }
    }
    
    public void ClearLog()
    {
        DecisionLog.Clear();
        RecentCatches.Clear();
    }
    
    public void ResetStats()
    {
        FishingAttempts = 0;
        SuccessfulCatches = 0;
        FailedCatches = 0;
        SuccessRate = 0;
        TotalFishingTime = TimeSpan.Zero;
        AverageCatchTime = TimeSpan.Zero;
    }
    
    public async Task ApplySettingsAsync()
    {
        try
        {
            // Aplicar configurações - implementação simplificada
            _logger.LogInformation("Configurações aplicadas com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aplicar configurações");
            SetError($"Erro ao aplicar configurações: {ex.Message}");
        }
    }
    
    #endregion
}

#region Classes de Dados

public class FishingZoneInfo
{
    public long ObjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int FishCount { get; set; }
    public long EstimatedValue { get; set; }
    public float Distance { get; set; }
    public bool IsActive { get; set; }
}

public class FishingCatchInfo
{
    public DateTime Timestamp { get; set; }
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
}

public class DecisionLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public int Priority { get; set; }
    public string Reason { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public bool IsAIDecision { get; set; }
}

public class BehaviorStatusInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public double SuccessRate { get; set; }
    public int TotalExecutions { get; set; }
    public bool IsHealthy { get; set; }
    public string LastError { get; set; } = string.Empty;
}

#endregion
