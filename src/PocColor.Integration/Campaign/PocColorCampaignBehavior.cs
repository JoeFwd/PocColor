using PocColor.Integration.Runtime;
using TaleWorlds.CampaignSystem;

namespace PocColor.Integration.Lifecycle;

public sealed class PocColorCampaignBehavior : CampaignBehaviorBase
{
    private readonly PocColorRuntimeBridge _bridge;

    public PocColorCampaignBehavior(PocColorRuntimeBridge bridge)
    {
        _bridge = bridge;
    }

    public override void RegisterEvents()
    {
        CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, ApplyCampaign);
        CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, ApplyCampaign);
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    private void ApplyCampaign(CampaignGameStarter _)
    {
        _bridge.ApplyCampaign();
    }
}
