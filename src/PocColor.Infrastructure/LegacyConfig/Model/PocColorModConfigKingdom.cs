namespace PocColor.Infrastructure.LegacyConfig.Model;

public class PocColorModConfigKingdom : ConfigSection, UnitsContainerSection
{
    public new int? mode { get; set; }

    public new string[] color { get; set; }

    public new string[] color2 { get; set; }

    public string clanBanner { get; set; }

    public string clanShield { get; set; }

    public string kingdomBanner { get; set; }

    public string kingdomShield { get; set; }

    public string primaryColor { get; set; }

    public string secondaryColor { get; set; }

    public string FollowKingdomColors { get; set; }

    public string FollowKingdomBackgroundOnly { get; set; }

    public new string banner { get; set; }

    public new string[] shields { get; set; }

    public new string[] banners { get; set; }

    public new string BearRulerColors { get; set; }

    public new string BearRulerBanner { get; set; }

    public new string BearRulerShield { get; set; }

    public new int? rulerMode { get; set; }

    public new string[] rulerColor { get; set; }

    public new string[] rulerColor2 { get; set; }

    public new string rulerBanner { get; set; }

    public new string rulerShield { get; set; }

    public new string[] combatShields { get; set; }

    public new string[] combatBanners { get; set; }

    public new Map<string, string[]>[] vars { get; set; }

    public Map<string, PocColorModConfigUnit> units { get; set; }

    public Map<string, PocColorModConfigClan> clans { get; set; }

    public PocColorModGroupEntry[] groups { get; set; }
}
