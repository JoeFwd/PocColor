using System;
using System.Runtime.CompilerServices;
using PocColor.Integration.Bannerlord;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class CampaignStateWriterTests
{
    private readonly ICampaignStateWriter _writer = new ReflectionCampaignStateWriter(new BannerCodePaletteMigrator());

    [Fact]
    public void Rejects_a_null_kingdom_target_before_reflection()
    {
        Assert.Throws<ArgumentNullException>(() => _writer.SetKingdomPrimaryColor(null, 101));
    }

    [Fact]
    public void Writes_kingdom_campaign_state_alongside_banner_state()
    {
        Kingdom kingdom = (Kingdom)RuntimeHelpers.GetUninitializedObject(typeof(Kingdom));
        string bannerCode = new Banner().Serialize();

        _writer.SetKingdomPrimaryColor(kingdom, 101);
        _writer.SetKingdomSecondaryColor(kingdom, 202);
        _writer.SetKingdomBanner(kingdom, bannerCode);

        Assert.Equal(101u, kingdom.Color);
        Assert.Equal(101u, kingdom.PrimaryBannerColor);
        Assert.Equal(202u, kingdom.Color2);
        Assert.Equal(202u, kingdom.SecondaryBannerColor);
        Assert.Equal(bannerCode, kingdom.Banner.Serialize());
    }

    [Fact]
    public void Replaces_clan_banner_backing_state()
    {
        Clan clan = (Clan)RuntimeHelpers.GetUninitializedObject(typeof(Clan));
        string bannerCode = new Banner().Serialize();

        _writer.SetClanBanner(clan, bannerCode);

        Assert.Equal(bannerCode, clan.Banner.Serialize());
    }
}
