using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig.Mapping;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig;

public sealed class LegacyConfigCompiler
{
    private readonly RuleOverrideMapper _ruleMapper;
    private readonly CampaignPolicyCompiler _campaignPolicyCompiler;

    public LegacyConfigCompiler()
    {
        var parser = new LegacyValueParser();
        _ruleMapper = new RuleOverrideMapper(parser);
        _campaignPolicyCompiler = new CampaignPolicyCompiler(new CampaignBannerPolicyMapper(parser));
    }

    public LegacyConfigCompileResult Compile(PocColorModConfig config)
    {
        var diagnostics = new List<LegacyConfigDiagnostic>();
        var rules = new Dictionary<ScopeKey, RuleOverride>();

        if (config == null)
        {
            return new LegacyConfigCompileResult(
                new AppearanceRuleSet(rules, new Dictionary<ScopeKey, CampaignBannerPolicyOverride>()),
                diagnostics);
        }

        AddRule(
            rules,
            ScopeKey.Default,
            _ruleMapper.MapDefault(config.defaultConfig, diagnostics));

        CompileUnits(rules, ScopeKey.ForUnit, config.groups, config.units, diagnostics);
        CompileClans(rules, config.clans, diagnostics);
        CompileKingdoms(rules, config.kingdoms, diagnostics);

        Dictionary<ScopeKey, CampaignBannerPolicyOverride> campaignPolicies =
            _campaignPolicyCompiler.Compile(config, diagnostics);

        return new LegacyConfigCompileResult(new AppearanceRuleSet(rules, campaignPolicies), diagnostics);
    }

    private void CompileKingdoms(
        IDictionary<ScopeKey, RuleOverride> rules,
        Map<string, PocColorModConfigKingdom> kingdoms,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (kingdoms == null)
        {
            return;
        }

        foreach (KeyValuePair<string, PocColorModConfigKingdom> entry in kingdoms)
        {
            if (entry.Value == null)
            {
                continue;
            }

            ScopeKey kingdomScope = ScopeKey.ForKingdom(entry.Key);
            AddRule(rules, kingdomScope, _ruleMapper.Map(entry.Value, kingdomScope, diagnostics));
            CompileUnits(rules, unitName => ScopeKey.ForKingdomUnit(entry.Key, unitName), entry.Value.groups, entry.Value.units, diagnostics);

            if (entry.Value.clans == null)
            {
                continue;
            }

            foreach (KeyValuePair<string, PocColorModConfigClan> clanEntry in entry.Value.clans)
            {
                if (clanEntry.Value == null)
                {
                    continue;
                }

                ScopeKey clanScope = ScopeKey.ForKingdomClan(entry.Key, clanEntry.Key);
                AddRule(rules, clanScope, _ruleMapper.Map(clanEntry.Value, clanScope, diagnostics));
                CompileUnits(
                    rules,
                    unitName => ScopeKey.ForKingdomClanUnit(entry.Key, clanEntry.Key, unitName),
                    clanEntry.Value.groups,
                    clanEntry.Value.units,
                    diagnostics);
            }
        }
    }

    private void CompileClans(
        IDictionary<ScopeKey, RuleOverride> rules,
        Map<string, PocColorModConfigClan> clans,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (clans == null)
        {
            return;
        }

        foreach (KeyValuePair<string, PocColorModConfigClan> entry in clans)
        {
            if (entry.Value == null)
            {
                continue;
            }

            ScopeKey clanScope = ScopeKey.ForClan(entry.Key);
            AddRule(rules, clanScope, _ruleMapper.Map(entry.Value, clanScope, diagnostics));
            CompileUnits(rules, unitName => ScopeKey.ForClanUnit(entry.Key, unitName), entry.Value.groups, entry.Value.units, diagnostics);
        }
    }

