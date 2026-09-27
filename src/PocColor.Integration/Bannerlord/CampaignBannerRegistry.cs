using System;
using System.Collections.Generic;

namespace PocColor.Integration.Bannerlord;

public sealed class CampaignBannerRegistry
{
    private readonly Dictionary<string, string> _kingdomsByBanner =
        new Dictionary<string, string>(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _clansByBanner =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, string> KingdomsByBanner => _kingdomsByBanner;

    public IReadOnlyDictionary<string, string> ClansByBanner => _clansByBanner;

    public void Clear()
    {
        _kingdomsByBanner.Clear();
        _clansByBanner.Clear();
    }

    public void Register(string bannerCode, string kingdomName, string clanName)
    {
        if (string.IsNullOrEmpty(bannerCode))
        {
            return;
        }

        if (!string.IsNullOrEmpty(kingdomName))
        {
            _kingdomsByBanner[bannerCode] = kingdomName;
        }

        if (!string.IsNullOrEmpty(clanName))
        {
            _clansByBanner[bannerCode] = clanName;
        }
    }
}
