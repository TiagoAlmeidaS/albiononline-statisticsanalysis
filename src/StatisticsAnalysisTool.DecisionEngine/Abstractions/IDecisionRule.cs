using System;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para regras de decisão
    /// </summary>
    public interface IDecisionRule
    {
        /// <summary>
        /// Nome da regra
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Descrição da regra
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Prioridade da regra (maior = mais prioritário)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Se a regra está habilitada
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Avalia a regra e retorna uma decisão
        /// </summary>
        DecisionResult Evaluate(IGameContext context);
    }
}
