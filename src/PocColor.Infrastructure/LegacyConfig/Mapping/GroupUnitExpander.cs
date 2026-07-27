using System;
using System.Collections.Generic;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig.Mapping;

public static class GroupUnitExpander
{
    public static IEnumerable<KeyValuePair<string, PocColorModConfigUnit>> Expand(
        PocColorModGroupEntry[] groups,
        Map<string, PocColorModConfigUnit> explicitUnits)
    {
        var expanded = new Dictionary<string, PocColorModConfigUnit>(StringComparer.Ordinal);
        if (groups != null)
        {
            foreach (PocColorModGroupEntry group in groups)
            {
                if (group == null || group.units == null)
                {
                    continue;
                }

                PocColorModConfigUnit compiledGroup = CopyGroup(group);
                foreach (string unitName in group.units)
                {
                    if (unitName != null && !expanded.ContainsKey(unitName))
                    {
                        expanded.Add(unitName, compiledGroup);
                    }
                }
            }
        }

        if (explicitUnits != null)
        {
            foreach (KeyValuePair<string, PocColorModConfigUnit> entry in explicitUnits)
            {
                expanded[entry.Key] = entry.Value;
            }
        }

        return expanded;
    }

    private static PocColorModConfigUnit CopyGroup(PocColorModGroupEntry group)
    {
        return new PocColorModConfigUnit
        {
            mode = group.mode,
            color = group.color,
            color2 = group.color2,
            banner = group.banner,
            banners = group.banners,
            shields = group.shields,
            combatBanners = group.combatBanners,
            combatShields = group.combatShields,
            BearRulerColors = group.BearRulerColors,
            BearRulerBanner = group.BearRulerBanner,
            BearRulerShield = group.BearRulerShield,
            rulerMode = group.rulerMode,
            rulerColor = group.rulerColor,
            rulerColor2 = group.rulerColor2,
            rulerBanner = group.rulerBanner,
            rulerShield = group.rulerShield,
            vars = group.vars,
            cultures = group.cultures,
            tiers = group.tiers,
            types = group.types
        };
    }
}
