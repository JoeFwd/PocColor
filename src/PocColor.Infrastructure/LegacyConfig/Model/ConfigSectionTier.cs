namespace PocColor.Infrastructure.LegacyConfig.Model;

public class ConfigSectionTier : ConfigSection
{
    public Map<string, ConfigSection> types { get; set; }
}
