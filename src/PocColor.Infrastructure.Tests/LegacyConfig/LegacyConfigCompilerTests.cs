using System;
using System.IO;
using Newtonsoft.Json;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;
using PocColor.Infrastructure.LegacyConfig;
using PocColor.Infrastructure.LegacyConfig.Model;
using Xunit;

namespace PocColor.Infrastructure.Tests.LegacyConfig;

public sealed class LegacyConfigCompilerTests
{
    [Fact]
    public void Compiler_preserves_v1_scope_order()
    {
        PocColorModConfig dto = LoadFixture("scope-order.json");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);

        Assert.Empty(result.Diagnostics);
        RuleOverride rule = result.Rules.Get(
            ScopeKey.ForKingdomClanUnit("Vlandia", "dey_tihr", "Vlandian Spearman"));
        Assert.NotSame(RuleOverride.Empty, rule);
        Assert.Equal(ColorStrategy.FixedPair, rule.Strategy);
        Assert.Equal(
            new[] { ColorReference.FromArgb(0xFF102030u), ColorReference.FromArgb(0xFF405060u) },
            rule.PrimaryColors);
    }

    [Fact]
    public void Compiler_expands_groups_without_mutating_the_dto()
    {
        PocColorModConfig dto = LoadFixture("groups-and-vars.json");
        var originalUnits = dto.units;

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);

        Assert.Same(originalUnits, dto.units);
        RuleOverride spearman = result.Rules.Get(ScopeKey.ForUnit("Vlandian Spearman"));
        RuleOverride sergeant = result.Rules.Get(ScopeKey.ForUnit("Vlandian Sergeant"));

        Assert.Equal(ColorStrategy.FixedPair, spearman.Strategy);
        Assert.Equal(
            new[] { ColorReference.FromArgb(0xFF102030u), ColorReference.FromArgb(0xFF405060u) },
            spearman.PrimaryColors);
        Assert.Equal(new[] { "first-$sigil" }, spearman.BannerCodes);
        Assert.Equal("wolf", spearman.VariableGroups[0]["$sigil"][0]);
        Assert.Equal(
            new[] { ColorReference.FromArgb(0xFF708090u), ColorReference.FromArgb(0xFFA0B0C0u) },
            sergeant.PrimaryColors);
        Assert.Equal(new[] { "explicit-$sigil" }, sergeant.BannerCodes);
        Assert.Equal("stag", sergeant.VariableGroups[0]["$sigil"][0]);
    }

    [Fact]
    public void Compiler_preserves_battle_overlay_and_ruler_fields()
    {
        PocColorModConfig dto = LoadFixture("battle-and-ruler.json");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        ResolvedAppearance appearance = new ResolveAppearanceUseCase().Resolve(
            result.Rules,
            new UnitProfile(
                "Vlandia", "dey_tihr", "Vlandian Spearman",
                false, false, false, false, false, false, false, false, 3, "vlandia", "ruler-banner"),
            new BannerSnapshot("original", new uint[] { 0xFF000000u, 0xFFFFFFFFu }),
            ResolutionKind.Battle,
            new FirstChoice());

        Assert.Empty(result.Diagnostics);
        Assert.Equal(ColorStrategy.FixedPair, appearance.Strategy);
        Assert.Equal(0xFF112233u, appearance.PrimaryColor);
        Assert.Equal(0xFF445566u, appearance.SecondaryColor);
        Assert.Equal("battle-banner", appearance.BannerCode);
        Assert.Equal("battle-shield", appearance.ShieldCode);
    }

    [Fact]
    public void Merger_overlays_nested_v1_sections_without_discarding_existing_values()
    {
        PocColorModConfig target = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'kingdoms': { 'Vlandia': { 'units': { 'Vlandian Spearman': { 'mode': 1 } } } } }");
        PocColorModConfig source = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'kingdoms': { 'Vlandia': { 'units': { 'Vlandian Spearman': { 'color': [ '#102030' ] } } } } }");

        PocColorModConfig merged = LegacyConfigMerger.Merge(target, source);

        PocColorModConfigUnit unit = merged.kingdoms["Vlandia"].units["Vlandian Spearman"];
        Assert.Same(target, merged);
        Assert.Equal(1, unit.mode);
        Assert.Equal(new[] { "#102030" }, unit.color);
    }

    [Fact]
    public void Merger_preserves_kingdom_and_clan_hidden_section_properties()
    {
        var targetKingdom = new PocColorModConfigKingdom
        {
            mode = 1,
            clans = new Map<string, PocColorModConfigClan>
            {
                ["dey_tihr"] = new PocColorModConfigClan { mode = 1 }
            }
        };
        var sourceKingdom = new PocColorModConfigKingdom
        {
            color = new[] { "#102030" },
            clans = new Map<string, PocColorModConfigClan>
            {
                ["dey_tihr"] = new PocColorModConfigClan { color = new[] { "#405060" } }
            }
        };
        var target = new PocColorModConfig
        {
            kingdoms = new Map<string, PocColorModConfigKingdom> { ["Vlandia"] = targetKingdom }
        };
        var source = new PocColorModConfig
        {
            kingdoms = new Map<string, PocColorModConfigKingdom> { ["Vlandia"] = sourceKingdom }
        };

        Assert.Null(((ConfigSection)sourceKingdom).color);
        Assert.Null(((ConfigSection)sourceKingdom.clans["dey_tihr"]).color);

        LegacyConfigMerger.Merge(target, source);

        Assert.Equal(1, target.kingdoms["Vlandia"].mode);
        Assert.Equal(new[] { "#102030" }, target.kingdoms["Vlandia"].color);
        Assert.Equal(1, target.kingdoms["Vlandia"].clans["dey_tihr"].mode);
        Assert.Equal(new[] { "#405060" }, target.kingdoms["Vlandia"].clans["dey_tihr"].color);
    }

    [Fact]
    public void Loader_reads_the_legacy_config_file()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "config.json"), "{ 'defaultConfig': { 'mode': 3 } }");

            PocColorModConfig config = new LegacyConfigLoader(directory).Load();

            Assert.Equal(3, config.defaultConfig.mode);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Loader_applies_legacy_then_sorted_poc_configs_and_reports_invalid_json()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string modules = Path.Combine(root, "Modules");
        string pocColor = Path.Combine(modules, "PocColor");
        Directory.CreateDirectory(Path.Combine(modules, "Alpha"));
        Directory.CreateDirectory(Path.Combine(modules, "Beta"));
        Directory.CreateDirectory(Path.Combine(modules, "Gamma"));
        Directory.CreateDirectory(pocColor);
        try
        {
            File.WriteAllText(Path.Combine(pocColor, "config.json"), "{ 'defaultConfig': { 'mode': 1 } }");
            File.WriteAllText(Path.Combine(modules, "Alpha", "poc.config"), "{ 'defaultConfig': { 'mode': 2 } }");
            File.WriteAllText(Path.Combine(modules, "Beta", "poc.config.json"), "{ 'defaultConfig': { 'mode': 3 } }");
            File.WriteAllText(Path.Combine(modules, "Gamma", "poc.config"), "{ invalid json }");

            var loader = new LegacyConfigLoader(pocColor);
            PocColorModConfig config = loader.Load();

            Assert.Equal(3, config.defaultConfig.mode);
            Assert.Contains(loader.Diagnostics, diagnostic => diagnostic.Code == "ConfigReadFailure");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Compiler_reports_invalid_v1_boolean_and_unknown_mode_as_an_unresolved_override()
    {
        PocColorModConfig dto = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'defaultConfig': { 'mode': 99 }, 'units': { 'Trooper': { 'BearRulerColors': 'not-a-boolean' } } }");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        RuleOverride defaultRule = result.Rules.Get(ScopeKey.Default);
        RuleOverride unitRule = result.Rules.Get(ScopeKey.ForUnit("Trooper"));

        Assert.Equal(ColorStrategy.Unresolved, defaultRule.Strategy);
        Assert.False(unitRule.BearRulerColors);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "UnknownMode");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "InvalidBoolean");
    }

    [Fact]
    public void Compiler_applies_default_ruler_flags_to_colours_banner_and_shield()
    {
        PocColorModConfig dto = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'defaultConfig': { 'mode': 3, 'color': [ '#102030', '#405060' ], 'BearRulerColors': 'false', 'BearRulerBanner': 'false', 'BearRulerShield': 'false' } }");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        ResolvedAppearance appearance = new ResolveAppearanceUseCase().Resolve(
            result.Rules,
            new UnitProfile(
                null, null, "Vlandian Spearman", false, false, false, false, false, false,
                false, false, 3, "vlandia", "ruler-banner"),
            new BannerSnapshot("original", new uint[] { 0xFF112233u, 0xFF445566u }),
            ResolutionKind.Normal,
            new FirstChoice());

        Assert.Equal(ColorStrategy.FixedPair, appearance.Strategy);
        Assert.Equal(0xFF102030u, appearance.PrimaryColor);
        Assert.Equal(0xFF405060u, appearance.SecondaryColor);
        Assert.Null(appearance.BannerCode);
        Assert.Null(appearance.ShieldCode);
    }

    [Fact]
    public void Scoped_ruler_flags_override_default_ruler_flags()
    {
        PocColorModConfig dto = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'defaultConfig': { 'BearRulerColors': 'false', 'BearRulerBanner': 'false', 'BearRulerShield': 'false' }, 'units': { 'Trooper': { 'BearRulerColors': 'true', 'BearRulerBanner': 'true', 'BearRulerShield': 'true', 'rulerMode': 3, 'rulerColor': [ '#102030', '#405060' ], 'rulerBanner': 'scoped-banner', 'rulerShield': 'scoped-shield' } } }");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        ResolvedAppearance appearance = new ResolveAppearanceUseCase().Resolve(
            result.Rules,
            new UnitProfile(
                null, null, "Trooper", false, false, false, false, false, false,
                false, false, 3, "vlandia", "ruler-banner"),
            new BannerSnapshot("original", new uint[] { 0xFF112233u, 0xFF445566u }),
            ResolutionKind.Normal,
            new FirstChoice());

        Assert.Equal(ColorStrategy.FixedPair, appearance.Strategy);
        Assert.Equal(0xFF102030u, appearance.PrimaryColor);
        Assert.Equal(0xFF405060u, appearance.SecondaryColor);
        Assert.Equal("scoped-banner", appearance.BannerCode);
        Assert.Equal("scoped-shield", appearance.ShieldCode);
    }

    [Fact]
    public void Compiler_keeps_an_unknown_nested_mode_unresolved_instead_of_inheriting_the_default()
    {
        PocColorModConfig dto = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'defaultConfig': { 'mode': 3, 'color': [ '#102030', '#405060' ] }, 'units': { 'Trooper': { 'mode': 99 } } }");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        ResolvedAppearance appearance = new ResolveAppearanceUseCase().Resolve(
            result.Rules,
            new UnitProfile(
                null, null, "Trooper", false, false, false, false, false, false,
                false, false, 3, "vlandia", null),
            new BannerSnapshot("original", new uint[] { 0xFF112233u, 0xFF445566u }),
            ResolutionKind.Normal,
            new FirstChoice());

        Assert.Equal(ColorStrategy.Unresolved, appearance.Strategy);
        Assert.Null(appearance.PrimaryColorReference);
        Assert.Null(appearance.SecondaryColorReference);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "UnknownMode");
    }

    [Fact]
    public void Compiler_preserves_decimal_palette_ids_separately_from_equal_bit_argb_values()
    {
        PocColorModConfig dto = LoadFixture("color-identity.json");

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        RuleOverride rule = result.Rules.Get(ScopeKey.Default);
        ResolvedAppearance paletteAppearance = new ResolveAppearanceUseCase().Resolve(
            result.Rules,
            new UnitProfile(
                null, null, "Trooper", false, false, false, false, false, false,
                false, false, 3, "vlandia", null),
            new BannerSnapshot("original", new uint[] { 0xFF112233u, 0xFF445566u }),
            ResolutionKind.Normal,
            new FirstChoice());

        Assert.Empty(result.Diagnostics);
        Assert.Equal(ColorReferenceKind.PaletteId, rule.PrimaryColors[0].Kind);
        Assert.Equal(83, rule.PrimaryColors[0].PaletteId);
        Assert.Equal(ColorReferenceKind.Argb, rule.PrimaryColors[1].Kind);
        Assert.Equal(0x00000053u, rule.PrimaryColors[1].Argb);
        Assert.Equal(ColorReferenceKind.PaletteId, paletteAppearance.PrimaryColorReference.Kind);
    }

    [Fact]
    public void Compiler_maps_variables_and_nested_culture_tier_type_rules()
    {
        PocColorModConfig dto = JsonConvert.DeserializeObject<PocColorModConfig>(
            """
            {
              'units': {
                'Vlandian Spearman': {
                  'cultures': {
                    'vlandia': {
                      'tiers': {
                        '3': {
                          'types': {
                            'Infantry': {
                              'mode': 3,
                              'color': [ '#102030', '#405060' ],
                              'banners': [ 'banner_$sigil' ],
                              'vars': [ { '$sigil': [ 'wolf' ] } ]
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);

        LegacyConfigCompileResult result = new LegacyConfigCompiler().Compile(dto);
        ResolvedAppearance appearance = new ResolveAppearanceUseCase().Resolve(
            result.Rules,
            new UnitProfile(
                null, null, "Vlandian Spearman", false, false, false, false, false, false,
                false, false, 3, "vlandia", null),
            new BannerSnapshot("original", new uint[] { 0xFF000000u, 0xFFFFFFFFu }),
            ResolutionKind.Normal,
            new FirstChoice());

        Assert.Empty(result.Diagnostics);
        Assert.Equal(0xFF102030u, appearance.PrimaryColor);
        Assert.Equal(0xFF405060u, appearance.SecondaryColor);
        Assert.Equal("banner_wolf", appearance.BannerCode);
    }

    private static PocColorModConfig LoadFixture(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "LegacyConfig", "Fixtures", name);
        return JsonConvert.DeserializeObject<PocColorModConfig>(File.ReadAllText(path));
    }

    private sealed class FirstChoice : IChoiceSource
    {
        public int NextIndex(int exclusiveUpperBound) => 0;
    }
}
