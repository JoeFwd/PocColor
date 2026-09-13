using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace PocColor.Integration.Bannerlord;

/// <summary>
/// Writes campaign banner state for the campaign adapter.
/// Implementations are injected so callers do not hide a reflection dependency.
/// </summary>
public interface ICampaignStateWriter
{
    void SetKingdomPrimaryColor(Kingdom kingdom, uint color);

    void SetKingdomSecondaryColor(Kingdom kingdom, uint color);

    void SetKingdomBanner(Kingdom kingdom, string bannerCode);

    void SetClanBanner(Clan clan, string bannerCode);
}

/// <summary>
/// Uses Bannerlord's private Kingdom color state where no public setter exists.
/// Keep this adapter at the framework edge; replace it when the game exposes a
/// supported API instead of changing campaign planning code.
/// </summary>
public sealed class ReflectionCampaignStateWriter : ICampaignStateWriter
{
    private static readonly Lazy<CampaignStateBindings> Bindings =
        new Lazy<CampaignStateBindings>(CampaignStateBindings.Create);

    public void SetKingdomPrimaryColor(Kingdom kingdom, uint color)
    {
        EnsureTarget(kingdom, nameof(kingdom));
        Bindings.Value.Kingdom.SetPrimaryColor(kingdom, color);
    }

    public void SetKingdomSecondaryColor(Kingdom kingdom, uint color)
    {
        EnsureTarget(kingdom, nameof(kingdom));
        Bindings.Value.Kingdom.SetSecondaryColor(kingdom, color);
    }

    public void SetKingdomBanner(Kingdom kingdom, string bannerCode)
    {
        EnsureTarget(kingdom, nameof(kingdom));
        Bindings.Value.Kingdom.SetBanner(kingdom, new Banner(bannerCode));
    }

    public void SetClanBanner(Clan clan, string bannerCode)
    {
        EnsureTarget(clan, nameof(clan));
        Bindings.Value.Clan.SetBanner(clan, new Banner(bannerCode));
    }

    private static void EnsureTarget(object target, string parameterName)
    {
        if (target == null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}

/// <summary>
/// Resolves the private fields used by the current Bannerlord reference
/// assemblies. Keeping these names in one place makes a game-version update
/// an isolated binding change instead of a change to campaign behavior.
/// </summary>
internal sealed class CampaignStateBindings
{
    private CampaignStateBindings(KingdomBindings kingdom, ClanBindings clan)
    {
        Kingdom = kingdom;
        Clan = clan;
    }

    public KingdomBindings Kingdom { get; }

    public ClanBindings Clan { get; }

    public static CampaignStateBindings Create()
    {
        return new CampaignStateBindings(
            KingdomBindings.Create(typeof(Kingdom)),
            ClanBindings.Create(typeof(Clan)));
    }

    internal sealed class KingdomBindings
    {
        private readonly PrivateField _banner;
        private readonly PrivateField _color;
        private readonly PrivateField _color2;
        private readonly PrivateField _primaryBannerColor;
        private readonly PrivateField _secondaryBannerColor;

        private KingdomBindings(
            PrivateField banner,
            PrivateField color,
            PrivateField color2,
            PrivateField primaryBannerColor,
            PrivateField secondaryBannerColor)
        {
            _banner = banner;
            _color = color;
            _color2 = color2;
            _primaryBannerColor = primaryBannerColor;
            _secondaryBannerColor = secondaryBannerColor;
        }

        public static KingdomBindings Create(Type kingdomType)
        {
            return new KingdomBindings(
                PrivateField.Required(kingdomType, CampaignStateFieldNames.KingdomBanner),
                PrivateField.Required(kingdomType, CampaignStateFieldNames.KingdomColor),
                PrivateField.Required(kingdomType, CampaignStateFieldNames.KingdomColor2),
                PrivateField.Required(kingdomType, CampaignStateFieldNames.KingdomPrimaryBannerColor),
                PrivateField.Required(kingdomType, CampaignStateFieldNames.KingdomSecondaryBannerColor));
        }

        public void SetPrimaryColor(Kingdom kingdom, uint color)
        {
            _color.Set(kingdom, color);
            _primaryBannerColor.Set(kingdom, color);
        }

        public void SetSecondaryColor(Kingdom kingdom, uint color)
        {
            _color2.Set(kingdom, color);
            _secondaryBannerColor.Set(kingdom, color);
        }

        public void SetBanner(Kingdom kingdom, Banner banner)
        {
            _banner.Set(kingdom, banner);
        }
    }

    internal sealed class ClanBindings
    {
        private readonly PrivateField _banner;

        private ClanBindings(PrivateField banner)
        {
            _banner = banner;
        }

        public static ClanBindings Create(Type clanType)
        {
            return new ClanBindings(
                PrivateField.Required(clanType, CampaignStateFieldNames.ClanBanner));
        }

        public void SetBanner(Clan clan, Banner banner)
        {
            _banner.Set(clan, banner);
        }
    }

    private sealed class PrivateField
    {
        private readonly FieldInfo _field;

        private PrivateField(FieldInfo field)
        {
            _field = field;
        }

        public static PrivateField Required(Type declaringType, string name)
        {
            FieldInfo field = declaringType.GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new MissingFieldException(declaringType.FullName, name);
            }

            return new PrivateField(field);
        }

        public void Set(object target, object value)
        {
            _field.SetValue(target, value);
        }
    }

    private static class CampaignStateFieldNames
    {
        public const string KingdomBanner = "<Banner>k__BackingField";
        public const string KingdomColor = "<Color>k__BackingField";
        public const string KingdomColor2 = "<Color2>k__BackingField";
        public const string KingdomPrimaryBannerColor = "<PrimaryBannerColor>k__BackingField";
        public const string KingdomSecondaryBannerColor = "<SecondaryBannerColor>k__BackingField";
        public const string ClanBanner = "_banner";
    }
}
