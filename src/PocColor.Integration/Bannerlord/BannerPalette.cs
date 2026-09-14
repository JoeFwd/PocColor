using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

public sealed class BannerPalette
{
    private static readonly int[] DefaultPaletteIds =
    {
        83, 84, 12, 40, 37, 49, 56, 64, 75, 89, 110, 116, 120, 124, 126,
        127, 128, 132, 138, 140, 141, 142, 143, 145, 149, 150, 154, 158, 159
    };

    private readonly BannerSnapshotMapper _bannerSnapshotMapper;

    public BannerPalette(BannerSnapshotMapper bannerSnapshotMapper)
    {
        _bannerSnapshotMapper = bannerSnapshotMapper ?? throw new ArgumentNullException(nameof(bannerSnapshotMapper));
    }

    public AppearancePalette Create()
    {
        return new AppearancePalette(null, CreateDefaultColors(), CreateSnapshot);
    }

    private BannerSnapshot CreateSnapshot(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        try
        {
            return _bannerSnapshotMapper.CreateSnapshot(new Banner(code));
        }
        catch (Exception)
        {
            // Invalid legacy banner strings follow V1's current-banner fallback.
            return null;
        }
    }

    private static IReadOnlyList<ColorReference> CreateDefaultColors()
    {
        var colors = new ColorReference[DefaultPaletteIds.Length];
        for (int index = 0; index < DefaultPaletteIds.Length; index++)
        {
            colors[index] = ColorReference.FromPaletteId(DefaultPaletteIds[index]);
        }

        return colors;
    }
}
