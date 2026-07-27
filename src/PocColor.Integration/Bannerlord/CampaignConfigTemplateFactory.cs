using PocColor.Infrastructure.LegacyConfig.Model;
using TaleWorlds.CampaignSystem;

namespace PocColor.Integration.Bannerlord;

public static class CampaignConfigTemplateFactory
{
    public static PocColorModConfig Create(Campaign campaign)
    {
        var template = new PocColorModConfig
        {
            kingdoms = new Map<string, PocColorModConfigKingdom>(),
            clans = new Map<string, PocColorModConfigClan>()
        };

        foreach (Kingdom kingdom in campaign.Kingdoms)
        {
            PocColorModConfigKingdom kingdomConfig = CreateKingdomTemplate();
            foreach (Clan clan in kingdom.Clans)
            {
                kingdomConfig.clans[clan.Name.ToString()] = CreateClanTemplate(clan);
            }

            template.kingdoms[kingdom.Name.ToString()] = kingdomConfig;
        }

        foreach (Clan clan in campaign.Clans)
        {
            if (clan.Kingdom == null)
            {
                template.clans[clan.Name.ToString()] = CreateClanTemplate(clan);
            }
        }

        return template;
    }

    private static PocColorModConfigKingdom CreateKingdomTemplate()
    {
        return new PocColorModConfigKingdom
        {
            mode = 2,
            color = new[] { "color1", "color2" },
            color2 = new[] { "color1", "color2" },
            banner = "dummy banner",
            banners = new[] { "banner1", "banner2", "banner3" },
            FollowKingdomColors = "false",
            kingdomBanner = "the.super.kingdom.banner",
            primaryColor = "88",
            secondaryColor = "40",
            shields = new[] { "shield1", "shield2", "shield3" },
            clans = new Map<string, PocColorModConfigClan>()
        };
    }

    private static PocColorModConfigClan CreateClanTemplate(Clan clan)
    {
        var clanConfig = new PocColorModConfigClan
        {
            mode = 2,
            color = new[] { "color1", "color2" },
            color2 = new[] { "color1", "color2" },
            banner = "dummy banner",
            banners = new[] { "banner1", "banner2", "banner3" },
            FollowKingdomColors = "false",
            clanBanner = "the.super.kingdom.banner",
            primaryColor = "83",
            secondaryColor = "40",
            shields = new[] { "shield1", "shield2", "shield3" },
            units = new Map<string, PocColorModConfigUnit>()
        };

        foreach (Hero hero in clan.Heroes)
        {
            clanConfig.units[hero.Name.ToString()] = CreateUnitTemplate();
        }

        return clanConfig;
    }

    private static PocColorModConfigUnit CreateUnitTemplate()
    {
        return new PocColorModConfigUnit
        {
            mode = 3,
            color = new[] { "color1", "color2" },
            color2 = new[] { "color1", "color2" },
            banner = "dummy banner",
            banners = new[] { "banner1", "banner2", "banner3" },
            shields = new[] { "shield1", "shield2", "shield3" }
        };
    }
}
