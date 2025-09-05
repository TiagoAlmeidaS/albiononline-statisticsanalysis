using System;
using System.Collections.Generic;

namespace AlbionFishing.Vision.Configuration;

/// <summary>
/// Interface para o serviço de configuração de visão
/// </summary>
public interface IVisionConfigurationService
{
    /// <summary>
    /// Obtém a configuração atual
    /// </summary>
    VisionConfiguration Configuration { get; }
    
    /// <summary>
    /// Obtém o caminho completo para um template
    /// </summary>
    /// <param name="templateName">Nome do template</param>
    /// <returns>Caminho completo do template</returns>
    string GetTemplatePath(string templateName);
    
    /// <summary>
    /// Obtém todos os candidatos de caminho para um template
    /// </summary>
    /// <param name="templateName">Nome do template</param>
    /// <returns>Lista de candidatos de caminho</returns>
    List<string> GetTemplateCandidates(string templateName);
    
    /// <summary>
    /// Valida se um template é válido
    /// </summary>
    /// <param name="templatePath">Caminho do template</param>
    /// <returns>True se válido</returns>
    bool ValidateTemplate(string templatePath);
    
    /// <summary>
    /// Obtém configuração específica para um detector
    /// </summary>
    /// <param name="detectorName">Nome do detector</param>
    /// <returns>Configuração do detector</returns>
    DetectorSpecificConfig GetDetectorConfig(string detectorName);
    
    /// <summary>
    /// Define configuração específica para um detector
    /// </summary>
    /// <param name="detectorName">Nome do detector</param>
    /// <param name="config">Configuração do detector</param>
    void SetDetectorConfig(string detectorName, DetectorSpecificConfig config);
    
    /// <summary>
    /// Obtém o diretório de debug
    /// </summary>
    /// <returns>Caminho do diretório de debug</returns>
    string GetDebugDirectory();
    
    /// <summary>
    /// Atualiza a configuração
    /// </summary>
    /// <param name="newConfiguration">Nova configuração</param>
    void UpdateConfiguration(VisionConfiguration newConfiguration);
    
    /// <summary>
    /// Evento disparado quando a configuração é atualizada
    /// </summary>
    event EventHandler<ConfigurationUpdatedEventArgs>? ConfigurationUpdated;
}

/// <summary>
/// Argumentos do evento de atualização de configuração
/// </summary>
public class ConfigurationUpdatedEventArgs : EventArgs
{
    public VisionConfiguration OldConfiguration { get; }
    public VisionConfiguration NewConfiguration { get; }
    public DateTime UpdateTime { get; }
    
    public ConfigurationUpdatedEventArgs(VisionConfiguration oldConfiguration, VisionConfiguration newConfiguration)
    {
        OldConfiguration = oldConfiguration;
        NewConfiguration = newConfiguration;
        UpdateTime = DateTime.UtcNow;
    }
}
