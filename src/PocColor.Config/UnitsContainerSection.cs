namespace PocColor.Config;

internal interface UnitsContainerSection
{
	Map<string, PocColorModConfigUnit> units { get; set; }
}
