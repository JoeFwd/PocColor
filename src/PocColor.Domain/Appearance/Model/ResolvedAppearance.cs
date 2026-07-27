namespace PocColor.Domain.Appearance.Model;

public sealed class ResolvedAppearance
{
    public ResolvedAppearance(
        ColorStrategy? strategy,
        ColorReference primaryColor,
        ColorReference secondaryColor,
        string bannerCode,
        string shieldCode)
    {
        Strategy = strategy;
        PrimaryColorReference = primaryColor;
        SecondaryColorReference = secondaryColor;
        BannerCode = bannerCode;
        ShieldCode = shieldCode;
    }

    public ColorStrategy? Strategy { get; }

    public ColorReference PrimaryColorReference { get; }

    public ColorReference SecondaryColorReference { get; }

    public uint? PrimaryColor => PrimaryColorReference == null ? null : PrimaryColorReference.Argb;

    public uint? SecondaryColor => SecondaryColorReference == null ? null : SecondaryColorReference.Argb;

    public string BannerCode { get; }

    public string ShieldCode { get; }
}
