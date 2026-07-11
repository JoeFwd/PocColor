namespace PocColor.Config;

public class ConfigSectionUnit : ConfigSectionCulture
{
	public Map<string, ConfigSectionCulture> cultures { get; set; }
}
