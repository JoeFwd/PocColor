using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig;
using PocColor.Infrastructure.LegacyConfig.Mapping;
using PocColor.Infrastructure.LegacyConfig.Model;
using Xunit;

namespace PocColor.Infrastructure.Tests.LegacyConfig;

public sealed class CampaignBannerPolicyMapperTests
{
    // ----------------------------------------------------------------
    // Map(PocColorModConfigColorEntry) — default scope
    // ----------------------------------------------------------------

    [Fact]
    public void Map_entry_returns_empty_policy_for_null_entry()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        CampaignBannerPolicyOverride result = Mapper().Map(
            (PocColorModConfigColorEntry)null, ScopeKey.Default, diagnostics);

        Assert.Null(result.FollowKingdomColors);
        Assert.Null(result.FollowKingdomBackgroundOnly);
        Assert.Null(result.ConfiguredBannerCode);
    }

    [Fact]
    public void Map_entry_maps_follow_kingdom_colors_booleans()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var entry = new PocColorModConfigColorEntry
        {
            FollowKingdomColors = "true",
            FollowKingdomBackgroundOnly = "false"
        };

        CampaignBannerPolicyOverride result = Mapper().Map(entry, ScopeKey.Default, diagnostics);

        Assert.True(result.FollowKingdomColors);
        Assert.False(result.FollowKingdomBackgroundOnly);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Map_entry_adds_diagnostic_for_invalid_boolean()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var entry = new PocColorModConfigColorEntry { FollowKingdomColors = "not-a-boolean" };

        Mapper().Map(entry, ScopeKey.Default, diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidBoolean", diagnostics[0].Code);
    }

    // ----------------------------------------------------------------
    // Map(PocColorModConfigKingdom)
    // ----------------------------------------------------------------

    [Fact]
    public void Map_kingdom_maps_kingdom_banner_and_colors()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var kingdom = new PocColorModConfigKingdom
        {
            FollowKingdomColors = "true",
            FollowKingdomBackgroundOnly = "false",
            clanBanner = "clan-banner-code",
            kingdomBanner = "kingdom-banner-code",
            primaryColor = "#102030",
            secondaryColor = "#405060"
        };
        var scope = ScopeKey.ForKingdom("Vlandia");

        CampaignBannerPolicyOverride result = Mapper().Map(kingdom, scope, diagnostics);

        Assert.Empty(diagnostics);
        Assert.True(result.FollowKingdomColors);
        Assert.False(result.FollowKingdomBackgroundOnly);
        Assert.Equal("clan-banner-code", result.ConfiguredBannerCode);
        Assert.Equal("kingdom-banner-code", result.KingdomBannerCode);
        Assert.NotNull(result.KingdomPrimaryColor);
        Assert.Equal(0xFF102030u, result.KingdomPrimaryColor.Argb);
        Assert.NotNull(result.KingdomSecondaryColor);
        Assert.Equal(0xFF405060u, result.KingdomSecondaryColor.Argb);
    }

    [Fact]
    public void Map_kingdom_adds_diagnostic_for_invalid_color()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var kingdom = new PocColorModConfigKingdom { primaryColor = "not-a-color" };

        Mapper().Map(kingdom, ScopeKey.ForKingdom("Vlandia"), diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidColor", diagnostics[0].Code);
    }

    // ----------------------------------------------------------------
    // Map(PocColorModConfigClan)
    // ----------------------------------------------------------------

    [Fact]
    public void Map_clan_maps_clan_banner_and_colors()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var clan = new PocColorModConfigClan
        {
            FollowKingdomColors = "false",
            FollowKingdomBackgroundOnly = "true",
            clanBanner = "my-clan-banner",
            primaryColor = "#AABBCC",
            secondaryColor = "#DDEEFF"
        };
        var scope = ScopeKey.ForClan("dey_tihr");

        CampaignBannerPolicyOverride result = Mapper().Map(clan, scope, diagnostics);

        Assert.Empty(diagnostics);
        Assert.False(result.FollowKingdomColors);
        Assert.True(result.FollowKingdomBackgroundOnly);
        Assert.Equal("my-clan-banner", result.ConfiguredBannerCode);
        Assert.NotNull(result.ConfiguredPrimaryColor);
        Assert.Equal(0xFFAABBCCu, result.ConfiguredPrimaryColor.Argb);
        Assert.NotNull(result.ConfiguredSecondaryColor);
        Assert.Equal(0xFFDDEEFFu, result.ConfiguredSecondaryColor.Argb);
    }

    [Fact]
    public void Map_clan_adds_diagnostic_for_invalid_secondary_color()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var clan = new PocColorModConfigClan { secondaryColor = "bad-color" };

        Mapper().Map(clan, ScopeKey.ForClan("dey_tihr"), diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidColor", diagnostics[0].Code);
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static CampaignBannerPolicyMapper Mapper() => new CampaignBannerPolicyMapper();
}
