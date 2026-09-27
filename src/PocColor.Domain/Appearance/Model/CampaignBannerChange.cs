namespace PocColor.Domain.Appearance.Model;

public sealed class CampaignBannerChange
{
    public CampaignBannerChange(
        string configuredBannerCode,
        string cachedBannerCode,
        string bannerCodeToRestore,
        bool applyKingdomPrimaryColor,
        bool applyKingdomSecondaryColor,
        ColorReference configuredPrimaryColor = null,
        ColorReference configuredSecondaryColor = null)
    {
        ConfiguredBannerCode = configuredBannerCode;
        CachedBannerCode = cachedBannerCode;
        BannerCodeToRestore = bannerCodeToRestore;
        ApplyKingdomPrimaryColor = applyKingdomPrimaryColor;
        ApplyKingdomSecondaryColor = applyKingdomSecondaryColor;
        ConfiguredPrimaryColor = configuredPrimaryColor;
        ConfiguredSecondaryColor = configuredSecondaryColor;
    }

    public string ConfiguredBannerCode { get; }

    public string CachedBannerCode { get; }

    public string BannerCodeToRestore { get; }

    public bool ApplyKingdomPrimaryColor { get; }

    public bool ApplyKingdomSecondaryColor { get; }

    public ColorReference ConfiguredPrimaryColor { get; }

    public ColorReference ConfiguredSecondaryColor { get; }

    public string ResolveBannerCodeToRestore(string bannerCodeBeforeKingdomUpdate)
    {
        if (string.IsNullOrWhiteSpace(BannerCodeToRestore))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(ConfiguredBannerCode))
        {
            return ConfiguredBannerCode;
        }

        return string.IsNullOrWhiteSpace(bannerCodeBeforeKingdomUpdate)
            ? BannerCodeToRestore
            : bannerCodeBeforeKingdomUpdate;
    }
}
