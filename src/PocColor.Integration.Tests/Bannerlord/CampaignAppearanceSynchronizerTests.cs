using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Integration.Bannerlord;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class CampaignAppearanceSynchronizerTests
{
    [Fact]
    public void Requires_an_explicit_campaign_state_writer()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CampaignAppearanceSynchronizer(
                new AppearanceRuleSet(null),
                null,
                new BannerSerializer(),
                new CampaignBannerRegistry()));
    }

    [Fact]
    public void Restores_the_banner_captured_before_native_load_when_campaign_cache_is_empty()
    {
        var campaignPolicies = new Dictionary<ScopeKey, CampaignBannerPolicyOverride>
        {
            [ScopeKey.Default] = new CampaignBannerPolicyOverride(followKingdomColors: false)
        };
        var writer = new ReflectionCampaignStateWriter();
        var synchronizer = new CampaignAppearanceSynchronizer(
            new AppearanceRuleSet(null, campaignPolicies),
            writer,
            new BannerSerializer(),
            new CampaignBannerRegistry());
        Clan clan = (Clan)RuntimeHelpers.GetUninitializedObject(typeof(Clan));
        var savedBanner = new Banner("11.127.172.1000.1000.764.764.1.0.0");
        var kingdomColoredBanner = new Banner("11.142.83.1000.1000.764.764.1.0.0");
        writer.SetClanBanner(clan, kingdomColoredBanner.Serialize());

        synchronizer.RestoreClanBannerAfterKingdomUpdate(clan, savedBanner.Serialize());

        Assert.Equal(savedBanner.Serialize(), clan.Banner.Serialize());
    }
}
