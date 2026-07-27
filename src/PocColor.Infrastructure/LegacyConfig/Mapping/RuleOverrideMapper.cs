using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig.Mapping;

public sealed class RuleOverrideMapper
{
    private readonly LegacyValueParser _parser;

    public RuleOverrideMapper() : this(new LegacyValueParser())
    {
    }

    internal RuleOverrideMapper(LegacyValueParser parser)
    {
        _parser = parser;
    }

    public RuleOverride MapDefault(
        PocColorModConfigColorEntry entry,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        int? mode = entry == null ? 2 : entry.mode ?? 2;
        return new RuleOverride(
            strategy: _parser.ParseStrategy(mode, ScopeKey.Default, diagnostics),
            primaryColorReferences: _parser.ParseColors(entry == null ? null : entry.color, ScopeKey.Default, "color", diagnostics),
            secondaryColorReferences: _parser.ParseColors(entry == null ? null : entry.color2, ScopeKey.Default, "color2", diagnostics),
            bearRulerColors: _parser.ParseBoolean(entry == null ? null : entry.BearRulerColors, ScopeKey.Default, "BearRulerColors", diagnostics),
            bearRulerBanner: _parser.ParseBoolean(entry == null ? null : entry.BearRulerBanner, ScopeKey.Default, "BearRulerBanner", diagnostics),
            bearRulerShield: _parser.ParseBoolean(entry == null ? null : entry.BearRulerShield, ScopeKey.Default, "BearRulerShield", diagnostics),
            variableGroups: _parser.ParseVariables(entry == null ? null : entry.vars));
    }

    public RuleOverride Map(
        ConfigSection section,
        ScopeKey scope,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        return new RuleOverride(
            strategy: _parser.ParseStrategy(Value<int?>(section, "mode"), scope, diagnostics),
            primaryColorReferences: _parser.ParseColors(Value<string[]>(section, "color"), scope, "color", diagnostics),
            secondaryColorReferences: _parser.ParseColors(Value<string[]>(section, "color2"), scope, "color2", diagnostics),
            bannerCode: Value<string>(section, "banner"),
            bannerCodes: Value<string[]>(section, "banners"),
            shieldCodes: Value<string[]>(section, "shields"),
            battleBannerCodes: Value<string[]>(section, "combatBanners"),
            battleShieldCodes: Value<string[]>(section, "combatShields"),
            bearRulerColors: _parser.ParseBoolean(Value<string>(section, "BearRulerColors"), scope, "BearRulerColors", diagnostics),
            bearRulerBanner: _parser.ParseBoolean(Value<string>(section, "BearRulerBanner"), scope, "BearRulerBanner", diagnostics),
            bearRulerShield: _parser.ParseBoolean(Value<string>(section, "BearRulerShield"), scope, "BearRulerShield", diagnostics),
            rulerStrategy: _parser.ParseStrategy(Value<int?>(section, "rulerMode"), scope, diagnostics),
            rulerPrimaryColorReferences: _parser.ParseColors(Value<string[]>(section, "rulerColor"), scope, "rulerColor", diagnostics),
            rulerSecondaryColorReferences: _parser.ParseColors(Value<string[]>(section, "rulerColor2"), scope, "rulerColor2", diagnostics),
            rulerBannerCode: Value<string>(section, "rulerBanner"),
            rulerShieldCode: Value<string>(section, "rulerShield"),
            variableGroups: _parser.ParseVariables(Value<Map<string, string[]>[]>(section, "vars")));
    }

    private static T Value<T>(ConfigSection section, string propertyName)
    {
        object value = section.GetType().GetProperty(propertyName).GetValue(section, null);
        return value == null ? default : (T)value;
    }
}
