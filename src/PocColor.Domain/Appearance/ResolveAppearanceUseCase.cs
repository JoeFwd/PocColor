using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;
using PocColor.Domain.Appearance.Resolution;

namespace PocColor.Domain.Appearance;

public sealed class ResolveAppearanceUseCase : IResolveAppearanceUseCase
{
    private readonly IResolveScopedRuleUtil _scopedRule;
    private readonly IResolveColorsUtil _colors;

    public ResolveAppearanceUseCase() : this(new ResolveScopedRuleUtil(), new ResolveColorsUtil())
    {
    }

    public ResolveAppearanceUseCase(IResolveScopedRuleUtil scopedRule, IResolveColorsUtil colors)
    {
        _scopedRule = scopedRule ?? throw new ArgumentNullException(nameof(scopedRule));
        _colors = colors ?? throw new ArgumentNullException(nameof(colors));
    }

    public ResolvedAppearance Resolve(
        AppearanceRuleSet rules,
        UnitProfile profile,
        BannerSnapshot banner,
        ResolutionKind resolutionKind,
        IChoiceSource choices)
    {
        return Resolve(rules, profile, banner, resolutionKind, choices, AppearancePalette.Empty);
    }

    public ResolvedAppearance Resolve(
        AppearanceRuleSet rules,
        UnitProfile profile,
        BannerSnapshot banner,
        ResolutionKind resolutionKind,
        IChoiceSource choices,
        AppearancePalette palette)
    {
        if (choices == null)
        {
            throw new ArgumentNullException(nameof(choices));
        }

        if (rules == null || profile == null)
        {
            return new ResolvedAppearance(null, null, null, null, null);
        }

        banner = banner ?? new BannerSnapshot(null, Array.Empty<ColorReference>());
        palette = palette ?? AppearancePalette.Empty;
        RuleOverride resolvedRule = _scopedRule.Resolve(rules, profile);

        ColorStrategy? strategy = resolvedRule.Strategy;
        IReadOnlyList<ColorReference> primaryColors = resolvedRule.PrimaryColors;
        IReadOnlyList<ColorReference> secondaryColors = resolvedRule.SecondaryColors;
        IReadOnlyList<string> bannerCodes = BuildCandidates(resolvedRule.BannerCodes, resolvedRule.BannerCode);
        IReadOnlyList<string> shieldCodes = BuildCandidates(resolvedRule.ShieldCodes, resolvedRule.ShieldCode);

        if (!string.IsNullOrEmpty(profile.RulerBannerCode))
        {
            if (resolvedRule.BearRulerColors != false)
            {
                strategy = resolvedRule.RulerStrategy ?? ColorStrategy.BannerFirstTwo;
                primaryColors = resolvedRule.RulerPrimaryColors;
                secondaryColors = resolvedRule.RulerSecondaryColors;
            }

            if (resolvedRule.BearRulerBanner != false)
            {
                bannerCodes = new[] { resolvedRule.RulerBannerCode ?? profile.RulerBannerCode };
            }

            if (resolvedRule.BearRulerShield != false)
            {
                shieldCodes = new[] { resolvedRule.RulerShieldCode ?? profile.RulerBannerCode };
            }
        }

        if (resolutionKind == ResolutionKind.Battle)
        {
            if (HasValues(resolvedRule.BattleBannerCodes))
            {
                bannerCodes = resolvedRule.BattleBannerCodes;
            }

            if (HasValues(resolvedRule.BattleShieldCodes))
            {
                shieldCodes = resolvedRule.BattleShieldCodes;
            }
        }

        IReadOnlyDictionary<string, string> variables = ResolveVariables(resolvedRule.VariableGroups, choices);
        string selectedBanner = Choose(Expand(bannerCodes, variables), choices);
        string selectedShield = Choose(Expand(shieldCodes, variables), choices);
        BannerSnapshot colorBanner = palette.GetBanner(selectedShield)
            ?? palette.GetBanner(selectedBanner)
            ?? banner;
        (ColorReference primary, ColorReference secondary) = _colors.Resolve(
            strategy,
            primaryColors,
            secondaryColors,
            colorBanner.Colors,
            palette.DefaultColors,
            choices);

        return new ResolvedAppearance(strategy, primary, secondary, selectedBanner, selectedShield);
    }

    private static IReadOnlyList<string> BuildCandidates(IReadOnlyList<string> values, string singleValue)
    {
        if (!HasValues(values))
        {
            return HasText(singleValue) ? new[] { singleValue } : null;
        }

        if (!HasText(singleValue))
        {
            return values;
        }

        var combined = new string[values.Count + 1];
        for (int index = 0; index < values.Count; index++)
        {
            combined[index] = values[index];
        }

        combined[values.Count] = singleValue;
        return combined;
    }

    private static IReadOnlyDictionary<string, string> ResolveVariables(
        IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> groups,
        IChoiceSource choices)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (groups == null)
        {
            return values;
        }

        foreach (IReadOnlyDictionary<string, IReadOnlyList<string>> group in groups)
        {
            int minimum = int.MaxValue;
            if (group != null)
            {
                foreach (KeyValuePair<string, IReadOnlyList<string>> entry in group)
                {
                    if (entry.Value != null)
                    {
                        minimum = Math.Min(minimum, entry.Value.Count);
                    }
                }
            }

            if (minimum <= 0 || minimum == int.MaxValue)
            {
                continue;
            }

            int index = choices.NextIndex(minimum);
            foreach (KeyValuePair<string, IReadOnlyList<string>> entry in group)
            {
                if (entry.Value != null && entry.Value.Count > index)
                {
                    values[entry.Key] = entry.Value[index];
                }
            }
        }

        return values;
    }

    private static IReadOnlyList<string> Expand(
        IReadOnlyList<string> candidates,
        IReadOnlyDictionary<string, string> variables)
    {
        if (candidates == null || variables == null || variables.Count == 0)
        {
            return candidates;
        }

        var expanded = new string[candidates.Count];
        for (int index = 0; index < candidates.Count; index++)
        {
            string value = candidates[index];
            if (value != null)
            {
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    value = value.Replace(variable.Key, variable.Value);
                }
            }

            expanded[index] = value;
        }

        return expanded;
    }

    private static string Choose(IReadOnlyList<string> candidates, IChoiceSource choices)
    {
        return HasValues(candidates) ? candidates[choices.NextIndex(candidates.Count)] : null;
    }

    private static bool HasValues<T>(IReadOnlyList<T> values) => values != null && values.Count > 0;

    private static bool HasText(string value) => !string.IsNullOrEmpty(value);
}
