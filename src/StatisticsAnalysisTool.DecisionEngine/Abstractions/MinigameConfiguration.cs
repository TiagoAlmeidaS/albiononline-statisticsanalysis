using System;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Configuração para o minigame
    /// </summary>
    public class MinigameConfiguration
    {
        /// <summary>
        /// Duração máxima do minigame
        /// </summary>
        public TimeSpan MaxDuration { get; set; } = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Sensibilidade da detecção
        /// </summary>
        public float Sensitivity { get; set; } = 1.0f;
        
        /// <summary>
        /// Habilita feedback visual
        /// </summary>
        public bool EnableVisualFeedback { get; set; } = true;
        
        /// <summary>
        /// Habilita feedback sonoro
        /// </summary>
        public bool EnableAudioFeedback { get; set; } = false;
        
        /// <summary>
        /// Configurações específicas do minigame
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }
}
