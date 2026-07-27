using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig;
using PocColor.Infrastructure.LegacyConfig.Mapping;
using PocColor.Infrastructure.LegacyConfig.Model;
using Xunit;

namespace PocColor.Infrastructure.Tests.LegacyConfig;

public sealed class RuleOverrideMapperTests
{
    // ----------------------------------------------------------------
    // MapDefault (was CompileDefault)
    // ----------------------------------------------------------------

    [Fact]
    public void MapDefault_uses_mode_2_when_entry_is_null()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        RuleOverride result = Mapper().MapDefault(null, diagnostics);

        Assert.Equal(ColorStrategy.BannerFirstTwo, result.Strategy);
    }

    [Fact]
    public void MapDefault_uses_mode_2_when_entry_mode_is_null()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var entry = new PocColorModConfigColorEntry { mode = null };
        RuleOverride result = Mapper().MapDefault(entry, diagnostics);

        Assert.Equal(ColorStrategy.BannerFirstTwo, result.Strategy);
    }

    [Fact]
    public void MapDefault_maps_color_and_color2_fields()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var entry = new PocColorModConfigColorEntry
        {
            mode = 3,
            color = new[] { "#102030", "#405060" },
            color2 = new[] { "#708090" }
        };

        RuleOverride result = Mapper().MapDefault(entry, diagnostics);

        Assert.Empty(diagnostics);
        Assert.Equal(ColorStrategy.FixedPair, result.Strategy);
        Assert.Equal(0xFF102030u, result.PrimaryColors[0].Argb);
        Assert.Equal(0xFF405060u, result.PrimaryColors[1].Argb);
        Assert.Equal(0xFF708090u, result.SecondaryColors[0].Argb);
    }

    [Fact]
    public void MapDefault_maps_ruler_flags()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var entry = new PocColorModConfigColorEntry
        {
            BearRulerColors = "true",
            BearRulerBanner = "false",
            BearRulerShield = "true"
        };

        RuleOverride result = Mapper().MapDefault(entry, diagnostics);

        Assert.True(result.BearRulerColors);
        Assert.False(result.BearRulerBanner);
        Assert.True(result.BearRulerShield);
    }

    [Fact]
    public void MapDefault_maps_variables()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var entry = new PocColorModConfigColorEntry
        {
            vars = new[] { new Map<string, string[]> { ["$sigil"] = new[] { "wolf" } } }
        };

        RuleOverride result = Mapper().MapDefault(entry, diagnostics);

        Assert.NotNull(result.VariableGroups);
        Assert.Single(result.VariableGroups);
        Assert.Equal("wolf", result.VariableGroups[0]["$sigil"][0]);
    }

    // ----------------------------------------------------------------
    // Map (was CompileSection)
    // ----------------------------------------------------------------

    [Fact]
    public void Map_reads_all_section_fields_from_config_section()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var section = new PocColorModConfigUnit
        {
            mode = 3,
            color = new[] { "#102030", "#405060" },
            color2 = new[] { "#708090" },
            banner = "main-banner",
            banners = new[] { "alt1", "alt2" },
            shields = new[] { "shield1" },
            combatBanners = new[] { "battle-banner" },
            combatShields = new[] { "battle-shield" },
            BearRulerColors = "true",
            BearRulerBanner = "false",
            BearRulerShield = "true",
            rulerMode = 1,
            rulerColor = new[] { "#AABBCC" },
            rulerColor2 = new[] { "#DDEEFF" },
            rulerBanner = "ruler-banner",
            rulerShield = "ruler-shield"
        };

        RuleOverride result = Mapper().Map(section, ScopeKey.Default, diagnostics);

        Assert.Empty(diagnostics);
        Assert.Equal(ColorStrategy.FixedPair, result.Strategy);
        Assert.Equal(0xFF102030u, result.PrimaryColors[0].Argb);
        Assert.Equal(0xFF708090u, result.SecondaryColors[0].Argb);
        Assert.Equal("main-banner", result.BannerCode);
        Assert.Equal(new[] { "alt1", "alt2" }, result.BannerCodes);
        Assert.Equal(new[] { "shield1" }, result.ShieldCodes);
        Assert.Equal(new[] { "battle-banner" }, result.BattleBannerCodes);
        Assert.Equal(new[] { "battle-shield" }, result.BattleShieldCodes);
        Assert.True(result.BearRulerColors);
        Assert.False(result.BearRulerBanner);
        Assert.True(result.BearRulerShield);
        Assert.Equal(ColorStrategy.RandomConfigured, result.RulerStrategy);
        Assert.Equal(0xFFAABBCCu, result.RulerPrimaryColors[0].Argb);
        Assert.Equal(0xFFDDEEFFu, result.RulerSecondaryColors[0].Argb);
        Assert.Equal("ruler-banner", result.RulerBannerCode);
        Assert.Equal("ruler-shield", result.RulerShieldCode);
    }

    [Fact]
    public void Map_returns_null_strategy_for_null_mode()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var section = new PocColorModConfigUnit { mode = null };

        RuleOverride result = Mapper().Map(section, ScopeKey.Default, diagnostics);

        Assert.Null(result.Strategy);
    }

    [Fact]
    public void Map_adds_diagnostic_for_unknown_mode()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var section = new PocColorModConfigUnit { mode = 99 };

        RuleOverride result = Mapper().Map(section, ScopeKey.Default, diagnostics);

        Assert.Equal(ColorStrategy.Unresolved, result.Strategy);
        Assert.Single(diagnostics);
        Assert.Equal("UnknownMode", diagnostics[0].Code);
    }

    [Fact]
    public void Map_adds_diagnostic_for_invalid_color()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var section = new PocColorModConfigUnit { color = new[] { "not-a-color" } };

        RuleOverride result = Mapper().Map(section, ScopeKey.Default, diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidColor", diagnostics[0].Code);
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static RuleOverrideMapper Mapper() => new RuleOverrideMapper();
}
