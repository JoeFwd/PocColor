using PocColor.Integration.Bannerlord;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class BannerCodePaletteMigratorTests
{
    [Fact]
    public void Migrates_only_colour_fields_in_each_banner_data_record()
    {
        string bannerCode = "145.145.40.128.129.132.140.141.144.0";

        string migrated = new BannerCodePaletteMigrator().MigrateV12PaletteIds(bannerCode);

        Assert.Equal("145.142.172.128.129.132.140.141.144.0", migrated);
    }
}
