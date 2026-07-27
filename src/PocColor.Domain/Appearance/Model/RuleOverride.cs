using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PocColor.Domain.Appearance.Model;

public sealed class RuleOverride
{
    public static RuleOverride Empty { get; } = new RuleOverride();

    public RuleOverride(
        ColorStrategy? strategy = null,
        IReadOnlyList<uint> primaryColors = null,
        IReadOnlyList<uint> secondaryColors = null,
        string bannerCode = null,
        IReadOnlyList<string> bannerCodes = null,
        string shieldCode = null,
        IReadOnlyList<string> shieldCodes = null,
        IReadOnlyList<string> battleBannerCodes = null,
        IReadOnlyList<string> battleShieldCodes = null,
        bool? bearRulerColors = null,
        bool? bearRulerBanner = null,
        bool? bearRulerShield = null,
        ColorStrategy? rulerStrategy = null,
        IReadOnlyList<uint> rulerPrimaryColors = null,
        IReadOnlyList<uint> rulerSecondaryColors = null,
        string rulerBannerCode = null,
        string rulerShieldCode = null,
        IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> variableGroups = null,
        IReadOnlyList<ColorReference> primaryColorReferences = null,
        IReadOnlyList<ColorReference> secondaryColorReferences = null,
        IReadOnlyList<ColorReference> rulerPrimaryColorReferences = null,
        IReadOnlyList<ColorReference> rulerSecondaryColorReferences = null)
    {
        Strategy = strategy;
        PrimaryColors = Copy(primaryColorReferences ?? ToReferences(primaryColors));
        SecondaryColors = Copy(secondaryColorReferences ?? ToReferences(secondaryColors));
        BannerCode = bannerCode;
        BannerCodes = Copy(bannerCodes);
        ShieldCode = shieldCode;
        ShieldCodes = Copy(shieldCodes);
        BattleBannerCodes = Copy(battleBannerCodes);
        BattleShieldCodes = Copy(battleShieldCodes);
        BearRulerColors = bearRulerColors;
        BearRulerBanner = bearRulerBanner;
        BearRulerShield = bearRulerShield;
        RulerStrategy = rulerStrategy;
        RulerPrimaryColors = Copy(rulerPrimaryColorReferences ?? ToReferences(rulerPrimaryColors));
        RulerSecondaryColors = Copy(rulerSecondaryColorReferences ?? ToReferences(rulerSecondaryColors));
        RulerBannerCode = rulerBannerCode;
        RulerShieldCode = rulerShieldCode;
        VariableGroups = CopyVariableGroups(variableGroups);
    }

    public ColorStrategy? Strategy { get; }

    public IReadOnlyList<ColorReference> PrimaryColors { get; }

    public IReadOnlyList<ColorReference> SecondaryColors { get; }

    public string BannerCode { get; }

    public IReadOnlyList<string> BannerCodes { get; }

    public string ShieldCode { get; }

    public IReadOnlyList<string> ShieldCodes { get; }

    public IReadOnlyList<string> BattleBannerCodes { get; }

    public IReadOnlyList<string> BattleShieldCodes { get; }

    public bool? BearRulerColors { get; }

    public bool? BearRulerBanner { get; }

    public bool? BearRulerShield { get; }

    public ColorStrategy? RulerStrategy { get; }

    public IReadOnlyList<ColorReference> RulerPrimaryColors { get; }

    public IReadOnlyList<ColorReference> RulerSecondaryColors { get; }

    public string RulerBannerCode { get; }

    public string RulerShieldCode { get; }

    public IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> VariableGroups { get; }

    public static RuleOverride ForFixedPair(uint primary, uint secondary)
    {
        return new RuleOverride(
            strategy: ColorStrategy.FixedPair,
            primaryColors: new[] { primary, secondary });
    }

    internal RuleOverride Merge(RuleOverride later)
    {
        if (later == null || ReferenceEquals(later, Empty))
        {
            return this;
        }

        return new RuleOverride(
            strategy: later.Strategy ?? Strategy,
            primaryColorReferences: later.PrimaryColors ?? PrimaryColors,
            secondaryColorReferences: later.SecondaryColors ?? SecondaryColors,
            bannerCode: later.BannerCode ?? BannerCode,
            bannerCodes: later.BannerCodes ?? BannerCodes,
            shieldCode: later.ShieldCode ?? ShieldCode,
            shieldCodes: later.ShieldCodes ?? ShieldCodes,
            battleBannerCodes: later.BattleBannerCodes ?? BattleBannerCodes,
            battleShieldCodes: later.BattleShieldCodes ?? BattleShieldCodes,
            bearRulerColors: later.BearRulerColors ?? BearRulerColors,
            bearRulerBanner: later.BearRulerBanner ?? BearRulerBanner,
            bearRulerShield: later.BearRulerShield ?? BearRulerShield,
            rulerStrategy: later.RulerStrategy ?? RulerStrategy,
            rulerPrimaryColorReferences: later.RulerPrimaryColors ?? RulerPrimaryColors,
            rulerSecondaryColorReferences: later.RulerSecondaryColors ?? RulerSecondaryColors,
            rulerBannerCode: later.RulerBannerCode ?? RulerBannerCode,
            rulerShieldCode: later.RulerShieldCode ?? RulerShieldCode,
            variableGroups: later.VariableGroups ?? VariableGroups);
    }

    private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> values)
    {
        if (values == null)
        {
            return null;
        }

        var copy = new T[values.Count];
        for (int index = 0; index < values.Count; index++)
        {
            copy[index] = values[index];
        }

        return new ReadOnlyCollection<T>(copy);
    }

    private static IReadOnlyList<ColorReference> ToReferences(IReadOnlyList<uint> values)
    {
        if (values == null)
        {
            return null;
        }

        var references = new ColorReference[values.Count];
        for (int index = 0; index < values.Count; index++)
        {
            references[index] = ColorReference.FromArgb(values[index]);
        }

        return references;
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> CopyVariableGroups(
        IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> groups)
    {
        if (groups == null)
        {
            return null;
        }

        var copy = new IReadOnlyDictionary<string, IReadOnlyList<string>>[groups.Count];
        for (int index = 0; index < groups.Count; index++)
        {
            IReadOnlyDictionary<string, IReadOnlyList<string>> group = groups[index];
            var groupCopy = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            if (group != null)
            {
                foreach (KeyValuePair<string, IReadOnlyList<string>> entry in group)
                {
                    groupCopy[entry.Key] = Copy(entry.Value);
                }
            }

            copy[index] = new ReadOnlyDictionary<string, IReadOnlyList<string>>(groupCopy);
        }

        return new ReadOnlyCollection<IReadOnlyDictionary<string, IReadOnlyList<string>>>(copy);
    }
}
