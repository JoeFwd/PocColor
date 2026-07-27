namespace PocColor.Infrastructure.LegacyConfig.Model;

public class PocColorModConfig : UnitsContainerSection
{
    public bool log { get; set; }

    public bool generateTemplate { get; set; }

    public PocColorModConfigColorEntry defaultConfig { get; set; }

    public Map<string, PocColorModConfigKingdom> kingdoms { get; set; }

    public Map<string, PocColorModConfigClan> clans { get; set; }

    public Map<string, PocColorModConfigUnit> units { get; set; }

    public PocColorModGroupEntry[] groups { get; set; }
}
