using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public sealed class CampaignAppearanceSynchronizer
{
    private readonly BannerSerializer _bannerSerializer;
    private readonly CampaignBannerRegistry _bannerRegistry;
    private readonly AppearanceRuleSet _rules;
    private readonly IPlanCampaignBannerUseCase _planner;
    private readonly ICampaignStateWriter _stateWriter;
    private readonly Dictionary<Clan, CampaignBannerChange> _clanChanges = new Dictionary<Clan, CampaignBannerChange>();
    private readonly Dictionary<Kingdom, KingdomColors> _kingdomColors = new Dictionary<Kingdom, KingdomColors>();

    public CampaignAppearanceSynchronizer(
        AppearanceRuleSet rules,
        ICampaignStateWriter stateWriter,
        BannerSerializer bannerSerializer,
        CampaignBannerRegistry bannerRegistry,
        IPlanCampaignBannerUseCase planner = null)
    {
        _rules = rules ?? new AppearanceRuleSet(null);
        _stateWriter = stateWriter ?? throw new ArgumentNullException(nameof(stateWriter));
        _bannerSerializer = bannerSerializer ?? throw new ArgumentNullException(nameof(bannerSerializer));
        _bannerRegistry = bannerRegistry ?? throw new ArgumentNullException(nameof(bannerRegistry));
        _planner = planner ?? new PlanCampaignBannerUseCase();
    }

    public void Synchronize()
    {
        _clanChanges.Clear();
        _kingdomColors.Clear();
        _bannerRegistry.Clear();

        Campaign campaign = Campaign.Current;
        if (campaign == null)
        {
            return;
        }

        foreach (Kingdom kingdom in campaign.Kingdoms)
        {
            ApplyKingdom(kingdom);
            foreach (Clan clan in kingdom.Clans)
            {
                ApplyClan(kingdom, clan);
            }
        }

        foreach (Clan clan in campaign.Clans)
        {
            if (clan.Kingdom == null)
            {
                ApplyClan(null, clan);
            }
        }
    }

    public string CaptureClanBannerBeforeKingdomUpdate(Clan clan)
    {
        return _bannerSerializer.Serialize(clan?.Banner);
    }

    public void RestoreClanBannerAfterKingdomUpdate(Clan clan, string bannerCodeBeforeKingdomUpdate)
    {
        if (clan == null
            || clan.Kingdom?.RulingClan == clan)
        {
            return;
        }

        if (!_clanChanges.TryGetValue(clan, out CampaignBannerChange change))
        {
            change = _planner.Plan(GetClanBannerPolicy(
                clan.Kingdom,
                clan,
                bannerCodeBeforeKingdomUpdate));
            _clanChanges[clan] = change;
        }

        string bannerCodeToRestore = change.ResolveBannerCodeToRestore(bannerCodeBeforeKingdomUpdate);
        if (!string.IsNullOrWhiteSpace(bannerCodeToRestore))
        {
            _stateWriter.SetClanBanner(clan, bannerCodeToRestore);
            return;
        }

        if (!string.IsNullOrWhiteSpace(change.ConfiguredBannerCode))
        {
            _stateWriter.SetClanBanner(clan, change.ConfiguredBannerCode);
        }

        ApplyClanColors(clan.Banner, change, GetKingdomColors(clan.Kingdom));
    }

    private void ApplyKingdom(Kingdom kingdom)
    {
        string kingdomName = kingdom.Name?.ToString();
        _bannerRegistry.Register(
            _bannerSerializer.Serialize(kingdom.Banner),
            kingdomName,
            kingdom.RulingClan?.Name?.ToString());

        bool isPlayerKingdom = Clan.PlayerClan?.Kingdom == kingdom;
        CampaignKingdomPolicy policy = _rules.GetCampaignKingdomPolicy(kingdomName, isPlayerKingdom);
        uint? primaryColor = BannerlordAppearanceApplier.Resolve(policy.PrimaryColor);
        uint? secondaryColor = BannerlordAppearanceApplier.Resolve(policy.SecondaryColor);

        if (!string.IsNullOrWhiteSpace(policy.BannerCode))
        {
            _stateWriter.SetKingdomBanner(kingdom, policy.BannerCode);
        }

        if (primaryColor.HasValue)
        {
            _stateWriter.SetKingdomPrimaryColor(kingdom, primaryColor.Value);
            kingdom.Banner?.ChangePrimaryColor(primaryColor.Value);
        }

        if (secondaryColor.HasValue)
        {
            _stateWriter.SetKingdomSecondaryColor(kingdom, secondaryColor.Value);
            kingdom.Banner?.ChangeIconColors(secondaryColor.Value);
        }

        _kingdomColors[kingdom] = new KingdomColors(
            primaryColor ?? kingdom.PrimaryBannerColor,
            secondaryColor ?? kingdom.SecondaryBannerColor);
        _bannerRegistry.Register(
            _bannerSerializer.Serialize(kingdom.Banner),
            kingdomName,
            kingdom.RulingClan?.Name?.ToString());
    }

    private void ApplyClan(Kingdom kingdom, Clan clan)
    {
        Kingdom resolvedKingdom = kingdom ?? clan.Kingdom;
        CampaignBannerPolicy policy = GetClanBannerPolicy(
            resolvedKingdom,
            clan,
            _bannerSerializer.Serialize(clan.Banner));
        CampaignBannerChange change = _planner.Plan(policy);

        uint? configuredPrimaryColor = BannerlordAppearanceApplier.Resolve(change.ConfiguredPrimaryColor);
        uint? configuredSecondaryColor = BannerlordAppearanceApplier.Resolve(change.ConfiguredSecondaryColor);
        if (configuredPrimaryColor.HasValue)
        {
            clan.Color = configuredPrimaryColor.Value;
        }

        if (configuredSecondaryColor.HasValue)
        {
            clan.Color2 = configuredSecondaryColor.Value;
        }

        if (!string.IsNullOrWhiteSpace(change.ConfiguredBannerCode))
        {
            _stateWriter.SetClanBanner(clan, change.ConfiguredBannerCode);
        }

        ApplyClanColors(clan.Banner, change, GetKingdomColors(resolvedKingdom));
        _clanChanges[clan] = _planner.Plan(new CampaignBannerPolicy(
            policy.FollowKingdomColors,
            policy.FollowKingdomBackgroundOnly,
            policy.ConfiguredBannerCode,
            _bannerSerializer.Serialize(clan.Banner),
            policy.ConfiguredPrimaryColor,
            policy.ConfiguredSecondaryColor));
        _bannerRegistry.Register(
            _bannerSerializer.Serialize(clan.Banner),
            resolvedKingdom?.Name?.ToString(),
            clan.Name?.ToString());
    }

    private CampaignBannerPolicy GetClanBannerPolicy(Kingdom kingdom, Clan clan, string cachedBannerCode)
    {
        string kingdomName = kingdom?.Name?.ToString();
        string clanName = clan.Name?.ToString();
        Clan playerClan = Campaign.Current == null ? null : Clan.PlayerClan;
        return _rules.GetCampaignBannerPolicy(
            kingdomName,
            clanName,
            playerClan?.Kingdom == kingdom,
            playerClan == clan,
            cachedBannerCode);
    }

    private static void ApplyClanColors(Banner banner, CampaignBannerChange change, KingdomColors kingdomColors)
    {
        if (banner == null)
        {
            return;
        }

        uint? primaryColor = change.ApplyKingdomPrimaryColor
            ? kingdomColors?.PrimaryColor
            : BannerlordAppearanceApplier.Resolve(change.ConfiguredPrimaryColor);
        uint? secondaryColor = change.ApplyKingdomSecondaryColor
            ? kingdomColors?.SecondaryColor
            : BannerlordAppearanceApplier.Resolve(change.ConfiguredSecondaryColor);

        if (primaryColor.HasValue)
        {
            banner.ChangePrimaryColor(primaryColor.Value);
        }

        if (secondaryColor.HasValue)
        {
            banner.ChangeIconColors(secondaryColor.Value);
        }
    }

    private KingdomColors GetKingdomColors(Kingdom kingdom)
    {
        if (kingdom == null)
        {
            return null;
        }

        return _kingdomColors.TryGetValue(kingdom, out KingdomColors colors)
            ? colors
            : new KingdomColors(kingdom.PrimaryBannerColor, kingdom.SecondaryBannerColor);
    }

    private sealed class KingdomColors
    {
        public KingdomColors(uint primaryColor, uint secondaryColor)
        {
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }

        public uint PrimaryColor { get; }

        public uint SecondaryColor { get; }
    }
}
