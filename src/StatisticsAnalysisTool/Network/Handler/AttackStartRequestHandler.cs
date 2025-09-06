using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AttackStartRequestHandler : RequestPacketHandler<AttackStartOperation>
{
    private readonly TrackingController _trackingController;

    public AttackStartRequestHandler(TrackingController trackingController) : base((int) OperationCodes.AttackStart)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AttackStartOperation value)
    {
        // Analisar início de ataque
        _trackingController.AnalyzeAttackStart(value);
        await Task.CompletedTask;
    }
}
