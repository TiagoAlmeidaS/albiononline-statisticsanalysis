using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Common;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision.Detectors;
using AlbionFishing.Vision.DependencyInjection;

namespace StatisticsAnalysisTool.ViewModels;

/// <summary>
/// ViewModel para configuração do sistema de visão
/// </summary>
public class VisionConfigurationViewModel : BaseViewModel
{
    private readonly IVisionConfigurationService _visionConfigService;
    private readonly IBobberDetectorFactory _detectorFactory;
    private readonly ILogger<VisionConfigurationViewModel> _logger;
    
    // Status do sistema
    private bool _isVisionSystemActive;
    private string _visionStatusText = "Sistema Inativo";
    private Brush _visionStatusColor = Brushes.Red;
    private int _validTemplatesCount;
    private int _totalTemplatesCount;
    private string _currentDetectorType = "Nenhum";
    
    // Configurações básicas
    private string _imagesBaseDirectory = "Data/images";
    private bool _enableAutoDiscovery = true;
    private bool _enableTemplateCache = true;
    private bool _enableTemplateValidation = true;
    private bool _enableDetailedLogging = false;
    private bool _enableVisualDebug = false;
    
    // Configurações de validação
    private int _minTemplateSize = 16;
    private int _maxTemplateSize = 128;
    private int _templateLoadTimeoutMs = 5000;
    
    // Configurações de debug
    private string _debugImageDirectory = "debug/vision";
    private bool _saveDebugImages = false;
    private bool _showDebugWindows = false;
    private bool _enablePerformanceLogging = false;
    
    // Configurações do detector
    private DetectorInfo _selectedDetector;
    private TemplateInfo _selectedTemplate;
    private double _confidenceThreshold = 0.7;
    private bool _enableDebugWindow = false;
    private bool _enableColorFiltering = true;
    private bool _enableHsvAnalysis = true;
    private bool _enableMultiScale = false;
    private bool _enableSignalAnalysis = false;
    
    // Teste
    private string _testResult = "Nenhum teste executado";
    
    // Coleções
    private ObservableCollection<DetectorInfo> _availableDetectors = new();
    private ObservableCollection<TemplateInfo> _availableTemplates = new();
    private ObservableCollection<TemplateStatusInfo> _templateStatuses = new();
    private ObservableCollection<TestLogEntry> _testLog = new();
    
    public VisionConfigurationViewModel(
        IVisionConfigurationService visionConfigService,
        IBobberDetectorFactory detectorFactory,
        ILogger<VisionConfigurationViewModel> logger)
    {
        _visionConfigService = visionConfigService;
        _detectorFactory = detectorFactory;
        _logger = logger;
        
        // Inicializar comandos
        InitializeCommands();
        
        // Carregar configurações
        LoadConfiguration();
        
        // Inicializar detectores e templates
        InitializeDetectors();
        InitializeTemplates();
        
        // Validar templates
        ValidateTemplatesAsync();
    }
    
    #region Propriedades Públicas
    
    // Status do sistema
    public bool IsVisionSystemActive
    {
        get => _isVisionSystemActive;
        set => SetProperty(ref _isVisionSystemActive, value);
    }
    
    public string VisionStatusText
    {
        get => _visionStatusText;
        set => SetProperty(ref _visionStatusText, value);
    }
    
    public Brush VisionStatusColor
    {
        get => _visionStatusColor;
        set => SetProperty(ref _visionStatusColor, value);
    }
    
    public int ValidTemplatesCount
    {
        get => _validTemplatesCount;
        set => SetProperty(ref _validTemplatesCount, value);
    }
    
    public int TotalTemplatesCount
    {
        get => _totalTemplatesCount;
        set => SetProperty(ref _totalTemplatesCount, value);
    }
    
    public string CurrentDetectorType
    {
        get => _currentDetectorType;
        set => SetProperty(ref _currentDetectorType, value);
    }
    
    // Configurações básicas
    public string ImagesBaseDirectory
    {
        get => _imagesBaseDirectory;
        set => SetProperty(ref _imagesBaseDirectory, value);
    }
    
