using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig.Mapping;

public sealed class LegacyValueParser
{
    public ColorStrategy? ParseStrategy(
        int? mode,
        ScopeKey scope,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (!mode.HasValue)
        {
            return null;
        }

        switch (mode.Value)
        {
            case 1:
                return ColorStrategy.RandomConfigured;
            case 11:
                return ColorStrategy.RandomPaired;
            case 3:
                return ColorStrategy.FixedPair;
            case 2:
            case 4:
                return ColorStrategy.BannerFirstTwo;
            case 21:
            case 41:
                return ColorStrategy.BannerFirstTwoRandomOrder;
            case 22:
            case 42:
                return ColorStrategy.BannerRandomColors;
            default:
                diagnostics.Add(new LegacyConfigDiagnostic(
                    "UnknownMode",
                    "The V1 color mode '" + mode.Value.ToString(CultureInfo.InvariantCulture)
                        + "' is not supported at " + Describe(scope) + "."));
                return ColorStrategy.Unresolved;
        }
    }

    public IReadOnlyList<ColorReference> ParseColors(
        string[] values,
        ScopeKey scope,
        string property,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (values == null)
        {
            return null;
        }

        var colors = new List<ColorReference>(values.Length);
        foreach (string value in values)
        {
            if (TryParseColor(value, out ColorReference color))
            {
                colors.Add(color);
            }
            else
            {
                diagnostics.Add(new LegacyConfigDiagnostic(
                    "InvalidColor",
                    "The V1 " + property + " value '" + value + "' is invalid at " + Describe(scope) + "."));
            }
        }

        return new ReadOnlyCollection<ColorReference>(colors);
    }

    public ColorReference ParseCampaignColor(
        string value,
        ScopeKey scope,
        string property,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (TryParseColor(value, out ColorReference color))
        {
            return color;
        }

        diagnostics.Add(new LegacyConfigDiagnostic(
            "InvalidColor",
            "The V1 " + property + " value '" + value + "' is invalid at " + Describe(scope) + "."));
        return null;
    }

    public bool? ParseBoolean(
        string value,
        ScopeKey scope,
        string property,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (value == null)
        {
            return null;
        }

        if (bool.TryParse(value, out bool parsed))
        {
            return parsed;
        }

        diagnostics.Add(new LegacyConfigDiagnostic(
            "InvalidBoolean",
            "The V1 " + property + " value '" + value + "' is invalid at " + Describe(scope) + "."));
        return false;
    }

    public IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> ParseVariables(
        Map<string, string[]>[] groups)
    {
        if (groups == null)
        {
            return null;
        }

        var compiledGroups = new List<IReadOnlyDictionary<string, IReadOnlyList<string>>>(groups.Length);
        foreach (Map<string, string[]> group in groups)
        {
            var compiledGroup = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            if (group != null)
            {
                foreach (KeyValuePair<string, string[]> entry in group)
                {
                    compiledGroup[entry.Key] = entry.Value == null
                        ? null
                        : new ReadOnlyCollection<string>((string[])entry.Value.Clone());
                }
            }

            compiledGroups.Add(new ReadOnlyDictionary<string, IReadOnlyList<string>>(compiledGroup));
        }

        return new ReadOnlyCollection<IReadOnlyDictionary<string, IReadOnlyList<string>>>(compiledGroups);
    }

    public string Describe(ScopeKey scope)
    {
        if (scope == null || scope.Equals(ScopeKey.Default))
        {
            return "the default scope";
        }

        return "scope '"
            + (scope.KingdomName ?? "*") + "/"
            + (scope.ClanName ?? "*") + "/"
            + (scope.UnitName ?? "*") + "'";
    }

    private static bool TryParseColor(string value, out ColorReference color)
    {
        color = ColorReference.FromArgb(uint.MaxValue);
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        string trimmed = value.Trim();
        if (trimmed.StartsWith("#", StringComparison.Ordinal))
        {
            string hex = trimmed.Substring(1);
            if (hex.Length == 6)
            {
                if (uint.TryParse("FF" + hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb))
                {
                    color = ColorReference.FromArgb(argb);
                    return true;
                }

                return false;
            }

            if (hex.Length == 8)
            {
                if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb))
                {
                    color = ColorReference.FromArgb(argb);
                    return true;
                }

                return false;
            }

            return false;
        }

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int paletteId))
        {
            color = ColorReference.FromPaletteId(paletteId);
            return true;
        }

        return false;
    }
}
