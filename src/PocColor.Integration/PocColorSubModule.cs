using System;
using Harmony.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PocColor.Integration.DI;
using PocColor.Integration.Lifecycle;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PocColor;

public sealed class PocColorSubModule : MBSubModuleBase
{
    private IServiceProvider _serviceProvider;

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();
        _serviceProvider = new PocColorServiceContainer().Build();
        _serviceProvider.GetRequiredService<IHarmonyPatcher>().ApplyPatches();
    }

    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        InformationManager.DisplayMessage(new InformationMessage("POC color mod [2.0.1]: loaded!"));
    }

    protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
    {
        base.InitializeGameStarter(game, starterObject);

        if (starterObject is CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddBehavior(_serviceProvider.GetRequiredService<PocColorCampaignBehavior>());
        }
    }

    public override void OnGameInitializationFinished(Game game)
    {
        base.OnGameInitializationFinished(game);
        _serviceProvider.GetRequiredService<PocColorCampaignTemplateWriter>().WriteWhenRequested();
    }
}
