using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public sealed class BannerSnapshotFactory
{
    public BannerSnapshot Create(Banner banner)
    {
        var colors = new List<ColorReference>();
        if (banner?.BannerDataList != null)
        {
            foreach (BannerData data in banner.BannerDataList)
            {
                colors.Add(ColorReference.FromPaletteId(data.ColorId));
            }
        }

        return new BannerSnapshot(BannerSerializer.Serialize(banner), colors);
    }
}
