namespace PocColor.Domain.Appearance.Model;

public sealed class CampaignKingdomPolicy
{
    public CampaignKingdomPolicy(
        string bannerCode,
        ColorReference primaryColor,
        ColorReference secondaryColor)
    {
        BannerCode = bannerCode;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    public string BannerCode { get; }

    public ColorReference PrimaryColor { get; }

    public ColorReference SecondaryColor { get; }
}
