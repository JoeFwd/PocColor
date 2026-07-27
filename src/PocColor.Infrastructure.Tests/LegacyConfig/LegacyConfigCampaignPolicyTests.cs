using Newtonsoft.Json;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig;
using PocColor.Infrastructure.LegacyConfig.Model;
using Xunit;

namespace PocColor.Infrastructure.Tests.LegacyConfig;

public sealed class LegacyConfigCampaignPolicyTests
{
    [Fact]
    public void Compiler_maps_campaign_policy_with_v1_scope_precedence()
    {
        PocColorModConfig dto = JsonConvert.DeserializeObject<PocColorModConfig>(
            "{ 'defaultConfig': { 'FollowKingdomColors': 'true', 'FollowKingdomBackgroundOnly': 'true' }, "
            + "'kingdoms': { 'Vlandia': { 'FollowKingdomColors': 'true', 'clanBanner': 'kingdom-banner', "
            + "'clans': { 'dey_tihr': { 'FollowKingdomBackgroundOnly': 'false', 'clanBanner': 'kingdom-clan-banner' } } } }, "
            + "'clans': { 'dey_tihr': { 'FollowKingdomColors': 'false', 'clanBanner': 'global-clan-banner' } } }");

        CampaignBannerPolicy policy = new LegacyConfigCompiler().Compile(dto).Rules.GetCampaignBannerPolicy(
            "Vlandia",
            "dey_tihr",
            false,
            false,
            "cached-banner");

        Assert.False(policy.FollowKingdomColors);
        Assert.False(policy.FollowKingdomBackgroundOnly);
        Assert.Equal("kingdom-clan-banner", policy.ConfiguredBannerCode);
        Assert.Equal("cached-banner", policy.CachedBannerCode);
    }
}
