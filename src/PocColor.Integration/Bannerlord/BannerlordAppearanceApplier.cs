using PocColor.Domain.Appearance.Model;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public sealed class BannerlordAppearanceApplier
{
    public uint? ApplyPrimary(ResolvedAppearance appearance)
    {
        return Resolve(appearance?.PrimaryColorReference);
    }

    public uint? ApplySecondary(ResolvedAppearance appearance)
    {
        return Resolve(appearance?.SecondaryColorReference);
    }

    public static uint? Resolve(ColorReference color)
    {
        if (color == null)
        {
            return null;
        }

        return color.Kind == ColorReferenceKind.PaletteId && color.PaletteId.HasValue
            ? BannerManager.GetColor(color.PaletteId.Value)
            : color.Argb;
    }
}
