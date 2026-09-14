using System;
using System.Runtime.CompilerServices;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;
using PocColor.Integration.Bannerlord;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PocColor.Integration.Runtime;

public sealed class PocColorRuntimeBridge
{
    private readonly AppearanceRuleSet _rules;
    private readonly IChoiceSource _choices;
    private readonly IResolveAppearanceUseCase _resolver;
    private readonly UnitProfileFactory _profiles;
    private readonly BannerPalette _palette;
    private readonly BannerlordAppearanceApplier _applier;
    private readonly CampaignSnapshotFactory _campaigns;
    private readonly ConditionalWeakTable<AgentBuildData, BuildAppearance> _buildAppearances = new();
    private readonly BannerSnapshotMapper _bannerSnapshotMapper;

    private BuildAppearance _lastBuildAppearance;

    public PocColorRuntimeBridge(
        AppearanceRuleSet rules,
        IChoiceSource choices,
        BannerSnapshotMapper bannerSnapshotMapper,
        UnitProfileFactory profiles,
        BannerPalette palette = null,
        BannerlordAppearanceApplier applier = null,
        CampaignSnapshotFactory campaigns = null,
        IResolveAppearanceUseCase resolver = null)
    {
        _rules = rules ?? new AppearanceRuleSet(null);
        _choices = choices ?? throw new ArgumentNullException(nameof(choices));
        _bannerSnapshotMapper = bannerSnapshotMapper ?? throw new ArgumentNullException(nameof(bannerSnapshotMapper));
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        _resolver = resolver ?? new ResolveAppearanceUseCase();
        _palette = palette ?? new BannerPalette(_bannerSnapshotMapper);
        _applier = applier ?? new BannerlordAppearanceApplier();
        _campaigns = campaigns;
    }

    public ResolvedAppearance LastResolvedAppearance { get; private set; }

    public uint? LastAppliedColor1 { get; private set; }

    public uint? LastAppliedColor2 { get; private set; }

    public void BeginAgentBuild(AgentBuildData builder, Banner banner)
    {
        if (builder == null)
        {
            return;
        }

        ResolvedAppearance appearance = Resolve(GetCharacter(builder), banner, ResolutionKind.Battle);
        var buildAppearance = new BuildAppearance(appearance);
        _buildAppearances.Remove(builder);
        _buildAppearances.Add(builder, buildAppearance);
        _lastBuildAppearance = buildAppearance;
        LastResolvedAppearance = appearance;
        ColorApplicationDiagnostics.WriteResolved(GetCharacter(builder), appearance);
    }

    public uint? ApplyClothingColor1(AgentBuildData builder)
    {
        BuildAppearance appearance = FindBuildAppearance(builder);
        uint? color = _applier.ApplyPrimary(appearance?.Appearance);
        LastAppliedColor1 = color;
        ColorApplicationDiagnostics.WriteApplied(GetCharacter(builder), "ClothingColor1", color);
        return color;
    }

    public uint? ApplyClothingColor2(AgentBuildData builder)
    {
        BuildAppearance appearance = FindBuildAppearance(builder);
        uint? color = _applier.ApplySecondary(appearance?.Appearance);
        LastAppliedColor2 = color;
        ColorApplicationDiagnostics.WriteApplied(GetCharacter(builder), "ClothingColor2", color);
        return color;
    }

    public uint? ApplyClothingColor1()
    {
        uint? color = _applier.ApplyPrimary(_lastBuildAppearance?.Appearance);
        LastAppliedColor1 = color;
        return color;
    }

    public uint? ApplyClothingColor2()
    {
        uint? color = _applier.ApplySecondary(_lastBuildAppearance?.Appearance);
        LastAppliedColor2 = color;
        return color;
    }

    public ResolvedAppearance ResolveForMap(BasicCharacterObject character, Banner banner)
    {
        return Resolve(character, banner, ResolutionKind.Normal);
    }

    public ResolvedAppearance ResolveForUi(BasicCharacterObject character, Banner banner)
    {
        return Resolve(character, banner, ResolutionKind.Normal);
    }

    public ResolvedAppearance ResolveForProfile(
        UnitProfile profile,
        Banner banner,
        ResolutionKind resolutionKind)
    {
        return Resolve(profile, banner, resolutionKind);
    }

    public void ReapplyClanBanner(Clan clan)
    {
        _campaigns?.ReapplyClanBanner(clan);
    }

    public void ApplyCampaign()
    {
        _campaigns?.Apply();
    }

    private ResolvedAppearance Resolve(BasicCharacterObject character, Banner banner, ResolutionKind kind)
    {
        return Resolve(_profiles.Create(character, banner), banner, kind);
    }

    private ResolvedAppearance Resolve(UnitProfile profile, Banner banner, ResolutionKind kind)
    {
        BannerSnapshot currentBanner = _bannerSnapshotMapper.CreateSnapshot(banner);
        return _resolver.Resolve(
            _rules,
            profile,
            currentBanner,
            kind,
            _choices,
            _palette.Create());
    }

    private BuildAppearance FindBuildAppearance(AgentBuildData builder)
    {
        if (builder != null && _buildAppearances.TryGetValue(builder, out BuildAppearance appearance))
        {
            return appearance;
        }

        return null;
    }

    private static BasicCharacterObject GetCharacter(AgentBuildData builder)
    {
        try
        {
            return builder.AgentCharacter;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private sealed class BuildAppearance
    {
        public BuildAppearance(ResolvedAppearance appearance)
        {
            Appearance = appearance;
        }

        public ResolvedAppearance Appearance { get; }
    }

}
