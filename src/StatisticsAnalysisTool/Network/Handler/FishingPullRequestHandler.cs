using StatisticsAnalysisTool.Network.Operations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingPullRequestHandler : RequestPacketHandler<FishingPullRequest>
{
    private readonly TrackingController _trackingController;

    public FishingPullRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingPull)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingPullRequest value)
    {
        // Log da operação de pull de pesca
        System.Diagnostics.Debug.WriteLine($"FishingPullRequest: EventId={value.EventId}, ItemIndex={value.ItemIndex}");
        
        // Aqui você pode adicionar lógica específica para o pull de pesca
        // Por exemplo, notificar o GatheringController sobre o pull
        
        await Task.CompletedTask;
    }
}
