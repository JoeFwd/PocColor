using System;
using System.Collections.Generic;
using System.Text;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public static class BannerSerializer
{
    public static string Serialize(Banner banner)
    {
        if (banner?.BannerDataList == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        bool first = true;
        foreach (BannerData data in banner.BannerDataList)
        {
            if (!first)
            {
                builder.Append('.');
            }

            first = false;
            builder.Append(data.MeshId);
            builder.Append('.');
            builder.Append(data.ColorId);
            builder.Append('.');
            builder.Append(data.ColorId2);
            builder.Append('.');
            builder.Append((int)data.Size.x);
            builder.Append('.');
            builder.Append((int)data.Size.y);
            builder.Append('.');
            builder.Append((int)data.Position.x);
            builder.Append('.');
            builder.Append((int)data.Position.y);
            builder.Append('.');
            builder.Append(data.DrawStroke ? 1 : 0);
            builder.Append('.');
            builder.Append(data.Mirror ? 1 : 0);
            builder.Append('.');
            builder.Append((int)Math.Round(data.RotationValue / 0.00278f, MidpointRounding.AwayFromZero));
        }

        return builder.ToString();
    }

    public static BannerSnapshot CreateSnapshot(Banner banner)
    {
        var colors = new List<ColorReference>();
        if (banner?.BannerDataList != null)
        {
            foreach (BannerData data in banner.BannerDataList)
            {
                colors.Add(ColorReference.FromPaletteId(data.ColorId));
            }
        }

        return new BannerSnapshot(Serialize(banner), colors);
    }
}
