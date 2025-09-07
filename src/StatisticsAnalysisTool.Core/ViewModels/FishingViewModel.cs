using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StatisticsAnalysisTool.Core.Interfaces;
using StatisticsAnalysisTool.Core.Common;

namespace StatisticsAnalysisTool.Core.ViewModels;

public class FishingViewModel : INotifyPropertyChanged
{
    private readonly IPlatformServices _platformServices;
    private bool _isRunning;
    private string _fishingStatus = "Stopped";
    private int _fishCount;
    private int _attemptCount;
    private double _successRate;
    private string _connectionStatus = "Disconnected";
    private string _lastUpdate = "Never";
    private ObservableCollection<string> _fishingLog = new();

    public FishingViewModel(IPlatformServices platformServices)
    {
        _platformServices = platformServices;
        StartFishingCommand = new RelayCommand(StartFishing, () => !IsRunning);
        StopFishingCommand = new RelayCommand(StopFishing, () => IsRunning);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
                ((RelayCommand)StartFishingCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StopFishingCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string FishingStatus
    {
        get => _fishingStatus;
        set
        {
            if (_fishingStatus != value)
            {
                _fishingStatus = value;
                OnPropertyChanged();
            }
        }
    }

    public int FishCount
    {
        get => _fishCount;
        set
        {
            if (_fishCount != value)
            {
                _fishCount = value;
                OnPropertyChanged();
                UpdateSuccessRate();
            }
        }
    }

    public int AttemptCount
    {
        get => _attemptCount;
        set
        {
            if (_attemptCount != value)
            {
                _attemptCount = value;
                OnPropertyChanged();
                UpdateSuccessRate();
            }
        }
    }

    public double SuccessRate
    {
        get => _successRate;
        set
        {
            if (_successRate != value)
            {
                _successRate = value;
                OnPropertyChanged();
            }
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            if (_connectionStatus != value)
            {
                _connectionStatus = value;
                OnPropertyChanged();
            }
        }
    }

    public string LastUpdate
    {
        get => _lastUpdate;
        set
        {
            if (_lastUpdate != value)
            {
                _lastUpdate = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<string> FishingLog
    {
        get => _fishingLog;
        set
        {
            if (_fishingLog != value)
            {
                _fishingLog = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand StartFishingCommand { get; }
    public ICommand StopFishingCommand { get; }

    private void StartFishing()
    {
        IsRunning = true;
        FishingStatus = "Running";
        ConnectionStatus = "Connected";
        AddLogEntry("Fishing started");
    }

    private void StopFishing()
    {
        IsRunning = false;
        FishingStatus = "Stopped";
        ConnectionStatus = "Disconnected";
        AddLogEntry("Fishing stopped");
    }

    private void UpdateSuccessRate()
    {
        SuccessRate = AttemptCount > 0 ? (double)FishCount / AttemptCount * 100 : 0;
    }

    private void AddLogEntry(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        FishingLog.Insert(0, $"[{timestamp}] {message}");
        LastUpdate = DateTime.Now.ToString("HH:mm:ss");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
