using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;

namespace PocColor.Domain.Appearance.Resolution;

public sealed class ResolveScopedRuleUtil : IResolveScopedRuleUtil
{
    public RuleOverride Resolve(AppearanceRuleSet rules, UnitProfile profile)
    {
        if (rules == null)
        {
            return RuleOverride.Empty;
        }

        if (profile == null)
        {
            return rules.Get(ScopeKey.Default);
        }

        return ResolveRule(rules, profile);
    }

    private static RuleOverride ResolveRule(AppearanceRuleSet rules, UnitProfile profile)
    {
        RuleOverride result = rules.Get(ScopeKey.Default);

        bool hasKingdom = TryResolveKingdom(rules, profile, out string kingdomName);
        bool hasClan = TryResolveClan(rules, profile, out string clanName);
        string kingdomClanName = null;
        bool hasKingdomClan = hasKingdom && TryResolveKingdomClan(rules, kingdomName, profile, out kingdomClanName);

        if (hasKingdom)
        {
            result = result.Merge(rules.Get(ScopeKey.ForKingdom(kingdomName)));
        }

        if (hasClan)
        {
            result = result.Merge(rules.Get(ScopeKey.ForClan(clanName)));
        }

        if (hasKingdomClan)
        {
            result = result.Merge(rules.Get(ScopeKey.ForKingdomClan(kingdomName, kingdomClanName)));
        }

        result = ApplyUnitScope(rules, result, profile, null, null);
        if (hasKingdom)
        {
            result = ApplyUnitScope(rules, result, profile, kingdomName, null);
        }

        if (hasClan)
        {
            result = ApplyUnitScope(rules, result, profile, null, clanName);
        }

        if (hasKingdomClan)
        {
            result = ApplyUnitScope(rules, result, profile, kingdomName, kingdomClanName);
        }

        return result;
    }

    private static RuleOverride ApplyUnitScope(
        AppearanceRuleSet rules,
        RuleOverride result,
        UnitProfile profile,
        string kingdomName,
        string clanName)
    {
        foreach (string unitName in UnitCandidates(profile))
        {
            ScopeKey scope = Scope(kingdomName, clanName, unitName);
            if (!rules.HasScope(scope))
            {
                continue;
            }

            return ApplyCriteria(rules, result, scope, profile);
        }

        return result;
    }

    private static RuleOverride ApplyCriteria(
        AppearanceRuleSet rules,
        RuleOverride result,
        ScopeKey scope,
        UnitProfile profile)
    {
        result = result.Merge(rules.Get(scope));
        if (!string.IsNullOrEmpty(profile.Culture))
        {
            result = result.Merge(rules.Get(scope.WithCriteria(culture: profile.Culture)));
        }

        result = ApplyTypeCriteria(rules, result, scope, profile);
        result = result.Merge(rules.Get(scope.WithCriteria(tier: profile.Tier)));

        if (!string.IsNullOrEmpty(profile.Culture))
        {
            ScopeKey cultureScope = scope.WithCriteria(culture: profile.Culture);
            result = ApplyTypeCriteria(rules, result, cultureScope, profile);
            result = result.Merge(rules.Get(cultureScope.WithCriteria(tier: profile.Tier)));
        }

        ScopeKey tierScope = scope.WithCriteria(tier: profile.Tier);
        result = ApplyTypeCriteria(rules, result, tierScope, profile);
        if (!string.IsNullOrEmpty(profile.Culture))
        {
            result = ApplyTypeCriteria(rules, result, tierScope.WithCriteria(culture: profile.Culture), profile);
        }

        return result;
    }

    private static RuleOverride ApplyTypeCriteria(
        AppearanceRuleSet rules,
        RuleOverride result,
        ScopeKey scope,
        UnitProfile profile)
    {
        result = result.Merge(rules.Get(scope.WithCriteria(isMounted: profile.IsMounted)));
        result = result.Merge(rules.Get(scope.WithCriteria(isRanged: profile.IsRanged)));
        return result.Merge(rules.Get(scope.WithCriteria(isMounted: profile.IsMounted, isRanged: profile.IsRanged)));
    }

    private static bool TryResolveKingdom(AppearanceRuleSet rules, UnitProfile profile, out string kingdomName)
    {
        kingdomName = profile.KingdomName;
        if (HasText(kingdomName) && rules.HasKingdomBranch(kingdomName))
        {
            return true;
        }

        if (profile.IsPlayerKingdom && rules.HasKingdomBranch("PlayerKingdom"))
        {
            kingdomName = "PlayerKingdom";
            return true;
        }

        return false;
    }

    private static bool TryResolveClan(AppearanceRuleSet rules, UnitProfile profile, out string clanName)
    {
        clanName = profile.ClanName;
        if (HasText(clanName) && rules.HasClanBranch(clanName))
        {
            return true;
        }

        if (profile.IsPlayerClan && rules.HasClanBranch("PlayerClan"))
        {
            clanName = "PlayerClan";
            return true;
        }

        return false;
    }

    private static bool TryResolveKingdomClan(
        AppearanceRuleSet rules,
        string kingdomName,
        UnitProfile profile,
        out string clanName)
    {
        clanName = profile.ClanName;
        if (HasText(clanName) && rules.HasKingdomClanBranch(kingdomName, clanName))
        {
            return true;
        }

        if (profile.IsPlayerClan && rules.HasKingdomClanBranch(kingdomName, "PlayerClan"))
        {
            clanName = "PlayerClan";
            return true;
        }

        return false;
    }

    private static IEnumerable<string> UnitCandidates(UnitProfile profile)
    {
        if (HasText(profile.UnitName))
        {
            yield return profile.UnitName;
        }

        if (profile.IsPlayer)
        {
            yield return "Player";
            yield break;
        }

        if (profile.IsKing)
        {
            yield return "King";
            yield break;
        }

        if (profile.IsLeader)
        {
            yield return "Leader";
            yield break;
        }

        if (profile.IsHero)
        {
            yield return "Hero";
            yield break;
        }

        yield return "Trooper";
    }

    private static ScopeKey Scope(string kingdomName, string clanName, string unitName)
    {
        if (kingdomName == null && clanName == null)
        {
            return ScopeKey.ForUnit(unitName);
        }

        if (kingdomName == null)
        {
            return ScopeKey.ForClanUnit(clanName, unitName);
        }

        if (clanName == null)
        {
            return ScopeKey.ForKingdomUnit(kingdomName, unitName);
        }

        return ScopeKey.ForKingdomClanUnit(kingdomName, clanName, unitName);
    }

    private static bool HasText(string value) => !string.IsNullOrEmpty(value);
}