    private void CompileUnits(
        IDictionary<ScopeKey, RuleOverride> rules,
        Func<string, ScopeKey> createScope,
        PocColorModGroupEntry[] groups,
        Map<string, PocColorModConfigUnit> explicitUnits,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        foreach (KeyValuePair<string, PocColorModConfigUnit> entry in GroupUnitExpander.Expand(groups, explicitUnits))
        {
            ScopeKey scope = createScope(entry.Key);
            CompileUnit(rules, scope, entry.Value, diagnostics);
        }
    }

    private void CompileUnit(
        IDictionary<ScopeKey, RuleOverride> rules,
        ScopeKey scope,
        PocColorModConfigUnit unit,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (unit == null)
        {
            return;
        }

        AddRule(rules, scope, _ruleMapper.Map(unit, scope, diagnostics));
        CompileTypes(rules, scope, unit.types, diagnostics);
        CompileTiers(rules, scope, unit.tiers, diagnostics);

        if (unit.cultures == null)
        {
            return;
        }

        foreach (KeyValuePair<string, ConfigSectionCulture> cultureEntry in unit.cultures)
        {
            if (cultureEntry.Value == null)
            {
                continue;
            }

            ScopeKey cultureScope = scope.WithCriteria(culture: cultureEntry.Key);
            AddRule(rules, cultureScope, _ruleMapper.Map(cultureEntry.Value, cultureScope, diagnostics));
            CompileTypes(rules, cultureScope, cultureEntry.Value.types, diagnostics);
            CompileTiers(rules, cultureScope, cultureEntry.Value.tiers, diagnostics);
        }
    }

    private void CompileTiers(
        IDictionary<ScopeKey, RuleOverride> rules,
        ScopeKey baseScope,
        Map<int, ConfigSectionTier> tiers,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (tiers == null)
        {
            return;
        }

        foreach (KeyValuePair<int, ConfigSectionTier> tierEntry in tiers)
        {
            if (tierEntry.Value == null)
            {
                continue;
            }

            ScopeKey tierScope = baseScope.WithCriteria(tier: tierEntry.Key);
            AddRule(rules, tierScope, _ruleMapper.Map(tierEntry.Value, tierScope, diagnostics));
            CompileTypes(rules, tierScope, tierEntry.Value.types, diagnostics);
        }
    }

    private void CompileTypes(
        IDictionary<ScopeKey, RuleOverride> rules,
        ScopeKey baseScope,
        Map<string, ConfigSection> types,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (types == null)
        {
            return;
        }

        CompileType(rules, baseScope, types["Mounted"], isMounted: true, isRanged: null, diagnostics);
        CompileType(rules, baseScope, types["Foot"], isMounted: false, isRanged: null, diagnostics);
        CompileType(rules, baseScope, types["Ranged"], isMounted: null, isRanged: true, diagnostics);
        CompileType(rules, baseScope, types["Melee"], isMounted: null, isRanged: false, diagnostics);
        CompileType(rules, baseScope, types["HorseArcher"], isMounted: true, isRanged: true, diagnostics);
        CompileType(rules, baseScope, types["Archer"], isMounted: false, isRanged: true, diagnostics);
        CompileType(rules, baseScope, types["Cavalry"], isMounted: true, isRanged: false, diagnostics);
        CompileType(rules, baseScope, types["Infantry"], isMounted: false, isRanged: false, diagnostics);
    }

    private void CompileType(
        IDictionary<ScopeKey, RuleOverride> rules,
        ScopeKey baseScope,
        ConfigSection section,
        bool? isMounted,
        bool? isRanged,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        if (section == null)
        {
            return;
        }

        ScopeKey scope = baseScope.WithCriteria(isMounted: isMounted, isRanged: isRanged);
        AddRule(rules, scope, _ruleMapper.Map(section, scope, diagnostics));
    }

    private static void AddRule(
        IDictionary<ScopeKey, RuleOverride> rules,
        ScopeKey scope,
        RuleOverride rule)
    {
        rules[scope] = rule ?? RuleOverride.Empty;
    }
}
