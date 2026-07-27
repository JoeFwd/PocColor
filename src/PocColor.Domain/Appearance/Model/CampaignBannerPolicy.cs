namespace PocColor.Domain.Appearance.Model;

public sealed class CampaignBannerPolicy
{
    public CampaignBannerPolicy(
        bool followKingdomColors,
        bool followKingdomBackgroundOnly,
        string configuredBannerCode = null,
        string cachedBannerCode = null,
        ColorReference configuredPrimaryColor = null,
        ColorReference configuredSecondaryColor = null)
    {
        FollowKingdomColors = followKingdomColors;
        FollowKingdomBackgroundOnly = followKingdomBackgroundOnly;
        ConfiguredBannerCode = configuredBannerCode;
        CachedBannerCode = cachedBannerCode;
        ConfiguredPrimaryColor = configuredPrimaryColor;
        ConfiguredSecondaryColor = configuredSecondaryColor;
    }

    public bool FollowKingdomColors { get; }

    public bool FollowKingdomBackgroundOnly { get; }

    public string ConfiguredBannerCode { get; }

    public string CachedBannerCode { get; }

    public ColorReference ConfiguredPrimaryColor { get; }

    public ColorReference ConfiguredSecondaryColor { get; }
}
