namespace PocColor.Infrastructure.LegacyConfig.Model;

public class PocColorModConfigColorEntry
{
    public int? mode { get; set; }

    public string[] color { get; set; }

    public string[] color2 { get; set; }

    public string FollowKingdomColors { get; set; }

    public string FollowKingdomBackgroundOnly { get; set; }

    public string BearRulerColors { get; set; }

    public string BearRulerBanner { get; set; }

    public string BearRulerShield { get; set; }

    public Map<string, string[]>[] vars { get; set; }
}
