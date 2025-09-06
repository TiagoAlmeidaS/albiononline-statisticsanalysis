using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class MoveRequestHandler : RequestPacketHandler<MoveOperation>
{
    private readonly TrackingController _trackingController;

    public MoveRequestHandler(TrackingController trackingController) : base((int) OperationCodes.Move)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(MoveOperation value)
    {
        // Analisar dados de movimento
        _trackingController.AnalyzeMovement(value);
        await Task.CompletedTask;
    }
}
