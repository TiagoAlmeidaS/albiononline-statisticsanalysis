using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlbionFishing.Vision.Configuration;

/// <summary>
/// Configuração centralizada para o sistema de visão
/// </summary>
public class VisionConfiguration
{
    private readonly ILogger<VisionConfiguration>? _logger;
    private readonly IConfiguration? _configuration;
    
    public VisionConfiguration(ILogger<VisionConfiguration>? logger = null, IConfiguration? configuration = null)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Carregar configurações padrão
        LoadDefaultConfiguration();
        
        // Carregar configurações do arquivo se disponível
        LoadFromConfiguration();
    }
    
    #region Propriedades de Configuração
    
    /// <summary>
    /// Diretório base para imagens de template
    /// </summary>
    public string ImagesBaseDirectory { get; set; } = "Data/images";
    
    /// <summary>
    /// Caminho do template do bobber básico
    /// </summary>
    public string BobberTemplatePath { get; set; } = "bobber.png";
    
    /// <summary>
    /// Caminho do template do bobber na água
    /// </summary>
    public string BobberInWaterTemplatePath { get; set; } = "bobber_in_water.png";
    
    /// <summary>
    /// Caminho do template alternativo do bobber na água
    /// </summary>
    public string BobberInWaterTemplatePath2 { get; set; } = "bobber_in_water2.png";
    
    /// <summary>
    /// Diretório de fallback para recursos
    /// </summary>
    public string ResourcesFallbackDirectory { get; set; } = "resources";
    
    /// <summary>
    /// Habilitar busca automática de templates
    /// </summary>
    public bool EnableAutoTemplateDiscovery { get; set; } = true;
    
    /// <summary>
    /// Habilitar cache de templates
    /// </summary>
    public bool EnableTemplateCache { get; set; } = true;
    
    /// <summary>
    /// Timeout para carregamento de templates (ms)
    /// </summary>
    public int TemplateLoadTimeoutMs { get; set; } = 5000;
    
    /// <summary>
    /// Habilitar validação de templates
    /// </summary>
    public bool EnableTemplateValidation { get; set; } = true;
    
    /// <summary>
    /// Tamanho mínimo de template (pixels)
    /// </summary>
    public int MinTemplateSize { get; set; } = 16;
    
    /// <summary>
    /// Tamanho máximo de template (pixels)
    /// </summary>
    public int MaxTemplateSize { get; set; } = 128;
    
    /// <summary>
    /// Habilitar logs detalhados
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Habilitar debug visual
    /// </summary>
    public bool EnableVisualDebug { get; set; } = false;
    
    /// <summary>
    /// Diretório para salvar imagens de debug
    /// </summary>
    public string DebugImageDirectory { get; set; } = "debug/vision";
    
    /// <summary>
    /// Configurações específicas por detector
    /// </summary>
    public Dictionary<string, DetectorSpecificConfig> DetectorConfigs { get; set; } = new();
    
    #endregion
    
    #region Métodos de Configuração
    
    /// <summary>
    /// Obtém o caminho completo para um template
    /// </summary>
    public string GetTemplatePath(string templateName)
    {
        var candidates = GetTemplateCandidates(templateName);
        
        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                _logger?.LogDebug("Template encontrado: {TemplatePath}", fullPath);
                return fullPath;
            }
        }
        
        _logger?.LogWarning("Template não encontrado: {TemplateName}. Candidatos testados: {Candidates}", 
            templateName, string.Join(", ", candidates));
        
        return candidates[0]; // Retornar o primeiro candidato como fallback
    }
    
    /// <summary>
    /// Obtém todos os candidatos de caminho para um template
    /// </summary>
    public List<string> GetTemplateCandidates(string templateName)
    {
        var candidates = new List<string>();
        
        // 1. Caminho direto (se já for absoluto)
        if (Path.IsPathRooted(templateName))
        {
            candidates.Add(templateName);
            return candidates;
        }
        
        // 2. Relativo ao diretório base de imagens
        candidates.Add(Path.Combine(ImagesBaseDirectory, templateName));
        
        // 3. Relativo ao diretório de trabalho atual
        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), ImagesBaseDirectory, templateName));
        
        // 4. Relativo ao diretório base da aplicação
        var appBaseDir = AppContext.BaseDirectory;
        candidates.Add(Path.Combine(appBaseDir, ImagesBaseDirectory, templateName));
        candidates.Add(Path.Combine(appBaseDir, "win-x64", ImagesBaseDirectory, templateName));
        
        // 5. Fallback para recursos
        if (EnableAutoTemplateDiscovery)
        {
            candidates.Add(Path.Combine(ResourcesFallbackDirectory, templateName));
            candidates.Add(Path.Combine(appBaseDir, ResourcesFallbackDirectory, templateName));
        }
        
        // 6. Busca recursiva se habilitada
        if (EnableAutoTemplateDiscovery)
        {
            var foundPaths = FindTemplateRecursively(templateName);
            candidates.AddRange(foundPaths);
        }
        
        return candidates;
    }
    
    /// <summary>
    /// Valida se um template é válido
    /// </summary>
    public bool ValidateTemplate(string templatePath)
    {
        if (!EnableTemplateValidation)
            return true;
        
        try
        {
            if (!File.Exists(templatePath))
            {
                _logger?.LogWarning("Template não existe: {TemplatePath}", templatePath);
                return false;
            }
            
            var fileInfo = new FileInfo(templatePath);
            if (fileInfo.Length == 0)
            {
                _logger?.LogWarning("Template está vazio: {TemplatePath}", templatePath);
                return false;
            }
            
            // Verificar se é uma imagem válida tentando carregar
            using var bitmap = new System.Drawing.Bitmap(templatePath);
            if (bitmap.Width < MinTemplateSize || bitmap.Width > MaxTemplateSize ||
                bitmap.Height < MinTemplateSize || bitmap.Height > MaxTemplateSize)
            {
                _logger?.LogWarning("Template com tamanho inválido: {TemplatePath} ({Width}x{Height})", 
                    templatePath, bitmap.Width, bitmap.Height);
                return false;
            }
            
            _logger?.LogDebug("Template válido: {TemplatePath} ({Width}x{Height})", 
                templatePath, bitmap.Width, bitmap.Height);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao validar template: {TemplatePath}", templatePath);
            return false;
        }
    }
    
    /// <summary>
    /// Obtém configuração específica para um detector
    /// </summary>
    public DetectorSpecificConfig GetDetectorConfig(string detectorName)
    {
        if (DetectorConfigs.TryGetValue(detectorName, out var config))
        {
            return config;
        }
        
        // Criar configuração padrão se não existir
        var defaultConfig = new DetectorSpecificConfig();
        DetectorConfigs[detectorName] = defaultConfig;
        return defaultConfig;
    }
    
    /// <summary>
    /// Define configuração específica para um detector
    /// </summary>
    public void SetDetectorConfig(string detectorName, DetectorSpecificConfig config)
    {
        DetectorConfigs[detectorName] = config;
        _logger?.LogDebug("Configuração definida para detector {DetectorName}", detectorName);
    }
    
    /// <summary>
    /// Obtém o diretório de debug
    /// </summary>
    public string GetDebugDirectory()
    {
        var debugDir = Path.GetFullPath(DebugImageDirectory);
        if (!Directory.Exists(debugDir))
        {
            Directory.CreateDirectory(debugDir);
            _logger?.LogDebug("Diretório de debug criado: {DebugDirectory}", debugDir);
        }
        return debugDir;
    }
    
    #endregion
    
    #region Métodos Privados
    
    private void LoadDefaultConfiguration()
    {
        // Configurações padrão já definidas nas propriedades
        _logger?.LogDebug("Configurações padrão carregadas");
    }
    
    private void LoadFromConfiguration()
    {
        if (_configuration == null)
            return;
        
        try
        {
            ImagesBaseDirectory = _configuration.GetValue("Vision:ImagesBaseDirectory", ImagesBaseDirectory);
            BobberTemplatePath = _configuration.GetValue("Vision:BobberTemplatePath", BobberTemplatePath);
            BobberInWaterTemplatePath = _configuration.GetValue("Vision:BobberInWaterTemplatePath", BobberInWaterTemplatePath);
            BobberInWaterTemplatePath2 = _configuration.GetValue("Vision:BobberInWaterTemplatePath2", BobberInWaterTemplatePath2);
            ResourcesFallbackDirectory = _configuration.GetValue("Vision:ResourcesFallbackDirectory", ResourcesFallbackDirectory);
            EnableAutoTemplateDiscovery = _configuration.GetValue("Vision:EnableAutoTemplateDiscovery", EnableAutoTemplateDiscovery);
            EnableTemplateCache = _configuration.GetValue("Vision:EnableTemplateCache", EnableTemplateCache);
            TemplateLoadTimeoutMs = _configuration.GetValue("Vision:TemplateLoadTimeoutMs", TemplateLoadTimeoutMs);
            EnableTemplateValidation = _configuration.GetValue("Vision:EnableTemplateValidation", EnableTemplateValidation);
            MinTemplateSize = _configuration.GetValue("Vision:MinTemplateSize", MinTemplateSize);
            MaxTemplateSize = _configuration.GetValue("Vision:MaxTemplateSize", MaxTemplateSize);
            EnableDetailedLogging = _configuration.GetValue("Vision:EnableDetailedLogging", EnableDetailedLogging);
            EnableVisualDebug = _configuration.GetValue("Vision:EnableVisualDebug", EnableVisualDebug);
            DebugImageDirectory = _configuration.GetValue("Vision:DebugImageDirectory", DebugImageDirectory);
            
            _logger?.LogInformation("Configurações carregadas do arquivo de configuração");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao carregar configurações do arquivo");
        }
    }
    
    private List<string> FindTemplateRecursively(string templateName)
    {
        var foundPaths = new List<string>();
        
        try
        {
            var searchDirectories = new[]
            {
                ImagesBaseDirectory,
                ResourcesFallbackDirectory,
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };
            
            foreach (var searchDir in searchDirectories)
            {
                if (!Directory.Exists(searchDir))
                    continue;
                
                var files = Directory.GetFiles(searchDir, templateName, SearchOption.AllDirectories);
                foundPaths.AddRange(files);
                
                // Também buscar por variações do nome
                var nameWithoutExt = Path.GetFileNameWithoutExtension(templateName);
                var ext = Path.GetExtension(templateName);
                
                var variations = new[]
                {
                    $"{nameWithoutExt}_*{ext}",
                    $"{nameWithoutExt}*{ext}",
                    $"*{nameWithoutExt}*{ext}"
                };
                
                foreach (var variation in variations)
                {
                    var variationFiles = Directory.GetFiles(searchDir, variation, SearchOption.AllDirectories);
                    foundPaths.AddRange(variationFiles);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro durante busca recursiva de template: {TemplateName}", templateName);
        }
        
        return foundPaths;
    }
    
    #endregion
}

/// <summary>
/// Configuração específica para um detector
/// </summary>
public class DetectorSpecificConfig
{
    public string TemplatePath { get; set; } = string.Empty;
    public double ConfidenceThreshold { get; set; } = 0.5;
    public bool EnableDebugWindow { get; set; } = false;
    public bool EnableColorFiltering { get; set; } = true;
    public bool EnableHSVFiltering { get; set; } = true;
    public bool EnableGradientChannel { get; set; } = false;
    public bool EnableMultiScale { get; set; } = false;
    public bool EnableSignalAnalysis { get; set; } = false;
    public double[] Scales { get; set; } = { 0.60, 0.70, 0.80, 0.90, 1.00, 1.10, 1.20 };
    public int MaxProcessingTimeMs { get; set; } = 100;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}
