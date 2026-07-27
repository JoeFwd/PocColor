using System.IO;
using Newtonsoft.Json;
using PocColor.Infrastructure.LegacyConfig.Model;
using PocColor.Integration.Bannerlord;
using TaleWorlds.CampaignSystem;

namespace PocColor.Integration.Lifecycle;

public sealed class PocColorCampaignTemplateWriter
{
    private readonly PocColorModConfig _configuration;
    private readonly string _moduleDirectory;

    public PocColorCampaignTemplateWriter(PocColorModConfig configuration, string moduleDirectory)
    {
        _configuration = configuration;
        _moduleDirectory = moduleDirectory;
    }

    public void WriteWhenRequested()
    {
        if (!_configuration.generateTemplate || Campaign.Current == null)
        {
            return;
        }

        string path = Path.Combine(_moduleDirectory, "config.json.full.template");
        using (var writer = new StreamWriter(path))
        {
            new JsonSerializer { NullValueHandling = NullValueHandling.Ignore }
                .Serialize(writer, CampaignConfigTemplateFactory.Create(Campaign.Current));
        }
    }
}
