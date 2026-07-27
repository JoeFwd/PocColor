using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Infrastructure.LegacyConfig.Mapping;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig;

public sealed class CampaignPolicyCompiler
{
    private readonly CampaignBannerPolicyMapper _policyMapper;

    public CampaignPolicyCompiler(CampaignBannerPolicyMapper policyMapper)
    {
        _policyMapper = policyMapper;
    }

    public Dictionary<ScopeKey, CampaignBannerPolicyOverride> Compile(
        PocColorModConfig config,
        ICollection<LegacyConfigDiagnostic> diagnostics)
    {
        var policies = new Dictionary<ScopeKey, CampaignBannerPolicyOverride>();

        AddCampaignPolicy(
            policies,
            ScopeKey.Default,
            _policyMapper.Map(config.defaultConfig, ScopeKey.Default, diagnostics));

        if (config.clans != null)
        {
            foreach (KeyValuePair<string, PocColorModConfigClan> entry in config.clans)
            {
                if (entry.Value != null)
                {
                    ScopeKey scope = ScopeKey.ForClan(entry.Key);
                    AddCampaignPolicy(policies, scope, _policyMapper.Map(entry.Value, scope, diagnostics));
                }
            }
        }

        if (config.kingdoms == null)
        {
            return policies;
        }

        foreach (KeyValuePair<string, PocColorModConfigKingdom> entry in config.kingdoms)
        {
            if (entry.Value == null)
            {
                continue;
            }

            ScopeKey kingdomScope = ScopeKey.ForKingdom(entry.Key);
            AddCampaignPolicy(policies, kingdomScope, _policyMapper.Map(entry.Value, kingdomScope, diagnostics));

            if (entry.Value.clans == null)
            {
                continue;
            }

            foreach (KeyValuePair<string, PocColorModConfigClan> clanEntry in entry.Value.clans)
            {
                if (clanEntry.Value != null)
                {
                    ScopeKey scope = ScopeKey.ForKingdomClan(entry.Key, clanEntry.Key);
                    AddCampaignPolicy(policies, scope, _policyMapper.Map(clanEntry.Value, scope, diagnostics));
                }
            }
        }

        return policies;
    }

    private static void AddCampaignPolicy(
        IDictionary<ScopeKey, CampaignBannerPolicyOverride> policies,
        ScopeKey scope,
        CampaignBannerPolicyOverride policy)
    {
        policies[scope] = policy ?? CampaignBannerPolicyOverride.Empty;
    }
}
