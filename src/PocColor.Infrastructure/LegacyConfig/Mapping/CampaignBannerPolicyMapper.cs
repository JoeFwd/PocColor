using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig.Mapping;

public sealed class CampaignBannerPolicyMapper
{
    private readonly LegacyValueParser _parser;

    public CampaignBannerPolicyMapper() : this(new LegacyValueParser())
    {
    }

    internal CampaignBannerPolicyMapper(LegacyValueParser parser)
    {
        _parser = parser;
    }

    public CampaignBannerPolicyOverride Map(
        PocColorModConfigColorEntry entry,
        ScopeKey scope,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        return MapCore(
            entry == null ? null : entry.FollowKingdomColors,
            entry == null ? null : entry.FollowKingdomBackgroundOnly,
            null,
            null,
            null,
            null,
            null,
            null,
            scope,
            diagnostics);
    }

    public CampaignBannerPolicyOverride Map(
        PocColorModConfigKingdom entry,
        ScopeKey scope,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        return MapCore(
            entry.FollowKingdomColors,
            entry.FollowKingdomBackgroundOnly,
            entry.clanBanner,
            null,
            null,
            entry.kingdomBanner,
            _parser.ParseCampaignColor(entry.primaryColor, scope, "primaryColor", diagnostics),
            _parser.ParseCampaignColor(entry.secondaryColor, scope, "secondaryColor", diagnostics),
            scope,
            diagnostics);
    }

    public CampaignBannerPolicyOverride Map(
        PocColorModConfigClan entry,
        ScopeKey scope,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        return MapCore(
            entry.FollowKingdomColors,
            entry.FollowKingdomBackgroundOnly,
            entry.clanBanner,
            _parser.ParseCampaignColor(entry.primaryColor, scope, "primaryColor", diagnostics),
            _parser.ParseCampaignColor(entry.secondaryColor, scope, "secondaryColor", diagnostics),
            null,
            null,
            null,
            scope,
            diagnostics);
    }

    private CampaignBannerPolicyOverride MapCore(
        string followKingdomColors,
        string followKingdomBackgroundOnly,
        string configuredBannerCode,
        ColorReference configuredPrimaryColor,
        ColorReference configuredSecondaryColor,
        string kingdomBannerCode,
        ColorReference kingdomPrimaryColor,
        ColorReference kingdomSecondaryColor,
        ScopeKey scope,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        return new CampaignBannerPolicyOverride(
            _parser.ParseBoolean(followKingdomColors, scope, "FollowKingdomColors", diagnostics),
            _parser.ParseBoolean(followKingdomBackgroundOnly, scope, "FollowKingdomBackgroundOnly", diagnostics),
            configuredBannerCode,
            configuredPrimaryColor,
            configuredSecondaryColor,
            kingdomBannerCode,
            kingdomPrimaryColor,
            kingdomSecondaryColor);
    }
}
