using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para gerenciamento de plugins
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// Plugins carregados
        /// </summary>
        IReadOnlyDictionary<string, IPlugin> LoadedPlugins { get; }
        
        /// <summary>
        /// Evento disparado quando um plugin é carregado
        /// </summary>
        event EventHandler<PluginLoadedEventArgs> PluginLoaded;
        
        /// <summary>
        /// Evento disparado quando um plugin é descarregado
        /// </summary>
        event EventHandler<PluginUnloadedEventArgs> PluginUnloaded;
        
        /// <summary>
        /// Inicializa o gerenciador
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o gerenciador
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Carrega um plugin
        /// </summary>
        Task<IPlugin> LoadPluginAsync(string pluginPath);
        
        /// <summary>
        /// Descarrega um plugin
        /// </summary>
        Task UnloadPluginAsync(string pluginName);
        
        /// <summary>
        /// Recarrega um plugin
        /// </summary>
        Task ReloadPluginAsync(string pluginName);
        
        /// <summary>
        /// Obtém um plugin por nome
        /// </summary>
        Task<IPlugin?> GetPluginAsync(string pluginName);
        
        /// <summary>
        /// Obtém plugins por tipo
        /// </summary>
        Task<IEnumerable<IPlugin>> GetPluginsByTypeAsync<T>() where T : IPlugin;
        
        /// <summary>
        /// Obtém plugins habilitados
        /// </summary>
        Task<IEnumerable<IPlugin>> GetEnabledPluginsAsync();
        
        /// <summary>
        /// Habilita/desabilita um plugin
        /// </summary>
        Task SetPluginEnabledAsync(string pluginName, bool enabled);
        
        /// <summary>
        /// Obtém informações de um plugin
        /// </summary>
        Task<PluginInfo> GetPluginInfoAsync(string pluginName);
        
        /// <summary>
        /// Obtém estatísticas de plugins
        /// </summary>
        Task<PluginManagerStats> GetStatsAsync();
        
        /// <summary>
        /// Valida um plugin
        /// </summary>
        Task<PluginValidation> ValidatePluginAsync(string pluginPath);
        
        /// <summary>
        /// Obtém dependências de um plugin
        /// </summary>
        Task<List<string>> GetPluginDependenciesAsync(string pluginName);
        
        /// <summary>
        /// Resolve dependências de plugins
        /// </summary>
        Task<List<string>> ResolveDependenciesAsync(List<string> pluginNames);
    }
    
    /// <summary>
    /// Interface base para plugins
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        string Author { get; }
        bool IsEnabled { get; set; }
        bool IsLoaded { get; }
        
        Task InitializeAsync();
        Task ShutdownAsync();
        Task<PluginInfo> GetInfoAsync();
        Task<PluginValidation> ValidateAsync();
    }
    
    /// <summary>
    /// Informações de um plugin
    /// </summary>
    public class PluginInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime LoadTime { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsLoaded { get; set; }
    }
    
    /// <summary>
    /// Validação de plugin
    /// </summary>
    public class PluginValidation
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public Dictionary<string, object> Details { get; set; } = new();
    }
    
    /// <summary>
    /// Estatísticas do gerenciador de plugins
    /// </summary>
    public class PluginManagerStats
    {
        public int TotalPlugins { get; set; }
        public int LoadedPlugins { get; set; }
        public int EnabledPlugins { get; set; }
        public int FailedPlugins { get; set; }
        public Dictionary<string, int> PluginTypes { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de plugin carregado
    /// </summary>
    public class PluginLoadedEventArgs : EventArgs
    {
        public string PluginName { get; set; } = string.Empty;
        public PluginInfo PluginInfo { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de plugin descarregado
    /// </summary>
    public class PluginUnloadedEventArgs : EventArgs
    {
        public string PluginName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
