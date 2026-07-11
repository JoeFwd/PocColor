namespace PocColor.Config;

public class ConfigSectionCulture : ConfigSectionTier
{
	public Map<int, ConfigSectionTier> tiers { get; set; }
}