    public bool EnableAutoDiscovery
    {
        get => _enableAutoDiscovery;
        set => SetProperty(ref _enableAutoDiscovery, value);
    }
    
    public bool EnableTemplateCache
    {
        get => _enableTemplateCache;
        set => SetProperty(ref _enableTemplateCache, value);
    }
    
    public bool EnableTemplateValidation
    {
        get => _enableTemplateValidation;
        set => SetProperty(ref _enableTemplateValidation, value);
    }
    
    public bool EnableDetailedLogging
    {
        get => _enableDetailedLogging;
        set => SetProperty(ref _enableDetailedLogging, value);
    }
    
    public bool EnableVisualDebug
    {
        get => _enableVisualDebug;
        set => SetProperty(ref _enableVisualDebug, value);
    }
    
    // Configurações de validação
    public int MinTemplateSize
    {
        get => _minTemplateSize;
        set => SetProperty(ref _minTemplateSize, value);
    }
    
    public int MaxTemplateSize
    {
        get => _maxTemplateSize;
        set => SetProperty(ref _maxTemplateSize, value);
    }
    
    public int TemplateLoadTimeoutMs
    {
        get => _templateLoadTimeoutMs;
        set => SetProperty(ref _templateLoadTimeoutMs, value);
    }
    
    // Configurações de debug
    public string DebugImageDirectory
    {
        get => _debugImageDirectory;
        set => SetProperty(ref _debugImageDirectory, value);
    }
    
    public bool SaveDebugImages
    {
        get => _saveDebugImages;
        set => SetProperty(ref _saveDebugImages, value);
    }
    
    public bool ShowDebugWindows
    {
        get => _showDebugWindows;
        set => SetProperty(ref _showDebugWindows, value);
    }
    
    public bool EnablePerformanceLogging
    {
        get => _enablePerformanceLogging;
        set => SetProperty(ref _enablePerformanceLogging, value);
    }
    
    // Configurações do detector
    public DetectorInfo SelectedDetector
    {
        get => _selectedDetector;
        set
        {
            SetProperty(ref _selectedDetector, value);
            OnDetectorChanged();
        }
    }
    
    public TemplateInfo SelectedTemplate
    {
        get => _selectedTemplate;
        set => SetProperty(ref _selectedTemplate, value);
    }
    
    public double ConfidenceThreshold
    {
        get => _confidenceThreshold;
        set => SetProperty(ref _confidenceThreshold, value);
    }
    
    public bool EnableDebugWindow
    {
        get => _enableDebugWindow;
        set => SetProperty(ref _enableDebugWindow, value);
    }
    
    public bool EnableColorFiltering
    {
        get => _enableColorFiltering;
        set => SetProperty(ref _enableColorFiltering, value);
    }
    
    public bool EnableHsvAnalysis
    {
        get => _enableHsvAnalysis;
        set => SetProperty(ref _enableHsvAnalysis, value);
    }
    
    public bool EnableMultiScale
    {
        get => _enableMultiScale;
        set => SetProperty(ref _enableMultiScale, value);
    }
    
    public bool EnableSignalAnalysis
    {
        get => _enableSignalAnalysis;
        set => SetProperty(ref _enableSignalAnalysis, value);
    }
    
    // Teste
    public string TestResult
    {
        get => _testResult;
        set => SetProperty(ref _testResult, value);
    }
    
    // Coleções
    public ObservableCollection<DetectorInfo> AvailableDetectors
    {
        get => _availableDetectors;
        set => SetProperty(ref _availableDetectors, value);
    }
    
    public ObservableCollection<TemplateInfo> AvailableTemplates
    {
        get => _availableTemplates;
        set => SetProperty(ref _availableTemplates, value);
    }
    
    public ObservableCollection<TemplateStatusInfo> TemplateStatuses
    {
        get => _templateStatuses;
        set => SetProperty(ref _templateStatuses, value);
    }
    
    public ObservableCollection<TestLogEntry> TestLog
    {
        get => _testLog;
        set => SetProperty(ref _testLog, value);
    }
    
