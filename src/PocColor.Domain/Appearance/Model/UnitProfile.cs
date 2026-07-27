namespace PocColor.Domain.Appearance.Model;

public sealed class UnitProfile
{
    public UnitProfile(
        string kingdomName,
        string clanName,
        string unitName,
        bool isPlayerKingdom,
        bool isPlayerClan,
        bool isPlayer,
        bool isKing,
        bool isLeader,
        bool isHero,
        bool isMounted,
        bool isRanged,
        int tier,
        string culture,
        string rulerBannerCode)
    {
        KingdomName = kingdomName;
        ClanName = clanName;
        UnitName = unitName;
        IsPlayerKingdom = isPlayerKingdom;
        IsPlayerClan = isPlayerClan;
        IsPlayer = isPlayer;
        IsKing = isKing;
        IsLeader = isLeader;
        IsHero = isHero;
        IsMounted = isMounted;
        IsRanged = isRanged;
        Tier = tier;
        Culture = culture;
        RulerBannerCode = rulerBannerCode;
    }

    public string KingdomName { get; }

    public string ClanName { get; }

    public string UnitName { get; }

    public bool IsPlayerKingdom { get; }

    public bool IsPlayerClan { get; }

    public bool IsPlayer { get; }

    public bool IsKing { get; }

    public bool IsLeader { get; }

    public bool IsHero { get; }

    public bool IsMounted { get; }

    public bool IsRanged { get; }

    public int Tier { get; }

    public string Culture { get; }

    public string RulerBannerCode { get; }
}
