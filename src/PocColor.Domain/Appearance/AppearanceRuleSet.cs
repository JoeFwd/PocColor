using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PocColor.Domain.Appearance.Model;

namespace PocColor.Domain.Appearance;

public sealed class AppearanceRuleSet
{
    private readonly IReadOnlyDictionary<ScopeKey, RuleOverride> _overrides;
    private readonly IReadOnlyDictionary<ScopeKey, CampaignBannerPolicyOverride> _campaignPolicies;

    public AppearanceRuleSet(
        IReadOnlyDictionary<ScopeKey, RuleOverride> overrides,
        IReadOnlyDictionary<ScopeKey, CampaignBannerPolicyOverride> campaignPolicies = null)
    {
        var copy = new Dictionary<ScopeKey, RuleOverride>();
        if (overrides != null)
        {
            foreach (KeyValuePair<ScopeKey, RuleOverride> entry in overrides)
            {
                if (entry.Key == null)
                {
                    throw new ArgumentException("A scope key cannot be null.", nameof(overrides));
                }

                copy.Add(entry.Key, entry.Value ?? RuleOverride.Empty);
            }
        }

        _overrides = new ReadOnlyDictionary<ScopeKey, RuleOverride>(copy);
        _campaignPolicies = CopyCampaignPolicies(campaignPolicies);
    }

    public RuleOverride Get(ScopeKey key)
    {
        return key != null && _overrides.TryGetValue(key, out RuleOverride rule)
            ? rule
            : RuleOverride.Empty;
    }

    public CampaignBannerPolicy GetCampaignBannerPolicy(
        string kingdomName,
        string clanName,
        bool isPlayerKingdom,
        bool isPlayerClan,
        string cachedBannerCode)
    {
        CampaignBannerPolicyOverride policy = GetCampaignPolicy(ScopeKey.Default);
        ScopeKey kingdomScope = FindKingdomScope(kingdomName, isPlayerKingdom);
        ScopeKey clanScope = FindClanScope(clanName, isPlayerClan);
        ScopeKey kingdomClanScope = FindKingdomClanScope(kingdomScope, clanName, isPlayerClan);

        policy = policy.Merge(GetCampaignPolicy(kingdomScope));
        policy = policy.Merge(GetCampaignPolicy(clanScope));
        policy = policy.Merge(GetCampaignPolicy(kingdomClanScope));

        return new CampaignBannerPolicy(
            policy.FollowKingdomColors ?? policy.ConfiguredBannerCode == null,
            policy.FollowKingdomBackgroundOnly ?? false,
            policy.ConfiguredBannerCode,
            cachedBannerCode,
            policy.ConfiguredPrimaryColor,
            policy.ConfiguredSecondaryColor);
    }

    public CampaignKingdomPolicy GetCampaignKingdomPolicy(string kingdomName, bool isPlayerKingdom)
    {
        ScopeKey kingdomScope = FindKingdomScope(kingdomName, isPlayerKingdom);
        CampaignBannerPolicyOverride policy = GetCampaignPolicy(kingdomScope);
        return new CampaignKingdomPolicy(
            policy.KingdomBannerCode,
            policy.KingdomPrimaryColor,
            policy.KingdomSecondaryColor);
    }

    internal bool HasScope(ScopeKey scope)
    {
        foreach (ScopeKey key in _overrides.Keys)
        {
            if (key.HasSameScopeIdentity(scope))
            {
                return true;
            }
        }

        return false;
    }

    internal bool HasKingdomBranch(string kingdomName)
    {
        foreach (ScopeKey key in _overrides.Keys)
        {
            if (string.Equals(key.KingdomName, kingdomName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    internal bool HasClanBranch(string clanName)
    {
        foreach (ScopeKey key in _overrides.Keys)
        {
            if (key.KingdomName == null
                && string.Equals(key.ClanName, clanName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    internal bool HasKingdomClanBranch(string kingdomName, string clanName)
    {
        foreach (ScopeKey key in _overrides.Keys)
        {
            if (string.Equals(key.KingdomName, kingdomName, StringComparison.Ordinal)
                && string.Equals(key.ClanName, clanName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyDictionary<ScopeKey, CampaignBannerPolicyOverride> CopyCampaignPolicies(
        IReadOnlyDictionary<ScopeKey, CampaignBannerPolicyOverride> campaignPolicies)
    {
        var copy = new Dictionary<ScopeKey, CampaignBannerPolicyOverride>();
        if (campaignPolicies != null)
        {
            foreach (KeyValuePair<ScopeKey, CampaignBannerPolicyOverride> entry in campaignPolicies)
            {
                if (entry.Key == null)
                {
                    throw new ArgumentException("A campaign policy scope key cannot be null.", nameof(campaignPolicies));
                }

                copy.Add(entry.Key, entry.Value ?? CampaignBannerPolicyOverride.Empty);
            }
        }

        return new ReadOnlyDictionary<ScopeKey, CampaignBannerPolicyOverride>(copy);
    }

    private CampaignBannerPolicyOverride GetCampaignPolicy(ScopeKey scope)
    {
        return scope != null && _campaignPolicies.TryGetValue(scope, out CampaignBannerPolicyOverride policy)
            ? policy
            : CampaignBannerPolicyOverride.Empty;
    }

    private ScopeKey FindKingdomScope(string kingdomName, bool isPlayerKingdom)
    {
        ScopeKey exact = kingdomName == null ? null : ScopeKey.ForKingdom(kingdomName);
        if (HasCampaignPolicy(exact))
        {
            return exact;
        }

        ScopeKey player = ScopeKey.ForKingdom("PlayerKingdom");
        return isPlayerKingdom && HasCampaignPolicy(player) ? player : null;
    }

    private ScopeKey FindClanScope(string clanName, bool isPlayerClan)
    {
        ScopeKey exact = clanName == null ? null : ScopeKey.ForClan(clanName);
        if (HasCampaignPolicy(exact))
        {
            return exact;
        }

        ScopeKey player = ScopeKey.ForClan("PlayerClan");
        return isPlayerClan && HasCampaignPolicy(player) ? player : null;
    }

    private ScopeKey FindKingdomClanScope(ScopeKey kingdomScope, string clanName, bool isPlayerClan)
    {
        if (kingdomScope == null)
        {
            return null;
        }

        ScopeKey exact = clanName == null
            ? null
            : ScopeKey.ForKingdomClan(kingdomScope.KingdomName, clanName);
        if (HasCampaignPolicy(exact))
        {
            return exact;
        }

        ScopeKey player = ScopeKey.ForKingdomClan(kingdomScope.KingdomName, "PlayerClan");
        return isPlayerClan && HasCampaignPolicy(player) ? player : null;
    }

    private bool HasCampaignPolicy(ScopeKey scope)
    {
        return scope != null && _campaignPolicies.ContainsKey(scope);
    }
}