    #endregion
    
    #region Comandos
    
    public RelayCommand ValidateTemplatesCommand { get; private set; }
    public RelayCommand TestDetectionCommand { get; private set; }
    public RelayCommand ApplyConfigurationCommand { get; private set; }
    public RelayCommand BrowseDirectoryCommand { get; private set; }
    public RelayCommand BrowseDebugDirectoryCommand { get; private set; }
    public RelayCommand RefreshTemplatesCommand { get; private set; }
    public RelayCommand CaptureTestAreaCommand { get; private set; }
    public RelayCommand ExecuteTestCommand { get; private set; }
    public RelayCommand ClearTestLogCommand { get; private set; }
    
    #endregion
    
    #region Métodos Privados
    
    private void InitializeCommands()
    {
        ValidateTemplatesCommand = new RelayCommand(async () => await ValidateTemplatesAsync());
        TestDetectionCommand = new RelayCommand(async () => await TestDetectionAsync());
        ApplyConfigurationCommand = new RelayCommand(async () => await ApplyConfigurationAsync());
        BrowseDirectoryCommand = new RelayCommand(BrowseDirectory);
        BrowseDebugDirectoryCommand = new RelayCommand(BrowseDebugDirectory);
        RefreshTemplatesCommand = new RelayCommand(async () => await RefreshTemplatesAsync());
        CaptureTestAreaCommand = new RelayCommand(CaptureTestArea);
        ExecuteTestCommand = new RelayCommand(async () => await ExecuteTestAsync());
        ClearTestLogCommand = new RelayCommand(ClearTestLog);
    }
    
    private void LoadConfiguration()
    {
        try
        {
            var config = _visionConfigService.Configuration;
            
            ImagesBaseDirectory = config.ImagesBaseDirectory;
            EnableAutoDiscovery = config.EnableAutoTemplateDiscovery;
            EnableTemplateCache = config.EnableTemplateCache;
            EnableTemplateValidation = config.EnableTemplateValidation;
            EnableDetailedLogging = config.EnableDetailedLogging;
            EnableVisualDebug = config.EnableVisualDebug;
            
            MinTemplateSize = config.MinTemplateSize;
            MaxTemplateSize = config.MaxTemplateSize;
            TemplateLoadTimeoutMs = config.TemplateLoadTimeoutMs;
            
            DebugImageDirectory = config.DebugImageDirectory;
            
            _logger.LogInformation("Configuração de visão carregada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar configuração de visão");
        }
    }
    
    private void InitializeDetectors()
    {
        AvailableDetectors.Clear();
        
        AvailableDetectors.Add(new DetectorInfo
        {
            Type = DetectorType.Template,
            Name = "Template Matcher",
            Description = "Detector básico baseado em template matching",
            IsAvailable = true
        });
        
        AvailableDetectors.Add(new DetectorInfo
        {
            Type = DetectorType.SignalEnhanced,
            Name = "Signal Enhanced",
            Description = "Detector avançado com análise de sinais",
            IsAvailable = true
        });
        
        AvailableDetectors.Add(new DetectorInfo
        {
            Type = DetectorType.Hybrid,
            Name = "Hybrid Ensemble",
            Description = "Detector híbrido combinando múltiplas técnicas",
            IsAvailable = true
        });
        
        SelectedDetector = AvailableDetectors.FirstOrDefault();
    }
    
    private void InitializeTemplates()
    {
        AvailableTemplates.Clear();
        
        var templates = new[]
        {
            "bobber.png",
            "bobber_in_water.png",
            "bobber_in_water2.png"
        };
        
        foreach (var template in templates)
        {
            AvailableTemplates.Add(new TemplateInfo
            {
                Name = template,
                Path = _visionConfigService.GetTemplatePath(template),
                IsValid = _visionConfigService.ValidateTemplate(_visionConfigService.GetTemplatePath(template))
            });
        }
        
        SelectedTemplate = AvailableTemplates.FirstOrDefault();
    }
    
