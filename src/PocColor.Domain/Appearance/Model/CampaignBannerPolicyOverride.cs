namespace PocColor.Domain.Appearance.Model;

public sealed class CampaignBannerPolicyOverride
{
    public static CampaignBannerPolicyOverride Empty { get; } = new CampaignBannerPolicyOverride();

    public CampaignBannerPolicyOverride(
        bool? followKingdomColors = null,
        bool? followKingdomBackgroundOnly = null,
        string configuredBannerCode = null,
        ColorReference configuredPrimaryColor = null,
        ColorReference configuredSecondaryColor = null,
        string kingdomBannerCode = null,
        ColorReference kingdomPrimaryColor = null,
        ColorReference kingdomSecondaryColor = null)
    {
        FollowKingdomColors = followKingdomColors;
        FollowKingdomBackgroundOnly = followKingdomBackgroundOnly;
        ConfiguredBannerCode = configuredBannerCode;
        ConfiguredPrimaryColor = configuredPrimaryColor;
        ConfiguredSecondaryColor = configuredSecondaryColor;
        KingdomBannerCode = kingdomBannerCode;
        KingdomPrimaryColor = kingdomPrimaryColor;
        KingdomSecondaryColor = kingdomSecondaryColor;
    }

    public bool? FollowKingdomColors { get; }

    public bool? FollowKingdomBackgroundOnly { get; }

    public string ConfiguredBannerCode { get; }

    public ColorReference ConfiguredPrimaryColor { get; }

    public ColorReference ConfiguredSecondaryColor { get; }

    public string KingdomBannerCode { get; }

    public ColorReference KingdomPrimaryColor { get; }

    public ColorReference KingdomSecondaryColor { get; }

    internal CampaignBannerPolicyOverride Merge(CampaignBannerPolicyOverride later)
    {
        if (later == null || ReferenceEquals(later, Empty))
        {
            return this;
        }

        return new CampaignBannerPolicyOverride(
            later.FollowKingdomColors ?? FollowKingdomColors,
            later.FollowKingdomBackgroundOnly ?? FollowKingdomBackgroundOnly,
            later.ConfiguredBannerCode ?? ConfiguredBannerCode,
            later.ConfiguredPrimaryColor ?? ConfiguredPrimaryColor,
            later.ConfiguredSecondaryColor ?? ConfiguredSecondaryColor,
            later.KingdomBannerCode ?? KingdomBannerCode,
            later.KingdomPrimaryColor ?? KingdomPrimaryColor,
            later.KingdomSecondaryColor ?? KingdomSecondaryColor);
    }
}
