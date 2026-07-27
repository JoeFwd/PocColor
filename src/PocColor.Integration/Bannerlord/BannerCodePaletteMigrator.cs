using System;
using System.Globalization;

namespace PocColor.Integration.Bannerlord;

public sealed class BannerCodePaletteMigrator
{
    private const int BannerDataFieldCount = 10;

    public string MigrateV12PaletteIds(string bannerCode)
    {
        if (string.IsNullOrWhiteSpace(bannerCode))
        {
            return bannerCode;
        }

        string[] fields = bannerCode.Split('.');
        if (fields.Length % BannerDataFieldCount != 0)
        {
            return bannerCode;
        }

        bool changed = false;
        for (int index = 0; index < fields.Length; index += BannerDataFieldCount)
        {
            changed |= MigrateColorId(fields, index + 1);
            changed |= MigrateColorId(fields, index + 2);
        }

        return changed ? string.Join(".", fields) : bannerCode;
    }

    private static bool MigrateColorId(string[] fields, int index)
    {
        if (!int.TryParse(fields[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out int colorId))
        {
            return false;
        }

        int migratedColorId;
        switch (colorId)
        {
            case 40: migratedColorId = 172; break;
            case 128: migratedColorId = 127; break;
            case 129: migratedColorId = 126; break;
            case 132: migratedColorId = 131; break;
            case 140: migratedColorId = 139; break;
            case 141: migratedColorId = 138; break;
            case 144: migratedColorId = 143; break;
            case 145: migratedColorId = 142; break;
            default: return false;
        }

        fields[index] = migratedColorId.ToString(CultureInfo.InvariantCulture);
        return true;
    }
}