    private async Task ValidateTemplatesAsync()
    {
        try
        {
            AddTestLogEntry("Validação", "Iniciando validação de templates...", "Info");
            
            TemplateStatuses.Clear();
            var validCount = 0;
            var totalCount = 0;
            
            foreach (var template in AvailableTemplates)
            {
                var status = new TemplateStatusInfo
                {
                    Name = template.Name,
                    Path = template.Path,
                    Status = "Validando...",
                    Size = "0x0",
                    LastModified = DateTime.MinValue
                };
                
                TemplateStatuses.Add(status);
                
                try
                {
                    var isValid = _visionConfigService.ValidateTemplate(template.Path);
                    var fileInfo = new FileInfo(template.Path);
                    
                    status.Status = isValid ? "Válido" : "Inválido";
                    status.Size = fileInfo.Exists ? $"{GetImageSize(template.Path)}" : "N/A";
                    status.LastModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue;
                    
                    if (isValid) validCount++;
                    totalCount++;
                    
                    AddTestLogEntry("Validação", $"Template {template.Name}: {status.Status}", 
                        isValid ? "Success" : "Error");
                }
                catch (Exception ex)
                {
                    status.Status = "Erro";
                    status.Size = "N/A";
                    status.LastModified = DateTime.MinValue;
                    
                    AddTestLogEntry("Validação", $"Erro ao validar {template.Name}: {ex.Message}", "Error");
                }
            }
            
            ValidTemplatesCount = validCount;
            TotalTemplatesCount = totalCount;
            
            UpdateVisionStatus();
            
            AddTestLogEntry("Validação", $"Validação concluída: {validCount}/{totalCount} templates válidos", 
                validCount > 0 ? "Success" : "Warning");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante validação de templates");
            AddTestLogEntry("Validação", $"Erro durante validação: {ex.Message}", "Error");
        }
    }
    
    private async Task TestDetectionAsync()
    {
        try
        {
            AddTestLogEntry("Teste", "Iniciando teste de detecção...", "Info");
            
            if (SelectedDetector == null)
            {
                AddTestLogEntry("Teste", "Nenhum detector selecionado", "Error");
                return;
            }
            
            var detector = _detectorFactory.CreateDetector(SelectedDetector.Type);
            var testArea = new System.Drawing.Rectangle(100, 100, 600, 400);
            
            var result = detector.DetectInArea(testArea, ConfidenceThreshold);
            
            TestResult = $"Detecção: {result.Detected}, Score: {result.Score:F3}, Posição: ({result.PositionX:F1}, {result.PositionY:F1})";
            
            AddTestLogEntry("Teste", $"Teste concluído: {TestResult}", 
                result.Detected ? "Success" : "Info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante teste de detecção");
            TestResult = $"Erro: {ex.Message}";
            AddTestLogEntry("Teste", $"Erro durante teste: {ex.Message}", "Error");
        }
    }
    
    private async Task ApplyConfigurationAsync()
    {
        try
        {
            AddTestLogEntry("Configuração", "Aplicando configurações...", "Info");
            
            var config = new VisionConfiguration
            {
                ImagesBaseDirectory = ImagesBaseDirectory,
                EnableAutoTemplateDiscovery = EnableAutoDiscovery,
                EnableTemplateCache = EnableTemplateCache,
                EnableTemplateValidation = EnableTemplateValidation,
                EnableDetailedLogging = EnableDetailedLogging,
                EnableVisualDebug = EnableVisualDebug,
                MinTemplateSize = MinTemplateSize,
                MaxTemplateSize = MaxTemplateSize,
                TemplateLoadTimeoutMs = TemplateLoadTimeoutMs,
                DebugImageDirectory = DebugImageDirectory
            };
            
            _visionConfigService.UpdateConfiguration(config);
            
            // Aplicar configuração do detector selecionado
            if (SelectedDetector != null && SelectedTemplate != null)
            {
                var detectorConfig = new DetectorSpecificConfig
                {
                    TemplatePath = SelectedTemplate.Path,
                    ConfidenceThreshold = ConfidenceThreshold,
                    EnableDebugWindow = EnableDebugWindow,
                    EnableColorFiltering = EnableColorFiltering,
                    EnableHSVFiltering = EnableHsvAnalysis,
                    EnableMultiScale = EnableMultiScale,
                    EnableSignalAnalysis = EnableSignalAnalysis
                };
                
                _visionConfigService.SetDetectorConfig(SelectedDetector.Name, detectorConfig);
            }
            
            UpdateVisionStatus();
            
            AddTestLogEntry("Configuração", "Configurações aplicadas com sucesso", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aplicar configurações");
            AddTestLogEntry("Configuração", $"Erro ao aplicar configurações: {ex.Message}", "Error");
        }
    }
    
    private void BrowseDirectory()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Selecionar Diretório de Imagens",
            InitialDirectory = ImagesBaseDirectory
        };
        
