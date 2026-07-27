using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using Xunit;

namespace PocColor.Domain.Tests.Appearance;

public sealed class AppearanceRuleSetTests
{
    [Fact]
    public void Rule_set_copies_the_source_dictionary()
    {
        var source = new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = RuleOverride.Empty
        };
        var rules = new AppearanceRuleSet(source);
        source[ScopeKey.Default] = RuleOverride.ForFixedPair(1u, 2u);

        Assert.Same(RuleOverride.Empty, rules.Get(ScopeKey.Default));
    }

    [Fact]
    public void Rule_override_copies_nested_colour_collections()
    {
        var colors = new List<uint> { 0xFF010101u };
        var rule = new RuleOverride(primaryColors: colors);
        colors[0] = 0xFF020202u;

        Assert.Equal(ColorReference.FromArgb(0xFF010101u), rule.PrimaryColors[0]);
    }

    [Fact]
    public void Rule_set_copies_campaign_policy_overrides()
    {
        var source = new Dictionary<ScopeKey, CampaignBannerPolicyOverride>
        {
            [ScopeKey.Default] = new CampaignBannerPolicyOverride(followKingdomColors: true)
        };
        var rules = new AppearanceRuleSet(null, source);
        source[ScopeKey.Default] = new CampaignBannerPolicyOverride(followKingdomColors: false);

        CampaignBannerPolicy policy = rules.GetCampaignBannerPolicy(
            null,
            null,
            false,
            false,
            "cached-banner");

        Assert.True(policy.FollowKingdomColors);
    }
}
