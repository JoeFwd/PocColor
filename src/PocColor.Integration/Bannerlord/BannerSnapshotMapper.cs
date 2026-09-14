using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public sealed class BannerSnapshotMapper
{
    private readonly BannerSerializer _bannerSerializer;

    public BannerSnapshotMapper(BannerSerializer bannerSerializer)
    {
        _bannerSerializer = bannerSerializer ?? throw new ArgumentNullException(nameof(bannerSerializer));
    }

    public BannerSnapshot CreateSnapshot(Banner banner)
    {
        var colors = new List<ColorReference>();
        if (banner?.BannerDataList != null)
        {
            foreach (BannerData data in banner.BannerDataList)
            {
                colors.Add(ColorReference.FromPaletteId(data.ColorId));
            }
        }

        return new BannerSnapshot(_bannerSerializer.Serialize(banner), colors);
    }
}
