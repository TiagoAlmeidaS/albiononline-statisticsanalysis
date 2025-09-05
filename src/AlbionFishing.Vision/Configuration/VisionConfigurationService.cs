using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AlbionFishing.Vision.Configuration;

/// <summary>
/// Implementação do serviço de configuração de visão
/// </summary>
public class VisionConfigurationService : IVisionConfigurationService
{
    private readonly ILogger<VisionConfigurationService> _logger;
    private VisionConfiguration _configuration;
    
    public VisionConfiguration Configuration => _configuration;
    
    public event EventHandler<ConfigurationUpdatedEventArgs>? ConfigurationUpdated;
    
    public VisionConfigurationService(
        ILogger<VisionConfigurationService> logger,
        VisionConfiguration? initialConfiguration = null)
    {
        _logger = logger;
        _configuration = initialConfiguration ?? new VisionConfiguration();
        
        _logger.LogInformation("Serviço de configuração de visão inicializado");
        _logger.LogDebug("Diretório base de imagens: {ImagesBaseDirectory}", _configuration.ImagesBaseDirectory);
        _logger.LogDebug("Template do bobber: {BobberTemplatePath}", _configuration.BobberTemplatePath);
        _logger.LogDebug("Template do bobber na água: {BobberInWaterTemplatePath}", _configuration.BobberInWaterTemplatePath);
    }
    
    public string GetTemplatePath(string templateName)
    {
        try
        {
            var path = _configuration.GetTemplatePath(templateName);
            _logger.LogDebug("Caminho do template {TemplateName}: {TemplatePath}", templateName, path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter caminho do template: {TemplateName}", templateName);
            return templateName; // Fallback para o nome original
        }
    }
    
    public List<string> GetTemplateCandidates(string templateName)
    {
        try
        {
            var candidates = _configuration.GetTemplateCandidates(templateName);
            _logger.LogDebug("Candidatos para template {TemplateName}: {Candidates}", 
                templateName, string.Join(", ", candidates));
            return candidates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter candidatos do template: {TemplateName}", templateName);
            return new List<string> { templateName };
        }
    }
    
    public bool ValidateTemplate(string templatePath)
    {
        try
        {
            var isValid = _configuration.ValidateTemplate(templatePath);
            _logger.LogDebug("Validação do template {TemplatePath}: {IsValid}", templatePath, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar template: {TemplatePath}", templatePath);
            return false;
        }
    }
    
    public DetectorSpecificConfig GetDetectorConfig(string detectorName)
    {
        try
        {
            var config = _configuration.GetDetectorConfig(detectorName);
            _logger.LogDebug("Configuração obtida para detector {DetectorName}", detectorName);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração do detector: {DetectorName}", detectorName);
            return new DetectorSpecificConfig();
        }
    }
    
    public void SetDetectorConfig(string detectorName, DetectorSpecificConfig config)
    {
        try
        {
            _configuration.SetDetectorConfig(detectorName, config);
            _logger.LogInformation("Configuração definida para detector {DetectorName}", detectorName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir configuração do detector: {DetectorName}", detectorName);
        }
    }
    
    public string GetDebugDirectory()
    {
        try
        {
            var debugDir = _configuration.GetDebugDirectory();
            _logger.LogDebug("Diretório de debug: {DebugDirectory}", debugDir);
            return debugDir;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter diretório de debug");
            return "debug/vision";
        }
    }
    
    public void UpdateConfiguration(VisionConfiguration newConfiguration)
    {
        try
        {
            var oldConfiguration = _configuration;
            _configuration = newConfiguration;
            
            _logger.LogInformation("Configuração de visão atualizada");
            
            // Disparar evento
            ConfigurationUpdated?.Invoke(this, new ConfigurationUpdatedEventArgs(oldConfiguration, newConfiguration));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar configuração de visão");
        }
    }
}
