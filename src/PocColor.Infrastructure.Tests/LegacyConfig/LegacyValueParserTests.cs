using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig;
using PocColor.Infrastructure.LegacyConfig.Mapping;
using PocColor.Infrastructure.LegacyConfig.Model;
using Xunit;

namespace PocColor.Infrastructure.Tests.LegacyConfig;

public sealed class LegacyValueParserTests
{
    // ----------------------------------------------------------------
    // TryParseColor / ParseColors
    // ----------------------------------------------------------------

    [Fact]
    public void ParseColors_returns_null_for_null_input()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(null, ScopeKey.Default, "color", diagnostics);

        Assert.Null(result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseColors_parses_6_digit_hex_as_ARGB_with_FF_alpha()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(new[] { "#102030" }, ScopeKey.Default, "color", diagnostics);

        Assert.Empty(diagnostics);
        Assert.Single(result);
        Assert.Equal(ColorReferenceKind.Argb, result[0].Kind);
        Assert.Equal(0xFF102030u, result[0].Argb);
    }

    [Fact]
    public void ParseColors_parses_8_digit_hex_as_ARGB()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(new[] { "#AA102030" }, ScopeKey.Default, "color", diagnostics);

        Assert.Empty(diagnostics);
        Assert.Single(result);
        Assert.Equal(0xAA102030u, result[0].Argb);
    }

    [Fact]
    public void ParseColors_parses_decimal_integer_as_palette_id()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(new[] { "83" }, ScopeKey.Default, "color", diagnostics);

        Assert.Empty(diagnostics);
        Assert.Single(result);
        Assert.Equal(ColorReferenceKind.PaletteId, result[0].Kind);
        Assert.Equal(83, result[0].PaletteId);
    }

    [Fact]
    public void ParseColors_adds_diagnostic_for_invalid_value_and_skips_it()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(new[] { "not-a-color", "#102030" }, ScopeKey.Default, "color", diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidColor", diagnostics[0].Code);
        Assert.Single(result);
        Assert.Equal(0xFF102030u, result[0].Argb);
    }

    [Fact]
    public void ParseColors_returns_empty_reference_for_whitespace_value()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(new[] { "  " }, ScopeKey.Default, "color", diagnostics);

        Assert.Empty(diagnostics);
        Assert.Single(result);
        Assert.Equal(uint.MaxValue, result[0].Argb);
    }

    [Fact]
    public void ParseColors_rejects_7_digit_hex()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseColors(new[] { "#1234567" }, ScopeKey.Default, "color", diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidColor", diagnostics[0].Code);
        Assert.Empty(result);
    }

    // ----------------------------------------------------------------
    // ParseCampaignColor
    // ----------------------------------------------------------------

    [Fact]
    public void ParseCampaignColor_returns_null_for_null_input()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseCampaignColor(null, ScopeKey.Default, "primaryColor", diagnostics);

        Assert.Null(result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseCampaignColor_returns_null_for_whitespace_input()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseCampaignColor("   ", ScopeKey.Default, "primaryColor", diagnostics);

        Assert.Null(result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseCampaignColor_parses_valid_hex_color()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseCampaignColor("#AABBCC", ScopeKey.Default, "primaryColor", diagnostics);

        Assert.Empty(diagnostics);
        Assert.NotNull(result);
        Assert.Equal(0xFFAABBCCu, result.Argb);
    }

    [Fact]
    public void ParseCampaignColor_adds_diagnostic_for_invalid_value_and_returns_null()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var result = Parser().ParseCampaignColor("not-a-color", ScopeKey.Default, "primaryColor", diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("InvalidColor", diagnostics[0].Code);
        Assert.Null(result);
    }

    // ----------------------------------------------------------------
    // ParseStrategy
    // ----------------------------------------------------------------

    [Theory]
    [InlineData(1, ColorStrategy.RandomConfigured)]
    [InlineData(11, ColorStrategy.RandomPaired)]
    [InlineData(3, ColorStrategy.FixedPair)]
    [InlineData(2, ColorStrategy.BannerFirstTwo)]
    [InlineData(4, ColorStrategy.BannerFirstTwo)]
    [InlineData(21, ColorStrategy.BannerFirstTwoRandomOrder)]
    [InlineData(41, ColorStrategy.BannerFirstTwoRandomOrder)]
    [InlineData(22, ColorStrategy.BannerRandomColors)]
    [InlineData(42, ColorStrategy.BannerRandomColors)]
    public void ParseStrategy_maps_known_mode_to_expected_strategy(int mode, ColorStrategy expected)
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        ColorStrategy? result = Parser().ParseStrategy(mode, ScopeKey.Default, diagnostics);

        Assert.Equal(expected, result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseStrategy_returns_null_for_null_mode()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        ColorStrategy? result = Parser().ParseStrategy(null, ScopeKey.Default, diagnostics);

        Assert.Null(result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseStrategy_adds_diagnostic_and_returns_Unresolved_for_unknown_mode()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        ColorStrategy? result = Parser().ParseStrategy(99, ScopeKey.Default, diagnostics);

        Assert.Equal(ColorStrategy.Unresolved, result);
        Assert.Single(diagnostics);
        Assert.Equal("UnknownMode", diagnostics[0].Code);
    }

    // ----------------------------------------------------------------
    // ParseBoolean
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    public void ParseBoolean_parses_valid_booleans(string value, bool expected)
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        bool? result = Parser().ParseBoolean(value, ScopeKey.Default, "BearRulerColors", diagnostics);

        Assert.Equal(expected, result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseBoolean_returns_null_for_null_input()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        bool? result = Parser().ParseBoolean(null, ScopeKey.Default, "BearRulerColors", diagnostics);

        Assert.Null(result);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ParseBoolean_adds_diagnostic_and_returns_false_for_invalid_input()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        bool? result = Parser().ParseBoolean("not-a-boolean", ScopeKey.Default, "BearRulerColors", diagnostics);

        Assert.False(result);
        Assert.Single(diagnostics);
        Assert.Equal("InvalidBoolean", diagnostics[0].Code);
    }

    // ----------------------------------------------------------------
    // ParseVariables
    // ----------------------------------------------------------------

    [Fact]
    public void ParseVariables_returns_null_for_null_input()
    {
        var result = Parser().ParseVariables(null);

        Assert.Null(result);
    }

    [Fact]
    public void ParseVariables_clones_each_group_and_its_values()
    {
        var original = new[] { "#102030", "#405060" };
        var groups = new[]
        {
            new Map<string, string[]> { ["$sigil"] = original }
        };

        var result = Parser().ParseVariables(groups);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result[0].ContainsKey("$sigil"));
        Assert.Equal(new[] { "#102030", "#405060" }, result[0]["$sigil"]);
        // verify clone — mutation of original should not affect result
        original[0] = "changed";
        Assert.Equal("#102030", result[0]["$sigil"][0]);
    }

    [Fact]
    public void ParseVariables_handles_null_group_entry()
    {
        var groups = new Map<string, string[]>[] { null };

        var result = Parser().ParseVariables(groups);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Empty(result[0]);
    }

    [Fact]
    public void ParseVariables_stores_null_value_for_null_array_entry()
    {
        var groups = new[]
        {
            new Map<string, string[]> { ["$sigil"] = null }
        };

        var result = Parser().ParseVariables(groups);

        Assert.Null(result[0]["$sigil"]);
    }

    // ----------------------------------------------------------------
    // Describe (via diagnostic message content)
    // ----------------------------------------------------------------

    [Fact]
    public void ParseStrategy_diagnostic_message_describes_default_scope()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        Parser().ParseStrategy(99, ScopeKey.Default, diagnostics);

        Assert.Contains("default scope", diagnostics[0].Message);
    }

    [Fact]
    public void ParseStrategy_diagnostic_message_describes_named_scope()
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        Parser().ParseStrategy(99, ScopeKey.ForKingdomClanUnit("Vlandia", "dey_tihr", "Trooper"), diagnostics);

        Assert.Contains("Vlandia", diagnostics[0].Message);
        Assert.Contains("dey_tihr", diagnostics[0].Message);
        Assert.Contains("Trooper", diagnostics[0].Message);
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static LegacyValueParser Parser() => new LegacyValueParser();
}
