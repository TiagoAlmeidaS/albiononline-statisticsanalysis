using System;

namespace AlbionFishing.Automation;

/// <summary>
/// Fábrica para criar a implementação de automação apropriada para a plataforma
/// </summary>
public static class AutomationServiceFactory
{
    /// <summary>
    /// Cria a implementação de automação apropriada para a plataforma atual
    /// </summary>
    public static IAutomationService Create()
    {
        // Windows
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return new WindowsAutomationService();
        }
        
        // Mac/Linux
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            return new UnixAutomationService();
        }
        
        throw new PlatformNotSupportedException(
            $"Plataforma não suportada: {Environment.OSVersion.Platform}. " +
            "Suportadas: Windows, macOS, Linux");
    }
    
    /// <summary>
    /// Verifica se a plataforma atual é suportada
    /// </summary>
    public static bool IsPlatformSupported()
    {
        try
        {
            var service = Create();
            return service.IsSupported();
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Obtém informações sobre a plataforma atual
    /// </summary>
    public static string GetPlatformInfo()
    {
        return $"OS: {Environment.OSVersion.Platform}, " +
               $"Version: {Environment.OSVersion.Version}, " +
               $"64-bit: {Environment.Is64BitOperatingSystem}";
    }
}