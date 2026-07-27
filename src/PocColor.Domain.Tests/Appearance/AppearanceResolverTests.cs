using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using Xunit;

namespace PocColor.Domain.Tests.Appearance;

public sealed class AppearanceResolverTests
{
    [Fact]
    public void Resolver_draws_again_for_each_independent_resolution()
    {
        var choices = new SequenceChoiceSource(0, 0, 1, 0);

        Assert.Equal(0xFF010101u, ResolveRandomConfigured(choices).PrimaryColor);
        Assert.Equal(0xFF020202u, ResolveRandomConfigured(choices).PrimaryColor);
        Assert.Equal(4, choices.CallCount);
    }

    [Fact]
    public void Most_specific_kingdom_clan_unit_override_wins()
    {
        ResolvedAppearance result = ResolveWithAllV1Scopes();

        Assert.Equal(0xFF102030u, result.PrimaryColor);
    }

    [Fact]
    public void Fixed_pair_uses_its_first_two_configured_colours()
    {
        ResolvedAppearance result = Resolve(
            RuleOverride.ForFixedPair(0xFF010203u, 0xFF040506u));

        Assert.Equal(0xFF010203u, result.PrimaryColor);
        Assert.Equal(0xFF040506u, result.SecondaryColor);
    }

    [Fact]
    public void Random_paired_uses_one_choice_for_both_channels()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: ColorStrategy.RandomPaired,
                primaryColors: new[] { 0xFF010101u, 0xFF020202u },
                secondaryColors: new[] { 0xFF030303u, 0xFF040404u }),
            choices: new SequenceChoiceSource(1));

        Assert.Equal(0xFF020202u, result.PrimaryColor);
        Assert.Equal(0xFF040404u, result.SecondaryColor);
    }

    [Fact]
    public void Banner_first_two_strategy_covers_v1_modes_2_and_4()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(strategy: ColorStrategy.BannerFirstTwo),
            banner: new BannerSnapshot("current", new[] { 0xFF010203u, 0xFF040506u }));

        Assert.Equal(0xFF010203u, result.PrimaryColor);
        Assert.Equal(0xFF040506u, result.SecondaryColor);
    }

    [Fact]
    public void Banner_first_two_random_order_strategy_covers_v1_modes_21_and_41()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(strategy: ColorStrategy.BannerFirstTwoRandomOrder),
            banner: new BannerSnapshot("current", new[] { 0xFF010203u, 0xFF040506u }),
            choices: new SequenceChoiceSource(1));

        Assert.Equal(0xFF040506u, result.PrimaryColor);
        Assert.Equal(0xFF010203u, result.SecondaryColor);
    }

    [Fact]
    public void Banner_random_colours_strategy_covers_v1_modes_22_and_42()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(strategy: ColorStrategy.BannerRandomColors),
            banner: new BannerSnapshot("current", new[] { 0xFF010101u, 0xFF020202u, 0xFF030303u }),
            choices: new SequenceChoiceSource(2, 0));

        Assert.Equal(0xFF030303u, result.PrimaryColor);
        Assert.Equal(0xFF010101u, result.SecondaryColor);
    }

    [Theory]
    [InlineData(ColorStrategy.BannerFirstTwo)]
    [InlineData(ColorStrategy.BannerFirstTwoRandomOrder)]
    [InlineData(ColorStrategy.BannerRandomColors)]
    public void Banner_strategies_use_the_selected_shield_palette_before_banner_or_current(
        ColorStrategy strategy)
    {
        var palette = new AppearancePalette(
            new Dictionary<string, BannerSnapshot>
            {
                ["selected-shield"] = new BannerSnapshot("selected-shield", new[] { 0xFF111111u, 0xFF121212u }),
                ["selected-banner"] = new BannerSnapshot("selected-banner", new[] { 0xFF222222u, 0xFF232323u })
            },
            new uint[0]);

        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: strategy,
                bannerCodes: new[] { "selected-banner" },
                shieldCodes: new[] { "selected-shield" }),
            banner: new BannerSnapshot("current", new[] { 0xFF333333u, 0xFF343434u }),
            choices: new SequenceChoiceSource(0),
            palette: palette);

        Assert.Equal(0xFF111111u, result.PrimaryColor);
    }

    [Fact]
    public void Selected_banner_palette_precedes_the_current_palette_when_the_shield_palette_is_unavailable()
    {
        var palette = new AppearancePalette(
            new Dictionary<string, BannerSnapshot>
            {
                ["selected-banner"] = new BannerSnapshot("selected-banner", new[] { 0xFF222222u, 0xFF232323u })
            },
            new uint[0]);

        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: ColorStrategy.BannerFirstTwo,
                bannerCodes: new[] { "selected-banner" },
                shieldCodes: new[] { "unknown-shield" }),
            banner: new BannerSnapshot("current", new[] { 0xFF333333u, 0xFF343434u }),
            palette: palette);

        Assert.Equal(0xFF222222u, result.PrimaryColor);
    }

    [Fact]
    public void Current_palette_is_used_when_neither_selected_code_has_palette_data()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: ColorStrategy.BannerFirstTwo,
                bannerCodes: new[] { "unknown-banner" },
                shieldCodes: new[] { "unknown-shield" }),
            banner: new BannerSnapshot("current", new[] { 0xFF333333u, 0xFF343434u }),
            palette: new AppearancePalette(new Dictionary<string, BannerSnapshot>(), new uint[0]));

        Assert.Equal(0xFF333333u, result.PrimaryColor);
    }

    [Fact]
    public void Banner_strategies_leave_colours_unset_when_the_banner_has_too_few_colours()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(strategy: ColorStrategy.BannerFirstTwo),
            banner: new BannerSnapshot("current", new[] { 0xFF010101u }));

        Assert.Null(result.PrimaryColor);
        Assert.Null(result.SecondaryColor);
    }

    [Fact]
    public void Random_configured_draws_a_default_palette_colour_for_each_empty_channel()
    {
        var choices = new SequenceChoiceSource(0, 1);
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: ColorStrategy.RandomConfigured,
                primaryColors: new uint[0],
                secondaryColors: new uint[0]),
            choices: choices,
            palette: new AppearancePalette(
                new Dictionary<string, BannerSnapshot>(),
                new[] { 0xFF010101u, 0xFF020202u }));

        Assert.Equal(0xFF010101u, result.PrimaryColor);
        Assert.Equal(0xFF020202u, result.SecondaryColor);
        Assert.Equal(2, choices.CallCount);
    }

    [Fact]
    public void Random_configured_uses_a_default_colour_when_one_configured_channel_is_empty()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: ColorStrategy.RandomConfigured,
                primaryColors: new uint[0],
                secondaryColors: new[] { 0xFF030303u }),
            choices: new SequenceChoiceSource(1, 0),
            palette: new AppearancePalette(
                new Dictionary<string, BannerSnapshot>(),
                new[] { 0xFF010101u, 0xFF020202u }));

        Assert.Equal(0xFF020202u, result.PrimaryColor);
        Assert.Equal(0xFF030303u, result.SecondaryColor);
    }

    [Fact]
    public void Random_paired_shares_one_default_palette_draw_when_both_channels_are_empty()
    {
        var choices = new SequenceChoiceSource(1);
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                strategy: ColorStrategy.RandomPaired,
                primaryColors: new uint[0],
                secondaryColors: new uint[0]),
            choices: choices,
            palette: new AppearancePalette(
                new Dictionary<string, BannerSnapshot>(),
                new[] { 0xFF010101u, 0xFF020202u }));

        Assert.Equal(0xFF020202u, result.PrimaryColor);
        Assert.Equal(0xFF020202u, result.SecondaryColor);
        Assert.Equal(1, choices.CallCount);
    }

    [Fact]
    public void Battle_resolution_prefers_non_empty_battle_banner_and_shield_choices()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                bannerCodes: new[] { "normal-banner" },
                shieldCodes: new[] { "normal-shield" },
                battleBannerCodes: new[] { "battle-banner" },
                battleShieldCodes: new[] { "battle-shield" }),
            kind: ResolutionKind.Battle);

        Assert.Equal("battle-banner", result.BannerCode);
        Assert.Equal("battle-shield", result.ShieldCode);
    }

    [Fact]
    public void Culture_tier_and_type_criteria_override_the_base_unit_rule()
    {
        ScopeKey unit = ScopeKey.ForUnit("Imperial Archer");
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [unit] = RuleOverride.ForFixedPair(0xFF010101u, 0xFF010101u),
            [unit.WithCriteria(culture: "Empire", tier: 3, isMounted: false, isRanged: true)] =
                RuleOverride.ForFixedPair(0xFF020202u, 0xFF020202u)
        });

        ResolvedAppearance result = new ResolveAppearanceUseCase().Resolve(
            rules,
            Unit(unitName: "Imperial Archer", isMounted: false, isRanged: true, tier: 3, culture: "Empire"),
            new BannerSnapshot("current", new uint[0]),
            ResolutionKind.Normal,
            new SequenceChoiceSource());

        Assert.Equal(0xFF020202u, result.PrimaryColor);
    }

    [Fact]
    public void Nested_only_kingdom_unit_scope_applies_without_a_root_kingdom_rule()
    {
        ResolvedAppearance result = Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.ForKingdomUnit("Vlandia", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF010101u, 0xFF010101u)
            }),
            Unit(kingdomName: "Vlandia", unitName: "Vlandian Spearman"));

        Assert.Equal(0xFF010101u, result.PrimaryColor);
    }

    [Fact]
    public void Nested_only_clan_unit_scope_applies_without_a_root_clan_rule()
    {
        ResolvedAppearance result = Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.ForClanUnit("dey_tihr", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF020202u, 0xFF020202u)
            }),
            Unit(clanName: "dey_tihr", unitName: "Vlandian Spearman"));

        Assert.Equal(0xFF020202u, result.PrimaryColor);
    }

    [Fact]
    public void Nested_only_kingdom_clan_unit_scope_applies_without_root_parent_rules()
    {
        ResolvedAppearance result = Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.ForKingdomClanUnit("Vlandia", "dey_tihr", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF030303u, 0xFF030303u)
            }),
            Unit(kingdomName: "Vlandia", clanName: "dey_tihr", unitName: "Vlandian Spearman"));

        Assert.Equal(0xFF030303u, result.PrimaryColor);
    }

    [Fact]
    public void Nested_only_player_kingdom_unit_alias_scope_applies_without_a_root_kingdom_rule()
    {
        ResolvedAppearance result = Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.ForKingdomUnit("PlayerKingdom", "Player")]= RuleOverride.ForFixedPair(0xFF040404u, 0xFF040404u)
            }),
            Unit(
                kingdomName: "Custom Kingdom",
                unitName: "Player",
                isPlayerKingdom: true));

        Assert.Equal(0xFF040404u, result.PrimaryColor);
    }

    [Fact]
    public void Nested_only_player_clan_unit_alias_scope_applies_without_a_root_clan_rule()
    {
        ResolvedAppearance result = Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.ForClanUnit("PlayerClan", "Player")]= RuleOverride.ForFixedPair(0xFF050505u, 0xFF050505u)
            }),
            Unit(
                clanName: "Custom Clan",
                unitName: "Player",
                isPlayerClan: true));

        Assert.Equal(0xFF050505u, result.PrimaryColor);
    }

    [Fact]
    public void Nested_only_player_clan_kingdom_clan_unit_alias_scope_applies_without_root_parent_rules()
    {
        ResolvedAppearance result = Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.ForKingdomClanUnit("Vlandia", "PlayerClan", "Player")] = RuleOverride.ForFixedPair(0xFF060606u, 0xFF060606u)
            }),
            Unit(
                kingdomName: "Vlandia",
                clanName: "Custom Clan",
                unitName: "Player",
                isPlayerClan: true));

        Assert.Equal(0xFF060606u, result.PrimaryColor);
    }

    [Theory]
    [InlineData(false, false, false, false, "Trooper")]
    [InlineData(true, false, false, false, "Player")]
    [InlineData(false, true, false, false, "King")]
    [InlineData(false, false, true, false, "Leader")]
    [InlineData(false, false, false, true, "Hero")]
    public void Unit_scope_uses_the_v1_player_king_leader_hero_and_trooper_fallbacks(
        bool isPlayer,
        bool isKing,
        bool isLeader,
        bool isHero,
        string expectedScope)
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.ForUnit(expectedScope)] = RuleOverride.ForFixedPair(0xFFABCDEFu, 0xFFABCDEFu)
        });

        ResolvedAppearance result = new ResolveAppearanceUseCase().Resolve(
            rules,
            Unit(isPlayer: isPlayer, isKing: isKing, isLeader: isLeader, isHero: isHero),
            new BannerSnapshot("current", new uint[0]),
            ResolutionKind.Normal,
            new SequenceChoiceSource());

        Assert.Equal(0xFFABCDEFu, result.PrimaryColor);
    }

    [Fact]
    public void Ruler_flags_apply_ruler_colours_and_the_serialized_ruler_banner()
    {
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                bearRulerColors: true,
                rulerStrategy: ColorStrategy.FixedPair,
                rulerPrimaryColors: new[] { 0xFF101010u, 0xFF202020u },
                bearRulerBanner: true,
                bearRulerShield: true),
            unit: Unit(rulerBannerCode: "serialized-ruler-banner"));

        Assert.Equal(0xFF101010u, result.PrimaryColor);
        Assert.Equal(0xFF202020u, result.SecondaryColor);
        Assert.Equal("serialized-ruler-banner", result.BannerCode);
        Assert.Equal("serialized-ruler-banner", result.ShieldCode);
    }

    [Fact]
    public void Variable_groups_are_evaluated_per_resolution_before_banner_selection()
    {
        var variables = new[]
        {
            (IReadOnlyDictionary<string, IReadOnlyList<string>>)new Dictionary<string, IReadOnlyList<string>>
            {
                ["{colour}"] = new[] { "red", "blue" },
                ["{shade}"] = new[] { "light", "dark" }
            }
        };
        ResolvedAppearance result = Resolve(
            new RuleOverride(
                bannerCodes: new[] { "banner-{colour}-{shade}" },
                variableGroups: variables),
            choices: new SequenceChoiceSource(1));

        Assert.Equal("banner-blue-dark", result.BannerCode);
    }

    private static ResolvedAppearance ResolveRandomConfigured(SequenceChoiceSource choices)
    {
        return Resolve(
            new RuleOverride(
                strategy: ColorStrategy.RandomConfigured,
                primaryColors: new[] { 0xFF010101u, 0xFF020202u }),
            choices: choices);
    }

    private static ResolvedAppearance ResolveWithAllV1Scopes()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.ForFixedPair(0xFF000001u, 0xFF000001u),
            [ScopeKey.ForKingdom("Vlandia")] = RuleOverride.ForFixedPair(0xFF000002u, 0xFF000002u),
            [ScopeKey.ForClan("dey_tihr")] = RuleOverride.ForFixedPair(0xFF000003u, 0xFF000003u),
            [ScopeKey.ForKingdomClan("Vlandia", "dey_tihr")] = RuleOverride.ForFixedPair(0xFF000004u, 0xFF000004u),
            [ScopeKey.ForUnit("Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF000005u, 0xFF000005u),
            [ScopeKey.ForKingdomUnit("Vlandia", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF000006u, 0xFF000006u),
            [ScopeKey.ForClanUnit("dey_tihr", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF000007u, 0xFF000007u),
            [ScopeKey.ForKingdomClanUnit("Vlandia", "dey_tihr", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF102030u, 0xFF102030u)
        });

        return new ResolveAppearanceUseCase().Resolve(
            rules,
            Unit(kingdomName: "Vlandia", clanName: "dey_tihr", unitName: "Vlandian Spearman"),
            new BannerSnapshot("current", new uint[0]),
            ResolutionKind.Normal,
            new SequenceChoiceSource());
    }

    private static ResolvedAppearance Resolve(
        RuleOverride rule,
        BannerSnapshot banner = null,
        ResolutionKind kind = ResolutionKind.Normal,
        SequenceChoiceSource choices = null,
        UnitProfile unit = null,
        AppearancePalette palette = null)
    {
        return new ResolveAppearanceUseCase().Resolve(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride> { [ScopeKey.Default] = rule }),
            unit ?? Unit(),
            banner ?? new BannerSnapshot("current", new uint[0]),
            kind,
            choices ?? new SequenceChoiceSource(),
            palette);
    }

    private static ResolvedAppearance Resolve(AppearanceRuleSet rules, UnitProfile unit)
    {
        return new ResolveAppearanceUseCase().Resolve(
            rules,
            unit,
            new BannerSnapshot("current", new uint[0]),
            ResolutionKind.Normal,
            new SequenceChoiceSource());
    }

    private static UnitProfile Unit(
        string kingdomName = "Kingdom",
        string clanName = "Clan",
        string unitName = "Unit",
        bool isPlayerKingdom = false,
        bool isPlayerClan = false,
        bool isPlayer = false,
        bool isKing = false,
        bool isLeader = false,
        bool isHero = false,
        bool isMounted = false,
        bool isRanged = false,
        int tier = 1,
        string culture = "Culture",
        string rulerBannerCode = null)
    {
        return new UnitProfile(
            kingdomName,
            clanName,
            unitName,
            isPlayerKingdom: isPlayerKingdom,
            isPlayerClan: isPlayerClan,
            isPlayer: isPlayer,
            isKing: isKing,
            isLeader: isLeader,
            isHero: isHero,
            isMounted: isMounted,
            isRanged: isRanged,
            tier: tier,
            culture: culture,
            rulerBannerCode: rulerBannerCode);
    }
}
