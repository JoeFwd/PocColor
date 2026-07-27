using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;

namespace PocColor.Domain.Appearance.Resolution;

public sealed class ResolveColorsUtil : IResolveColorsUtil
{
    public (ColorReference primary, ColorReference secondary) Resolve(
        ColorStrategy? strategy,
        IReadOnlyList<ColorReference> primaryColors,
        IReadOnlyList<ColorReference> secondaryColors,
        IReadOnlyList<ColorReference> bannerColors,
        IReadOnlyList<ColorReference> defaultColors,
        IChoiceSource choices)
    {
        if (!strategy.HasValue)
        {
            return (null, null);
        }

        switch (strategy.Value)
        {
            case ColorStrategy.RandomConfigured:
                return (
                    ChooseConfiguredOrDefault(primaryColors ?? secondaryColors, defaultColors, choices),
                    ChooseConfiguredOrDefault(secondaryColors ?? primaryColors, defaultColors, choices));
            case ColorStrategy.RandomPaired:
                if (!HasValues(primaryColors) && !HasValues(secondaryColors))
                {
                    ColorReference defaultColor = ChooseColor(defaultColors, choices);
                    return (defaultColor, defaultColor);
                }

                IReadOnlyList<ColorReference> pairedPrimary = primaryColors ?? secondaryColors;
                IReadOnlyList<ColorReference> pairedSecondary = secondaryColors ?? primaryColors;
                if (!HasValues(pairedPrimary) || !HasValues(pairedSecondary))
                {
                    return (null, null);
                }

                int pairedIndex = choices.NextIndex(Math.Min(pairedPrimary.Count, pairedSecondary.Count));
                return (pairedPrimary[pairedIndex], pairedSecondary[pairedIndex]);
            case ColorStrategy.FixedPair:
                return HasValues(primaryColors) && primaryColors.Count >= 2
                    ? (primaryColors[0], primaryColors[1])
                    : (null, null);
            case ColorStrategy.BannerFirstTwo:
                return HasValues(bannerColors) && bannerColors.Count >= 2
                    ? (bannerColors[0], bannerColors[1])
                    : (null, null);
            case ColorStrategy.BannerFirstTwoRandomOrder:
                if (!HasValues(bannerColors) || bannerColors.Count < 2)
                {
                    return (null, null);
                }

                return choices.NextIndex(2) == 0
                    ? (bannerColors[0], bannerColors[1])
                    : (bannerColors[1], bannerColors[0]);
            case ColorStrategy.BannerRandomColors:
                return (
                    ChooseColor(bannerColors, choices),
                    ChooseColor(bannerColors, choices));
            default:
                return (null, null);
        }
    }

    private static ColorReference ChooseColor(IReadOnlyList<ColorReference> colors, IChoiceSource choices)
    {
        return HasValues(colors) ? colors[choices.NextIndex(colors.Count)] : null;
    }

    private static ColorReference ChooseConfiguredOrDefault(
        IReadOnlyList<ColorReference> configuredColors,
        IReadOnlyList<ColorReference> defaultColors,
        IChoiceSource choices)
    {
        return ChooseColor(configuredColors, choices) ?? ChooseColor(defaultColors, choices);
    }

    private static bool HasValues<T>(IReadOnlyList<T> values) => values != null && values.Count > 0;
}
