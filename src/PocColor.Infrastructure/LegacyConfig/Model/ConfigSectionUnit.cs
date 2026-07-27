namespace PocColor.Infrastructure.LegacyConfig.Model;

public class ConfigSectionUnit : ConfigSectionCulture
{
    public Map<string, ConfigSectionCulture> cultures { get; set; }
}
