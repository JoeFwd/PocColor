namespace PocColor.Config;

public class ConfigSectionTier : ConfigSection
{
	public Map<string, ConfigSection> types { get; set; }
}
