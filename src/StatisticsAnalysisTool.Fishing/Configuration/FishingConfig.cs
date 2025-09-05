using System;

namespace StatisticsAnalysisTool.Fishing
{
    /// <summary>
    /// Configuração do sistema de pesca
    /// </summary>
    public class FishingConfig
    {
        /// <summary>
        /// Habilita detecção automática de zonas de pesca
        /// </summary>
        public bool EnableAutoZoneDetection { get; set; } = true;
        
        /// <summary>
        /// Habilita detecção automática de bobber
        /// </summary>
        public bool EnableAutoBobberDetection { get; set; } = true;
        
        /// <summary>
        /// Habilita resolução automática de minigame
        /// </summary>
        public bool EnableAutoMinigameResolution { get; set; } = true;
        
        /// <summary>
        /// Intervalo de verificação de zonas de pesca (em ms)
        /// </summary>
        public int ZoneCheckInterval { get; set; } = 1000;
        
        /// <summary>
        /// Intervalo de verificação de bobber (em ms)
        /// </summary>
        public int BobberCheckInterval { get; set; } = 100;
        
        /// <summary>
        /// Timeout para resolução de minigame (em ms)
        /// </summary>
        public int MinigameTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Sensibilidade da detecção de bobber (0.0 - 1.0)
        /// </summary>
        public float BobberDetectionSensitivity { get; set; } = 0.8f;
        
        /// <summary>
        /// Habilita logging detalhado
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;
        
        /// <summary>
        /// Configurações específicas de zonas de pesca
        /// </summary>
        public FishingZoneConfig ZoneConfig { get; set; } = new();
        
        /// <summary>
        /// Configurações específicas de bobber
        /// </summary>
        public BobberConfig BobberConfig { get; set; } = new();
        
        /// <summary>
        /// Configurações específicas de minigame
        /// </summary>
        public MinigameConfig MinigameConfig { get; set; } = new();
    }
    
    /// <summary>
    /// Configuração de zonas de pesca
    /// </summary>
    public class FishingZoneConfig
    {
        /// <summary>
        /// Raio máximo para detecção de zonas (em metros)
        /// </summary>
        public float MaxDetectionRadius { get; set; } = 50.0f;
        
        /// <summary>
        /// Habilita filtro por tipo de zona
        /// </summary>
        public bool EnableZoneTypeFilter { get; set; } = true;
        
        /// <summary>
        /// Tipos de zona permitidos
        /// </summary>
        public string[] AllowedZoneTypes { get; set; } = { "FISHING_ZONE", "WATER" };
    }
    
    /// <summary>
    /// Configuração de bobber
    /// </summary>
    public class BobberConfig
    {
        /// <summary>
        /// Tamanho mínimo do bobber para detecção (em pixels)
        /// </summary>
        public int MinBobberSize { get; set; } = 10;
        
        /// <summary>
        /// Tamanho máximo do bobber para detecção (em pixels)
        /// </summary>
        public int MaxBobberSize { get; set; } = 100;
        
        /// <summary>
        /// Habilita detecção de movimento do bobber
        /// </summary>
        public bool EnableMovementDetection { get; set; } = true;
        
        /// <summary>
        /// Threshold de movimento para considerar bobber ativo
        /// </summary>
        public float MovementThreshold { get; set; } = 5.0f;
    }
    
    /// <summary>
    /// Configuração de minigame
    /// </summary>
    public class MinigameConfig
    {
        /// <summary>
        /// Habilita resolução automática
        /// </summary>
        public bool EnableAutoResolution { get; set; } = true;
        
        /// <summary>
        /// Delay antes de iniciar resolução (em ms)
        /// </summary>
        public int StartDelay { get; set; } = 500;
        
        /// <summary>
        /// Habilita feedback visual
        /// </summary>
        public bool EnableVisualFeedback { get; set; } = true;
        
        /// <summary>
        /// Habilita feedback sonoro
        /// </summary>
        public bool EnableAudioFeedback { get; set; } = false;
    }
}
