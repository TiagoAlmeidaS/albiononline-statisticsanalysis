using System;
using System.Collections.Generic;
using System.Drawing;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para contexto de jogo
    /// </summary>
    public interface IGameContext
    {
        /// <summary>
        /// Estado atual do jogo
        /// </summary>
        string CurrentState { get; set; }
        
        /// <summary>
        /// Posição do jogador
        /// </summary>
        Point PlayerPosition { get; set; }
        
        /// <summary>
        /// Vida do jogador
        /// </summary>
        int PlayerHealth { get; set; }
        
        /// <summary>
        /// Energia do jogador
        /// </summary>
        int PlayerEnergy { get; set; }
        
        /// <summary>
        /// Verifica se uma flag está definida
        /// </summary>
        bool HasFlag(string flag);
        
        /// <summary>
        /// Define uma flag
        /// </summary>
        void SetFlag(string flag, bool value);
        
        /// <summary>
        /// Tenta obter um valor do contexto
        /// </summary>
        bool TryGetValue<T>(string key, out T value);
        
        /// <summary>
        /// Define um valor no contexto
        /// </summary>
        void SetValue(string key, object value);
        
        /// <summary>
        /// Obtém todos os valores do contexto
        /// </summary>
        IReadOnlyDictionary<string, object> GetAllValues();
        
        /// <summary>
        /// Obtém todas as flags do contexto
        /// </summary>
        IReadOnlySet<string> GetAllFlags();
        
        /// <summary>
        /// Limpa o contexto
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Clona o contexto
        /// </summary>
        IGameContext Clone();
    }
}
