namespace PocColor.Infrastructure.LegacyConfig.Model;

public class ConfigSectionCulture : ConfigSectionTier
{
    public Map<int, ConfigSectionTier> tiers { get; set; }
}
