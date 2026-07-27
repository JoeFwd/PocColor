using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;

namespace PocColor.Domain.Appearance;

public interface IResolveAppearanceUseCase
{
    ResolvedAppearance Resolve(
        AppearanceRuleSet rules,
        UnitProfile profile,
        BannerSnapshot banner,
        ResolutionKind resolutionKind,
        IChoiceSource choices);

    ResolvedAppearance Resolve(
        AppearanceRuleSet rules,
        UnitProfile profile,
        BannerSnapshot banner,
        ResolutionKind resolutionKind,
        IChoiceSource choices,
        AppearancePalette palette);
}
