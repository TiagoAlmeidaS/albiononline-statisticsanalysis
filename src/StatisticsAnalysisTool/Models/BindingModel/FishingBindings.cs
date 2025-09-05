using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

/// <summary>
/// Bindings para o painel de pesca
/// </summary>
public class FishingBindings : INotifyPropertyChanged
{
    private GridLength _gridSplitterPosition = new(1, GridUnitType.Star);
    private bool _isFishingActive = false;
    private bool _isAutoFishingEnabled = true;
    private bool _isMinigameResolutionEnabled = true;
    private bool _isDecisionEngineActive = false;
    private string _currentFishingState = "Idle";
    private string _lastDecision = "Nenhuma";
    private int _fishingAttempts = 0;
    private int _successfulCatches = 0;
    private int _failedCatches = 0;
    private double _successRate = 0;
    private TimeSpan _totalFishingTime = TimeSpan.Zero;
    private TimeSpan _averageCatchTime = TimeSpan.Zero;
    private string _currentFishingZone = "Nenhuma";
    private int _fishInZone = 0;
    private long _estimatedZoneValue = 0;
    private float _playerHealthPercentage = 100;
    private float _playerEnergyPercentage = 100;
    private bool _isPlayerInCombat = false;
    private bool _isPlayerInSafeZone = true;
    private string _lastError = string.Empty;
    private bool _hasError = false;
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
    private int _totalDecisions = 0;
    private int _successfulDecisions = 0;
    private double _decisionSuccessRate = 0;
    private TimeSpan _averageDecisionTime = TimeSpan.Zero;
    private int _activeBehaviors = 0;
    private int _totalBehaviors = 0;
    
