using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;

namespace PocColor.Domain.Appearance.Resolution;

public interface IResolveColorsUtil
{
    (ColorReference primary, ColorReference secondary) Resolve(
        ColorStrategy? strategy,
        IReadOnlyList<ColorReference> primaryColors,
        IReadOnlyList<ColorReference> secondaryColors,
        IReadOnlyList<ColorReference> bannerColors,
        IReadOnlyList<ColorReference> defaultColors,
        IChoiceSource choices);
}
