using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Common;
using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.DecisionEngine.Core;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;
using Microsoft.Extensions.Logging;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision.Detectors;
using AlbionFishing.Vision.DependencyInjection;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Controle de interface para o sistema de pesca com motor de decisão
/// </summary>
public partial class FishingControl : UserControl
{
    private FishingControlViewModel? _viewModel;
    
    public FishingControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Obter serviços do DI container
            var serviceProvider = Application.Current.Resources["ServiceProvider"] as IServiceProvider;
            if (serviceProvider == null)
            {
                MessageBox.Show("Erro: ServiceProvider não encontrado. O sistema de pesca não pode ser inicializado.", 
                    "Erro de Inicialização", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Obter motor de decisão
            var decisionEngine = serviceProvider.GetService<DecisionEngine.Abstractions.IUniversalDecisionEngine>();
            if (decisionEngine == null)
            {
                MessageBox.Show("Erro: Motor de Decisão não encontrado. Verifique se o sistema foi configurado corretamente.", 
                    "Erro de Configuração", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Obter logger
            var logger = serviceProvider.GetService<ILogger<FishingControlViewModel>>();
            if (logger == null)
            {
                MessageBox.Show("Erro: Logger não encontrado. O sistema de pesca não pode ser inicializado.", 
                    "Erro de Inicialização", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Obter serviços adicionais
            var visionConfigService = serviceProvider.GetService<IVisionConfigurationService>();
            var detectorFactory = serviceProvider.GetService<IBobberDetectorFactory>();
            
            if (visionConfigService == null || detectorFactory == null)
            {
                MessageBox.Show("Erro: Serviços de visão não encontrados. O sistema de pesca não pode ser inicializado.", 
                    "Erro de Configuração", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Criar ViewModel
            _viewModel = new FishingControlViewModel(decisionEngine, logger, visionConfigService, detectorFactory, serviceProvider);
            DataContext = _viewModel;
            
            // Inicializar comandos
            InitializeCommands();
            
            // Inicializar motor de decisão se não estiver ativo
            if (!decisionEngine.IsActive)
            {
                await decisionEngine.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao inicializar o sistema de pesca: {ex.Message}", 
                "Erro de Inicialização", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Limpar recursos se necessário
            _viewModel = null;
            DataContext = null;
        }
        catch (Exception ex)
        {
            // Log do erro se possível
            System.Diagnostics.Debug.WriteLine($"Erro ao descarregar FishingControl: {ex.Message}");
        }
    }
    
    private void InitializeCommands()
    {
        if (_viewModel == null) return;
        
        // Configurar comandos
        _viewModel.StartFishingCommand = new RelayCommand(async () => await _viewModel.StartFishingAsync());
        _viewModel.StopFishingCommand = new RelayCommand(async () => await _viewModel.StopFishingAsync());
        _viewModel.RefreshZonesCommand = new RelayCommand(async () => await _viewModel.RefreshZonesAsync());
        _viewModel.ClearLogCommand = new RelayCommand(() => _viewModel.ClearLog());
        _viewModel.ResetStatsCommand = new RelayCommand(() => _viewModel.ResetStats());
        _viewModel.ApplySettingsCommand = new RelayCommand(async () => await _viewModel.ApplySettingsAsync());
    }
    
    /// <summary>
    /// Método público para iniciar o sistema de pesca
    /// </summary>
    public async Task StartFishingSystemAsync()
    {
        try
        {
            if (_viewModel != null)
            {
                await _viewModel.StartFishingAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao iniciar sistema de pesca: {ex.Message}", 
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Método público para parar o sistema de pesca
    /// </summary>
    public async Task StopFishingSystemAsync()
    {
        try
        {
            if (_viewModel != null)
            {
                await _viewModel.StopFishingAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao parar sistema de pesca: {ex.Message}", 
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Método público para obter estatísticas do sistema
    /// </summary>
    public FishingSystemStats? GetSystemStats()
    {
        try
        {
            if (_viewModel == null) return null;
            
            return new FishingSystemStats
            {
                IsActive = _viewModel.IsFishingActive,
                IsDecisionEngineActive = _viewModel.IsDecisionEngineActive,
                CurrentState = _viewModel.CurrentFishingState,
                FishingAttempts = _viewModel.FishingAttempts,
                SuccessfulCatches = _viewModel.SuccessfulCatches,
                FailedCatches = _viewModel.FailedCatches,
                SuccessRate = _viewModel.SuccessRate,
                TotalDecisions = _viewModel.TotalDecisions,
                DecisionSuccessRate = _viewModel.DecisionSuccessRate,
                ActiveBehaviors = _viewModel.ActiveBehaviors,
                TotalBehaviors = _viewModel.TotalBehaviors,
                LastError = _viewModel.LastError,
                HasError = _viewModel.HasError
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao obter estatísticas: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Método público para aplicar configurações
    /// </summary>
    public async Task ApplyConfigurationAsync(FishingConfiguration config)
    {
        try
        {
            if (_viewModel == null) return;
            
            _viewModel.IsAutoFishingEnabled = config.IsAutoFishingEnabled;
            _viewModel.IsMinigameResolutionEnabled = config.IsMinigameResolutionEnabled;
            _viewModel.EnableAIDecisions = config.EnableAIDecisions;
            _viewModel.MinHealthPercentage = config.MinHealthPercentage;
            _viewModel.MinEnergyPercentage = config.MinEnergyPercentage;
            _viewModel.MaxAttempts = config.MaxAttempts;
            _viewModel.CooldownSeconds = config.CooldownSeconds;
            _viewModel.MaxConsecutiveFailures = config.MaxConsecutiveFailures;
            _viewModel.AIConfidenceThreshold = config.AIConfidenceThreshold;
            
            await _viewModel.ApplySettingsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao aplicar configurações: {ex.Message}", 
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

/// <summary>
/// Estatísticas do sistema de pesca
/// </summary>
public class FishingSystemStats
{
    public bool IsActive { get; set; }
    public bool IsDecisionEngineActive { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public int FishingAttempts { get; set; }
    public int SuccessfulCatches { get; set; }
    public int FailedCatches { get; set; }
    public double SuccessRate { get; set; }
    public int TotalDecisions { get; set; }
    public double DecisionSuccessRate { get; set; }
    public int ActiveBehaviors { get; set; }
    public int TotalBehaviors { get; set; }
    public string LastError { get; set; } = string.Empty;
    public bool HasError { get; set; }
}

/// <summary>
/// Configuração do sistema de pesca
/// </summary>
public class FishingConfiguration
{
    public bool IsAutoFishingEnabled { get; set; } = true;
    public bool IsMinigameResolutionEnabled { get; set; } = true;
    public bool EnableAIDecisions { get; set; } = false;
    public int MinHealthPercentage { get; set; } = 50;
    public int MinEnergyPercentage { get; set; } = 30;
    public int MaxAttempts { get; set; } = 10;
    public int CooldownSeconds { get; set; } = 5;
    public int MaxConsecutiveFailures { get; set; } = 3;
    public double AIConfidenceThreshold { get; set; } = 70;
}
