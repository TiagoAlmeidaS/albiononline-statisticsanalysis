using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface simplificada para comportamentos do DecisionEngine
    /// </summary>
    public interface ISimpleBehavior
    {
        string Name { get; }
        string Description { get; }
        int Priority { get; set; }
        bool IsEnabled { get; set; }
        bool IsActive { get; }
        
        Task InitializeAsync();
        Task ShutdownAsync();
        Task<bool> CanProcessAsync(IGameContext context);
        Task<DecisionResult> MakeDecisionAsync(IGameContext context);
        Task<BehaviorResult> ExecuteActionAsync(DecisionResult decision);
        Task UpdateAsync(IGameContext context);
        BehaviorStats GetStats();
        IEnumerable<string> GetSupportedActions();
        IEnumerable<string> GetSupportedConditions();
    }
}
