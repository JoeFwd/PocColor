using PocColor.Integration.Bannerlord;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class CampaignBannerRegistryTests
{
    [Fact]
    public void Registers_and_clears_banner_owners()
    {
        var registry = new CampaignBannerRegistry();

        registry.Register("banner", "kingdom", "clan");

        Assert.Equal("kingdom", registry.KingdomsByBanner["banner"]);
        Assert.Equal("clan", registry.ClansByBanner["banner"]);

        registry.Clear();

        Assert.Empty(registry.KingdomsByBanner);
        Assert.Empty(registry.ClansByBanner);
    }
}