        if (dialog.ShowDialog() == true)
        {
            ImagesBaseDirectory = dialog.FolderName;
        }
    }
    
    private void BrowseDebugDirectory()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Selecionar Diretório de Debug",
            InitialDirectory = DebugImageDirectory
        };
        
        if (dialog.ShowDialog() == true)
        {
            DebugImageDirectory = dialog.FolderName;
        }
    }
    
    private async Task RefreshTemplatesAsync()
    {
        try
        {
            AddTestLogEntry("Atualização", "Atualizando lista de templates...", "Info");
            
            InitializeTemplates();
            await ValidateTemplatesAsync();
            
            AddTestLogEntry("Atualização", "Lista de templates atualizada", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar templates");
            AddTestLogEntry("Atualização", $"Erro ao atualizar templates: {ex.Message}", "Error");
        }
    }
    
    private void CaptureTestArea()
    {
        try
        {
            AddTestLogEntry("Captura", "Capturando área de teste...", "Info");
            
            // Implementar captura de área de teste
            // Por enquanto, simular
            TestResult = "Área de teste capturada (simulado)";
            
            AddTestLogEntry("Captura", "Área de teste capturada", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao capturar área de teste");
            AddTestLogEntry("Captura", $"Erro ao capturar área: {ex.Message}", "Error");
        }
    }
    
    private async Task ExecuteTestAsync()
    {
        await TestDetectionAsync();
    }
    
    private void ClearTestLog()
    {
        TestLog.Clear();
    }
    
    private void OnDetectorChanged()
    {
        if (SelectedDetector != null)
        {
            CurrentDetectorType = SelectedDetector.Name;
            _logger.LogDebug("Detector alterado para: {DetectorType}", SelectedDetector.Name);
        }
    }
    
    private void UpdateVisionStatus()
    {
        if (ValidTemplatesCount > 0 && TotalTemplatesCount > 0)
        {
            IsVisionSystemActive = true;
            VisionStatusText = "Sistema Ativo";
            VisionStatusColor = Brushes.Green;
        }
        else
        {
            IsVisionSystemActive = false;
            VisionStatusText = "Sistema Inativo";
            VisionStatusColor = Brushes.Red;
        }
    }
    
    private void AddTestLogEntry(string type, string details, string status)
    {
        TestLog.Add(new TestLogEntry
        {
            Timestamp = DateTime.Now,
            Type = type,
            Status = status,
            Details = details
        });
        
        // Manter apenas os últimos 100 registros
        if (TestLog.Count > 100)
        {
            TestLog.RemoveAt(0);
        }
    }
    
    private string GetImageSize(string imagePath)
    {
        try
        {
            if (File.Exists(imagePath))
            {
                using var bitmap = new System.Drawing.Bitmap(imagePath);
                return $"{bitmap.Width}x{bitmap.Height}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao obter tamanho da imagem: {ImagePath}", imagePath);
        }
        
        return "N/A";
    }
    
    #endregion
}

#region Classes de Dados

public class DetectorInfo
{
    public DetectorType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class TemplateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsValid { get; set; }
}

public class TemplateStatusInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

public class TestLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

#endregion
