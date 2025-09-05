using System;
using System.Drawing;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para serviço de minigame (mock para DecisionEngine)
    /// </summary>
    public interface IMinigameService
    {
        bool IsMinigameActive { get; }
        MinigameStats CurrentStats { get; }
        
        event EventHandler<MinigameStartedEventArgs>? MinigameStarted;
        event EventHandler<MinigameFinishedEventArgs>? MinigameFinished;
        event EventHandler<BobberDetectedEventArgs>? BobberDetected;
        event EventHandler<MouseStateChangedEventArgs>? MouseStateChanged;
        
        Task StartMinigameAsync(Rectangle region);
        void StopMinigame();
    }
}
