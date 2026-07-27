using System;
using System.IO;
using Harmony.DependencyInjection;
using Harmony.DependencyInjection.Patches;
using Microsoft.Extensions.DependencyInjection;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Port;
using PocColor.Domain.Appearance.Resolution;
using PocColor.Infrastructure.DI;
using PocColor.Infrastructure.LegacyConfig.Model;
using PocColor.Integration.Bannerlord;
using PocColor.Integration.HarmonyPatches;
using PocColor.Integration.Lifecycle;
using PocColor.Integration.Runtime;

namespace PocColor.Integration.DI;

public sealed class PocColorServiceContainer
{
    public IServiceProvider Build()
    {
        var services = new ServiceCollection();
        string moduleDirectory = GetModuleDirectory();

        services.AddPocColorInfrastructure(moduleDirectory);
        RegisterDomainServices(services);
        RegisterIntegrationServices(services, moduleDirectory);
        RegisterPatches(services);
        services.AddLogging();
        services.AddHarmonyPatching();

        return services.BuildServiceProvider();
    }

    private static void RegisterDomainServices(IServiceCollection services)
    {
        services.AddSingleton<IChoiceSource, RandomChoiceSource>();
        services.AddSingleton<IResolveScopedRuleUtil, ResolveScopedRuleUtil>();
        services.AddSingleton<IResolveColorsUtil, ResolveColorsUtil>();
        services.AddSingleton<IResolveAppearanceUseCase, ResolveAppearanceUseCase>();
        services.AddSingleton<IPlanCampaignBannerUseCase, PlanCampaignBannerUseCase>();
    }

    private static void RegisterIntegrationServices(IServiceCollection services, string moduleDirectory)
    {
        services.AddSingleton<BannerCodePaletteMigrator>();
        services.AddSingleton<BannerSnapshotFactory>();
        services.AddSingleton<ICampaignStateWriter, ReflectionCampaignStateWriter>();
        services.AddSingleton<CampaignSnapshotFactory>();
        services.AddSingleton<UnitProfileFactory>(provider =>
        {
            CampaignSnapshotFactory campaigns = provider.GetRequiredService<CampaignSnapshotFactory>();
            return new UnitProfileFactory(campaigns.KingdomsByBanner, campaigns.ClansByBanner);
        });
        services.AddSingleton<BannerPalette>();
        services.AddSingleton<BannerlordAppearanceApplier>();
        services.AddSingleton<PocColorRuntimeBridge>();
        services.AddSingleton<PocColorCampaignBehavior>();
        services.AddSingleton(provider => new PocColorCampaignTemplateWriter(
            provider.GetRequiredService<PocColorModConfig>(),
            moduleDirectory));
    }

    private static void RegisterPatches(IServiceCollection services)
    {
        services.AddSingleton<IPatch, AgentBuildDataBannerPatch>();
        services.AddSingleton<IPatch, AgentBuildDataClothingColor1Patch>();
        services.AddSingleton<IPatch, AgentBuildDataClothingColor2Patch>();
        services.AddSingleton<IPatch, ClanBannerColorPatch>();
    }

    private static string GetModuleDirectory()
    {
        string assemblyDirectory = Path.GetDirectoryName(typeof(PocColorSubModule).Assembly.Location);
        return Path.GetFullPath(Path.Combine(assemblyDirectory, "..", ".."));
    }

    private sealed class RandomChoiceSource : IChoiceSource
    {
        private readonly Random _random = CreateRandom();

        private static Random CreateRandom()
        {
            return int.TryParse(Environment.GetEnvironmentVariable("POC_COLOR_REGRESSION_SEED"), out int seed)
                ? new Random(seed)
                : new Random();
        }

        public int NextIndex(int exclusiveUpperBound)
        {
            return exclusiveUpperBound <= 0 ? 0 : _random.Next(exclusiveUpperBound);
        }
    }
}
