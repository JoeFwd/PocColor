using System;
using PocColor.Domain.Appearance;
using PocColor.Integration.Bannerlord;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class CampaignSnapshotFactoryTests
{
    [Fact]
    public void Requires_an_explicit_campaign_state_writer()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CampaignSnapshotFactory(new AppearanceRuleSet(null), null, new BannerSerializer()));
    }
}
