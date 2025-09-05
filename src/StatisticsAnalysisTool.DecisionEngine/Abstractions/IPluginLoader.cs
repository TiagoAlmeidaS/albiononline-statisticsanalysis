using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para carregamento de plugins
    /// </summary>
    public interface IPluginLoader
    {
        /// <summary>
        /// Carrega um plugin de um arquivo
        /// </summary>
        Task<IPlugin> LoadFromFileAsync(string filePath);
        
        /// <summary>
        /// Carrega um plugin de um assembly
        /// </summary>
        Task<IPlugin> LoadFromAssemblyAsync(string assemblyPath, string typeName);
        
        /// <summary>
        /// Carrega plugins de um diretório
        /// </summary>
        Task<List<IPlugin>> LoadFromDirectoryAsync(string directoryPath);
        
        /// <summary>
        /// Carrega plugins de um diretório com filtro
        /// </summary>
        Task<List<IPlugin>> LoadFromDirectoryAsync(string directoryPath, string searchPattern);
        
        /// <summary>
        /// Valida um arquivo de plugin
        /// </summary>
        Task<PluginValidation> ValidateFileAsync(string filePath);
        
        /// <summary>
        /// Obtém informações de um arquivo de plugin
        /// </summary>
        Task<PluginInfo> GetFileInfoAsync(string filePath);
        
        /// <summary>
        /// Obtém dependências de um arquivo de plugin
        /// </summary>
        Task<List<string>> GetFileDependenciesAsync(string filePath);
        
        /// <summary>
        /// Obtém configuração do carregador
        /// </summary>
        Task<PluginLoaderConfiguration> GetConfigurationAsync();
        
        /// <summary>
        /// Atualiza configuração do carregador
        /// </summary>
        Task UpdateConfigurationAsync(PluginLoaderConfiguration config);
    }
    
    /// <summary>
    /// Configuração do carregador de plugins
    /// </summary>
    public class PluginLoaderConfiguration
    {
        public List<string> SearchPaths { get; set; } = new();
        public List<string> AllowedExtensions { get; set; } = new() { ".dll", ".exe" };
        public bool EnableHotReload { get; set; } = false;
        public bool EnableSandboxing { get; set; } = true;
        public TimeSpan LoadTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }
}
