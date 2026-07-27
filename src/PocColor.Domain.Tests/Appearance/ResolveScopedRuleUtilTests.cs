using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Resolution;
using Xunit;

namespace PocColor.Domain.Tests.Appearance;

public sealed class ResolveScopedRuleUtilTests
{
    // ----------------------------------------------------------------
    // Default scope
    // ----------------------------------------------------------------

    [Fact]
    public void Returns_default_rule_when_no_other_scopes_match()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.ForFixedPair(0xFF010101u, 0xFF010101u)
        });

        RuleOverride result = Util().Resolve(rules, Unit());

        Assert.Equal(ColorStrategy.FixedPair, result.Strategy);
        Assert.Equal(0xFF010101u, result.PrimaryColors[0].Argb);
    }

    // ----------------------------------------------------------------
    // Kingdom / clan scope precedence
    // ----------------------------------------------------------------

    [Fact]
    public void Kingdom_scope_overrides_default()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.ForFixedPair(0xFF000001u, 0xFF000001u),
            [ScopeKey.ForKingdom("Vlandia")] = RuleOverride.ForFixedPair(0xFF000002u, 0xFF000002u)
        });

        RuleOverride result = Util().Resolve(rules, Unit(kingdomName: "Vlandia"));

        Assert.Equal(0xFF000002u, result.PrimaryColors[0].Argb);
    }

    [Fact]
    public void Clan_scope_overrides_default()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.ForFixedPair(0xFF000001u, 0xFF000001u),
            [ScopeKey.ForClan("dey_tihr")] = RuleOverride.ForFixedPair(0xFF000003u, 0xFF000003u)
        });

        RuleOverride result = Util().Resolve(rules, Unit(clanName: "dey_tihr"));

        Assert.Equal(0xFF000003u, result.PrimaryColors[0].Argb);
    }

    [Fact]
    public void KingdomClan_scope_overrides_kingdom_and_clan()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.ForFixedPair(0xFF000001u, 0xFF000001u),
            [ScopeKey.ForKingdom("Vlandia")] = RuleOverride.ForFixedPair(0xFF000002u, 0xFF000002u),
            [ScopeKey.ForClan("dey_tihr")] = RuleOverride.ForFixedPair(0xFF000003u, 0xFF000003u),
            [ScopeKey.ForKingdomClan("Vlandia", "dey_tihr")] = RuleOverride.ForFixedPair(0xFF000004u, 0xFF000004u)
        });

        RuleOverride result = Util().Resolve(rules, Unit(kingdomName: "Vlandia", clanName: "dey_tihr"));

        Assert.Equal(0xFF000004u, result.PrimaryColors[0].Argb);
    }

    // ----------------------------------------------------------------
    // Unit scope precedence (most specific wins across all 8 scopes)
    // ----------------------------------------------------------------

    [Fact]
    public void Most_specific_kingdom_clan_unit_scope_wins_over_all_others()
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

        RuleOverride result = Util().Resolve(
            rules,
            Unit(kingdomName: "Vlandia", clanName: "dey_tihr", unitName: "Vlandian Spearman"));

        Assert.Equal(0xFF102030u, result.PrimaryColors[0].Argb);
    }

    // ----------------------------------------------------------------
    // Unit candidates: Player/King/Leader/Hero/Trooper fallbacks
    // ----------------------------------------------------------------

    [Theory]
    [InlineData(false, false, false, false, "Trooper")]
    [InlineData(true, false, false, false, "Player")]
    [InlineData(false, true, false, false, "King")]
    [InlineData(false, false, true, false, "Leader")]
    [InlineData(false, false, false, true, "Hero")]
    public void Unit_scope_uses_player_king_leader_hero_trooper_fallbacks(
        bool isPlayer, bool isKing, bool isLeader, bool isHero, string expectedScope)
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.ForUnit(expectedScope)] = RuleOverride.ForFixedPair(0xFFABCDEFu, 0xFFABCDEFu)
        });

        RuleOverride result = Util().Resolve(
            rules,
            Unit(isPlayer: isPlayer, isKing: isKing, isLeader: isLeader, isHero: isHero));

        Assert.Equal(0xFFABCDEFu, result.PrimaryColors[0].Argb);
    }

    // ----------------------------------------------------------------
    // PlayerKingdom / PlayerClan alias resolution
    // ----------------------------------------------------------------

    [Fact]
    public void PlayerKingdom_alias_resolves_when_kingdom_branch_exists()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.ForKingdomUnit("PlayerKingdom", "Player")] = RuleOverride.ForFixedPair(0xFF040404u, 0xFF040404u)
        });

        RuleOverride result = Util().Resolve(
            rules,
            Unit(kingdomName: "Custom Kingdom", unitName: "Player", isPlayerKingdom: true));

        Assert.Equal(0xFF040404u, result.PrimaryColors[0].Argb);
    }

    [Fact]
    public void PlayerClan_alias_resolves_when_clan_branch_exists()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.ForClanUnit("PlayerClan", "Player")] = RuleOverride.ForFixedPair(0xFF050505u, 0xFF050505u)
        });

        RuleOverride result = Util().Resolve(
            rules,
            Unit(clanName: "Custom Clan", unitName: "Player", isPlayerClan: true));

        Assert.Equal(0xFF050505u, result.PrimaryColors[0].Argb);
    }

    // ----------------------------------------------------------------
    // Criteria (culture / tier / type) within unit scope
    // ----------------------------------------------------------------

    [Fact]
    public void Culture_tier_and_type_criteria_override_base_unit_rule()
    {
        ScopeKey unit = ScopeKey.ForUnit("Imperial Archer");
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [unit] = RuleOverride.ForFixedPair(0xFF010101u, 0xFF010101u),
            [unit.WithCriteria(culture: "Empire", tier: 3, isMounted: false, isRanged: true)] =
                RuleOverride.ForFixedPair(0xFF020202u, 0xFF020202u)
        });

        RuleOverride result = Util().Resolve(
            rules,
            Unit(unitName: "Imperial Archer", isMounted: false, isRanged: true, tier: 3, culture: "Empire"));

        Assert.Equal(0xFF020202u, result.PrimaryColors[0].Argb);
    }

    [Fact]
    public void Nested_only_kingdom_unit_scope_applies_without_root_kingdom_rule()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.ForKingdomUnit("Vlandia", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF010101u, 0xFF010101u)
        });

        RuleOverride result = Util().Resolve(
            rules,
            Unit(kingdomName: "Vlandia", unitName: "Vlandian Spearman"));

        Assert.Equal(0xFF010101u, result.PrimaryColors[0].Argb);
    }

    [Fact]
    public void Nested_only_clan_unit_scope_applies_without_root_clan_rule()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.ForClanUnit("dey_tihr", "Vlandian Spearman")] = RuleOverride.ForFixedPair(0xFF020202u, 0xFF020202u)
        });

        RuleOverride result = Util().Resolve(
            rules,
            Unit(clanName: "dey_tihr", unitName: "Vlandian Spearman"));

        Assert.Equal(0xFF020202u, result.PrimaryColors[0].Argb);
    }

    [Fact]
    public void Null_rules_returns_empty_rule()
    {
        RuleOverride result = Util().Resolve(null, Unit());

        Assert.Same(RuleOverride.Empty, result);
    }

    [Fact]
    public void Null_profile_returns_default_rule()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.ForFixedPair(0xFF010101u, 0xFF010101u)
        });

        RuleOverride result = Util().Resolve(rules, null);

        Assert.Equal(0xFF010101u, result.PrimaryColors[0].Argb);
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static IResolveScopedRuleUtil Util() => new ResolveScopedRuleUtil();

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
            kingdomName, clanName, unitName,
            isPlayerKingdom, isPlayerClan,
            isPlayer, isKing, isLeader, isHero,
            isMounted, isRanged,
            tier, culture, rulerBannerCode);
    }
}
