using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewMobEventHandler : EventPacketHandler<NewMobEvent>
{
    private readonly TrackingController _trackingController;

    public NewMobEventHandler(TrackingController trackingController) : base((int) EventCodes.NewMob)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewMobEvent value)
    {
        // Adicionar mob ao dungeon
        await _trackingController.DungeonController.AddTierToCurrentDungeonAsync(value.MobIndex);
        _trackingController.DungeonController.AddLevelToCurrentDungeon(value.MobIndex, value.HitPointsMax);

        // Analisar posição do mob se disponível
        if (value.Position.Length >= 2)
        {
            _trackingController.AnalyzeMobPosition(value.ObjectId, value.Position, value.NewPosition);
        }
    }
}