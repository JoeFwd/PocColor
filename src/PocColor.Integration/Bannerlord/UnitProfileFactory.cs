using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public sealed class UnitProfileFactory
{
    private readonly BannerSerializer _bannerSerializer;
    private readonly IReadOnlyDictionary<string, string> _kingdomsByBanner;
    private readonly IReadOnlyDictionary<string, string> _clansByBanner;

    public UnitProfileFactory(
        BannerSerializer bannerSerializer,
        IReadOnlyDictionary<string, string> kingdomsByBanner = null,
        IReadOnlyDictionary<string, string> clansByBanner = null)
    {
        _bannerSerializer = bannerSerializer ?? throw new ArgumentNullException(nameof(bannerSerializer));
        _kingdomsByBanner = kingdomsByBanner;
        _clansByBanner = clansByBanner;
    }

    public UnitProfile Create(BasicCharacterObject character, Banner banner)
    {
        string unitName = character?.Name?.ToString() ?? string.Empty;
        CharacterObject campaignCharacter = character as CharacterObject ?? FindCharacter(unitName);
        Clan clan = campaignCharacter?.HeroObject?.Clan;
        Kingdom kingdom = clan?.Kingdom;
        string bannerCode = _bannerSerializer.Serialize(banner);

        string cachedClanName = GetCached(_clansByBanner, bannerCode);
        string cachedKingdomName = GetCached(_kingdomsByBanner, bannerCode);
        string clanName = clan?.Name?.ToString() ?? cachedClanName ?? string.Empty;
        string kingdomName = kingdom?.Name?.ToString() ?? cachedKingdomName ?? string.Empty;
        Clan resolvedClan = clan ?? FindClan(clanName);
        Kingdom resolvedKingdom = kingdom ?? resolvedClan?.Kingdom ?? FindKingdom(kingdomName);
        Clan playerClan = GetPlayerClan();
        Kingdom rulerKingdom = resolvedKingdom ?? resolvedClan?.Kingdom;

        return new UnitProfile(
            resolvedKingdom?.Name?.ToString() ?? kingdomName,
            resolvedClan?.Name?.ToString() ?? clanName,
            unitName,
            playerClan?.Kingdom != null && rulerKingdom == playerClan.Kingdom,
            playerClan != null && resolvedClan == playerClan,
            GetMainHeroName() == unitName,
            rulerKingdom?.Leader?.Name?.ToString() == unitName,
            resolvedClan?.Leader?.Name?.ToString() == unitName,
            character?.IsHero ?? false,
            character?.IsMounted ?? false,
            character?.IsRanged ?? false,
            campaignCharacter?.Tier ?? 0,
            character?.Culture?.Name?.ToString(),
            _bannerSerializer.Serialize(rulerKingdom?.RulingClan?.Banner));
    }

    private static string GetCached(IReadOnlyDictionary<string, string> cache, string bannerCode)
    {
        return cache != null
            && !string.IsNullOrEmpty(bannerCode)
            && cache.TryGetValue(bannerCode, out string value)
            ? value
            : null;
    }

    private static CharacterObject FindCharacter(string name)
    {
        return string.IsNullOrEmpty(name)
            ? null
            : CharacterObject.FindFirst(character => character.Name?.ToString() == name);
    }

    private static Clan GetPlayerClan()
    {
        try
        {
            return Clan.PlayerClan;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private static string GetMainHeroName()
    {
        try
        {
            return Hero.MainHero?.Name?.ToString();
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private static Clan FindClan(string name)
    {
        return string.IsNullOrEmpty(name) || Campaign.Current == null
            ? null
            : Clan.FindFirst(clan => clan.Name?.ToString() == name);
    }

    private static Kingdom FindKingdom(string name)
    {
        if (string.IsNullOrEmpty(name) || Campaign.Current == null)
        {
            return null;
        }

        foreach (Kingdom kingdom in Campaign.Current.Kingdoms)
        {
            if (kingdom.Name?.ToString() == name)
            {
                return kingdom;
            }
        }

        return null;
    }
}
