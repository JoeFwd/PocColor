using System;

namespace PocColor.Domain.Appearance.Model;

public sealed class ScopeKey : IEquatable<ScopeKey>
{
    public static ScopeKey Default { get; } = new ScopeKey(null, null, null);

    public ScopeKey(string kingdomName, string clanName, string unitName)
        : this(kingdomName, clanName, unitName, null, null, null, null)
    {
    }

    private ScopeKey(
        string kingdomName,
        string clanName,
        string unitName,
        string culture,
        int? tier,
        bool? isMounted,
        bool? isRanged)
    {
        KingdomName = kingdomName;
        ClanName = clanName;
        UnitName = unitName;
        Culture = culture;
        Tier = tier;
        IsMounted = isMounted;
        IsRanged = isRanged;
    }

    public string KingdomName { get; }

    public string ClanName { get; }

    public string UnitName { get; }

    public string Culture { get; }

    public int? Tier { get; }

    public bool? IsMounted { get; }

    public bool? IsRanged { get; }

    public static ScopeKey ForKingdom(string kingdomName) => new ScopeKey(kingdomName, null, null);

    public static ScopeKey ForClan(string clanName) => new ScopeKey(null, clanName, null);

    public static ScopeKey ForKingdomClan(string kingdomName, string clanName) => new ScopeKey(kingdomName, clanName, null);

    public static ScopeKey ForUnit(string unitName) => new ScopeKey(null, null, unitName);

    public static ScopeKey ForKingdomUnit(string kingdomName, string unitName) => new ScopeKey(kingdomName, null, unitName);

    public static ScopeKey ForClanUnit(string clanName, string unitName) => new ScopeKey(null, clanName, unitName);

    public static ScopeKey ForKingdomClanUnit(string kingdomName, string clanName, string unitName) => new ScopeKey(kingdomName, clanName, unitName);

    public ScopeKey WithCriteria(string culture = null, int? tier = null, bool? isMounted = null, bool? isRanged = null)
    {
        return new ScopeKey(
            KingdomName,
            ClanName,
            UnitName,
            culture ?? Culture,
            tier ?? Tier,
            isMounted ?? IsMounted,
            isRanged ?? IsRanged);
    }

    public bool Equals(ScopeKey other)
    {
        return other != null
            && string.Equals(KingdomName, other.KingdomName, StringComparison.Ordinal)
            && string.Equals(ClanName, other.ClanName, StringComparison.Ordinal)
            && string.Equals(UnitName, other.UnitName, StringComparison.Ordinal)
            && string.Equals(Culture, other.Culture, StringComparison.Ordinal)
            && Tier == other.Tier
            && IsMounted == other.IsMounted
            && IsRanged == other.IsRanged;
    }

    public override bool Equals(object obj) => Equals(obj as ScopeKey);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + Hash(KingdomName);
            hash = (hash * 31) + Hash(ClanName);
            hash = (hash * 31) + Hash(UnitName);
            hash = (hash * 31) + Hash(Culture);
            hash = (hash * 31) + Tier.GetHashCode();
            hash = (hash * 31) + IsMounted.GetHashCode();
            hash = (hash * 31) + IsRanged.GetHashCode();
            return hash;
        }
    }

    internal bool HasSameScopeIdentity(ScopeKey other)
    {
        return other != null
            && string.Equals(KingdomName, other.KingdomName, StringComparison.Ordinal)
            && string.Equals(ClanName, other.ClanName, StringComparison.Ordinal)
            && string.Equals(UnitName, other.UnitName, StringComparison.Ordinal);
    }

    private static int Hash(string value) => value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
}