    // Coleções
    private ObservableCollection<FishingZoneInfo> _availableFishingZones = new();
    private ObservableCollection<FishingCatchInfo> _recentCatches = new();
    private ObservableCollection<DecisionLogEntry> _decisionLog = new();
    private ObservableCollection<BehaviorStatusInfo> _behaviorStatuses = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    #region Propriedades Públicas
    
    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            OnPropertyChanged(nameof(GridSplitterPosition));
        }
    }
    
    public bool IsFishingActive
    {
        get => _isFishingActive;
        set
        {
            _isFishingActive = value;
            OnPropertyChanged(nameof(IsFishingActive));
        }
    }
    
    public bool IsAutoFishingEnabled
    {
        get => _isAutoFishingEnabled;
        set
        {
            _isAutoFishingEnabled = value;
            OnPropertyChanged(nameof(IsAutoFishingEnabled));
        }
    }
    
    public bool IsMinigameResolutionEnabled
    {
        get => _isMinigameResolutionEnabled;
        set
        {
            _isMinigameResolutionEnabled = value;
            OnPropertyChanged(nameof(IsMinigameResolutionEnabled));
        }
    }
    
    public bool IsDecisionEngineActive
    {
        get => _isDecisionEngineActive;
        set
        {
            _isDecisionEngineActive = value;
            OnPropertyChanged(nameof(IsDecisionEngineActive));
        }
    }
    
    public string CurrentFishingState
    {
        get => _currentFishingState;
        set
        {
            _currentFishingState = value;
            OnPropertyChanged(nameof(CurrentFishingState));
        }
    }
    
    public string LastDecision
    {
        get => _lastDecision;
        set
        {
            _lastDecision = value;
            OnPropertyChanged(nameof(LastDecision));
        }
    }
    
    public int FishingAttempts
    {
        get => _fishingAttempts;
        set
        {
            _fishingAttempts = value;
            OnPropertyChanged(nameof(FishingAttempts));
        }
    }
    
    public int SuccessfulCatches
    {
        get => _successfulCatches;
        set
        {
            _successfulCatches = value;
            OnPropertyChanged(nameof(SuccessfulCatches));
        }
    }
    
    public int FailedCatches
    {
        get => _failedCatches;
        set
        {
            _failedCatches = value;
            OnPropertyChanged(nameof(FailedCatches));
        }
    }
    
    public double SuccessRate
    {
        get => _successRate;
        set
        {
            _successRate = value;
            OnPropertyChanged(nameof(SuccessRate));
        }
    }
    
    public TimeSpan TotalFishingTime
    {
        get => _totalFishingTime;
        set
        {
            _totalFishingTime = value;
            OnPropertyChanged(nameof(TotalFishingTime));
        }
    }
    
    public TimeSpan AverageCatchTime
    {
        get => _averageCatchTime;
        set
        {
            _averageCatchTime = value;
            OnPropertyChanged(nameof(AverageCatchTime));
        }
    }
    
    public string CurrentFishingZone
    {
        get => _currentFishingZone;
        set
        {
            _currentFishingZone = value;
            OnPropertyChanged(nameof(CurrentFishingZone));
        }
    }
    
    public int FishInZone
    {
        get => _fishInZone;
        set
        {
            _fishInZone = value;
            OnPropertyChanged(nameof(FishInZone));
        }
    }
    
    public long EstimatedZoneValue
    {
        get => _estimatedZoneValue;
        set
        {
            _estimatedZoneValue = value;
            OnPropertyChanged(nameof(EstimatedZoneValue));
        }
    }
    
    public float PlayerHealthPercentage
    {
        get => _playerHealthPercentage;
        set
        {
            _playerHealthPercentage = value;
            OnPropertyChanged(nameof(PlayerHealthPercentage));
        }
    }
    
    public float PlayerEnergyPercentage
    {
        get => _playerEnergyPercentage;
        set
        {
            _playerEnergyPercentage = value;
            OnPropertyChanged(nameof(PlayerEnergyPercentage));
        }
    }
    
    public bool IsPlayerInCombat
    {
        get => _isPlayerInCombat;
        set
        {
            _isPlayerInCombat = value;
            OnPropertyChanged(nameof(IsPlayerInCombat));
        }
    }
    
    public bool IsPlayerInSafeZone
    {
        get => _isPlayerInSafeZone;
        set
        {
            _isPlayerInSafeZone = value;
            OnPropertyChanged(nameof(IsPlayerInSafeZone));
        }
    }
    
    public string LastError
    {
        get => _lastError;
        set
        {
            _lastError = value;
            OnPropertyChanged(nameof(LastError));
        }
    }
    
    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            OnPropertyChanged(nameof(HasError));
        }
    }
    
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged(nameof(StatusText));
        }
    }
    
    #endregion
    
    #region Configurações
    
    public int MinHealthPercentage
    {
        get => _minHealthPercentage;
        set
        {
            _minHealthPercentage = value;
            OnPropertyChanged(nameof(MinHealthPercentage));
        }
    }
    
    public int MinEnergyPercentage
    {
        get => _minEnergyPercentage;
        set
        {
            _minEnergyPercentage = value;
            OnPropertyChanged(nameof(MinEnergyPercentage));
        }
    }
    
    public int MaxAttempts
    {
        get => _maxAttempts;
        set
        {
            _maxAttempts = value;
            OnPropertyChanged(nameof(MaxAttempts));
        }
    }
    
    public int CooldownSeconds
    {
        get => _cooldownSeconds;
        set
        {
            _cooldownSeconds = value;
            OnPropertyChanged(nameof(CooldownSeconds));
        }
    }
    
    public int MaxConsecutiveFailures
    {
        get => _maxConsecutiveFailures;
        set
        {
            _maxConsecutiveFailures = value;
            OnPropertyChanged(nameof(MaxConsecutiveFailures));
        }
    }
    
    public bool EnableAIDecisions
    {
        get => _enableAIDecisions;
        set
        {
            _enableAIDecisions = value;
            OnPropertyChanged(nameof(EnableAIDecisions));
        }
    }
    
    public double AIConfidenceThreshold
    {
        get => _aiConfidenceThreshold;
        set
        {
            _aiConfidenceThreshold = value;
            OnPropertyChanged(nameof(AIConfidenceThreshold));
        }
    }
    
    #endregion
    
    #region Estatísticas do Motor de Decisão
    
    public int TotalDecisions
    {
        get => _totalDecisions;
        set
        {
            _totalDecisions = value;
            OnPropertyChanged(nameof(TotalDecisions));
        }
    }
    
    public int SuccessfulDecisions
    {
        get => _successfulDecisions;
        set
        {
            _successfulDecisions = value;
            OnPropertyChanged(nameof(SuccessfulDecisions));
        }
    }
    
    public double DecisionSuccessRate
    {
        get => _decisionSuccessRate;
        set
        {
            _decisionSuccessRate = value;
            OnPropertyChanged(nameof(DecisionSuccessRate));
        }
    }
    
    public TimeSpan AverageDecisionTime
    {
        get => _averageDecisionTime;
        set
        {
            _averageDecisionTime = value;
            OnPropertyChanged(nameof(AverageDecisionTime));
        }
    }
    
    public int ActiveBehaviors
    {
        get => _activeBehaviors;
        set
        {
            _activeBehaviors = value;
            OnPropertyChanged(nameof(ActiveBehaviors));
        }
    }
    
    public int TotalBehaviors
    {
        get => _totalBehaviors;
        set
        {
            _totalBehaviors = value;
            OnPropertyChanged(nameof(TotalBehaviors));
        }
    }
    
    #endregion
    
    #region Coleções
    
    public ObservableCollection<FishingZoneInfo> AvailableFishingZones
    {
        get => _availableFishingZones;
        set
        {
            _availableFishingZones = value;
            OnPropertyChanged(nameof(AvailableFishingZones));
        }
    }
    
    public ObservableCollection<FishingCatchInfo> RecentCatches
    {
        get => _recentCatches;
        set
        {
            _recentCatches = value;
            OnPropertyChanged(nameof(RecentCatches));
        }
    }
    
    public ObservableCollection<DecisionLogEntry> DecisionLog
    {
        get => _decisionLog;
        set
        {
            _decisionLog = value;
            OnPropertyChanged(nameof(DecisionLog));
        }
    }
    
    public ObservableCollection<BehaviorStatusInfo> BehaviorStatuses
    {
        get => _behaviorStatuses;
        set
        {
            _behaviorStatuses = value;
            OnPropertyChanged(nameof(BehaviorStatuses));
        }
    }
    
    #endregion
}

#region Classes de Dados

/// <summary>
/// Informações sobre zona de pesca
/// </summary>
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

/// <summary>
/// Informações sobre captura de peixe
/// </summary>
public class FishingCatchInfo
{
    public DateTime Timestamp { get; set; }
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
}

/// <summary>
/// Entrada do log de decisões
/// </summary>
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

/// <summary>
/// Informações sobre status do comportamento
/// </summary>
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
