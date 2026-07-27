namespace PocColor.Infrastructure.LegacyConfig.Model;

public interface UnitsContainerSection
{
    Map<string, PocColorModConfigUnit> units { get; set; }
}
