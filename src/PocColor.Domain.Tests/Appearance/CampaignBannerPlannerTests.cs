using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using Xunit;

namespace PocColor.Domain.Tests.Appearance;

public sealed class CampaignBannerPlannerTests
{
    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, true, true)]
    public void Planner_selects_requested_kingdom_channels(
        bool followColors,
        bool backgroundOnly,
        bool primary,
        bool secondary)
    {
        CampaignBannerChange change = new PlanCampaignBannerUseCase().Plan(
            Policy(followColors, backgroundOnly));

        Assert.Equal(primary, change.ApplyKingdomPrimaryColor);
        Assert.Equal(secondary, change.ApplyKingdomSecondaryColor);
    }

    [Theory]
    [InlineData(false, false, "configured-banner", "cached-banner", "cached-banner")]
    [InlineData(true, false, "configured-banner", "cached-banner", null)]
    [InlineData(false, true, "configured-banner", "cached-banner", null)]
    public void Planner_keeps_configured_and_cached_banner_intent_distinct(
        bool followColors,
        bool backgroundOnly,
        string configuredBannerCode,
        string cachedBannerCode,
        string bannerCodeToRestore)
    {
        CampaignBannerChange change = new PlanCampaignBannerUseCase().Plan(
            Policy(followColors, backgroundOnly, configuredBannerCode, cachedBannerCode));

        Assert.Equal(configuredBannerCode, change.ConfiguredBannerCode);
        Assert.Equal(cachedBannerCode, change.CachedBannerCode);
        Assert.Equal(bannerCodeToRestore, change.BannerCodeToRestore);
    }

    [Theory]
    [InlineData(false, false, "cached-banner")]
    [InlineData(true, false, null)]
    [InlineData(false, true, null)]
    public void Planner_restores_cached_banner_without_configured_banner_only_when_not_following(
        bool followColors,
        bool backgroundOnly,
        string bannerCodeToRestore)
    {
        CampaignBannerChange change = new PlanCampaignBannerUseCase().Plan(
            Policy(followColors, backgroundOnly, null, "cached-banner"));

        Assert.Null(change.ConfiguredBannerCode);
        Assert.Equal("cached-banner", change.CachedBannerCode);
        Assert.Equal(bannerCodeToRestore, change.BannerCodeToRestore);
    }

    private static CampaignBannerPolicy Policy(
        bool followColors,
        bool backgroundOnly,
        string configuredBannerCode = "configured-banner",
        string cachedBannerCode = "cached-banner")
    {
        return new CampaignBannerPolicy(
            followColors,
            backgroundOnly,
            configuredBannerCode,
            cachedBannerCode);
    }
}
