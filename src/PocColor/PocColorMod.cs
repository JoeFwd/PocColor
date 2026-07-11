using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using PocColor.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PocColor;

public class PocColorMod : MBSubModuleBase
{
	public const string MOD_VERSION = "1.1.7";

	public static bool doLog;

	public static PocColorModConfig config { get; set; }

	public static Map<string, string> bannerKingdomCache { get; set; }

	public static Map<string, string> bannerClanCache { get; set; }

	public static string SerializeBanner(Banner banner)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (BannerData item in (List<BannerData>)(object)banner.BannerDataList)
		{
			try
			{
				if (!flag)
				{
					stringBuilder.Append(".");
				}
				flag = false;
				stringBuilder.Append(item.MeshId);
				stringBuilder.Append('.');
				stringBuilder.Append(item.ColorId);
				stringBuilder.Append('.');
				stringBuilder.Append(item.ColorId2);
				stringBuilder.Append('.');
				stringBuilder.Append((int)item.Size.x);
				stringBuilder.Append('.');
				stringBuilder.Append((int)item.Size.y);
				stringBuilder.Append('.');
				stringBuilder.Append((int)item.Position.x);
				stringBuilder.Append('.');
				stringBuilder.Append((int)item.Position.y);
				stringBuilder.Append('.');
				stringBuilder.Append(item.DrawStroke ? 1 : 0);
				stringBuilder.Append('.');
				stringBuilder.Append(item.Mirror ? 1 : 0);
				stringBuilder.Append('.');
				float num = item.RotationValue / 0.00278f;
				int value = short.Parse(Math.Round(num, MidpointRounding.AwayFromZero).ToString());
				stringBuilder.Append(value);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
		return stringBuilder.ToString();
	}

	protected override void OnSubModuleLoad()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((MBSubModuleBase)this).OnSubModuleLoad();
		try
		{
			new Harmony("com.prpl.poccolormod").PatchAll();
		}
		catch (Exception ex)
		{
			Log.write("==> Harmony patching failed:" + ex.Message);
		}
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((MBSubModuleBase)this).OnBeforeInitialModuleScreenSetAsRoot();
		InformationManager.DisplayMessage(new InformationMessage("POC color mod [1.1.7]: loaded!"));
	}

	public override void OnGameInitializationFinished(Game game)
	{
		//IL_16eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_16f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_16fa: Expected O, but got Unknown
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Expected O, but got Unknown
		//IL_0858: Unknown result type (might be due to invalid IL or missing references)
		//IL_085f: Expected O, but got Unknown
		//IL_0d37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3e: Expected O, but got Unknown
		bool flag = false;
		bool flag2 = false;
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		Map<string, string> map = new Map<string, string>();
		Map<string, string> map2 = new Map<string, string>();
		try
		{
			config = new PocColorModConfig();
			Log.write("--- POC COLOR MOD: 1.1.7 ---");
			Log.write("read config file");
			config = JsonConvert.DeserializeObject<PocColorModConfig>(File.ReadAllText("..\\..\\modules\\PocColor\\config.json"));
			config.parseGroups();
		}
		catch (FileNotFoundException ex)
		{
			Log.write("config file not found: " + ex.Message);
		}
		catch (Exception ex2)
		{
			Log.write("Could not parse the config file :(");
			Log.write(" =====>" + ex2.Message);
		}
		doLog = config.log;
		Log.write("Build Cache -- start");
		try
		{
			for (int i = 0; i < ((List<Kingdom>)(object)Campaign.Current.Kingdoms).Count; i++)
			{
				Kingdom val = ((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i];
				Log.write("====> kingdom [" + ((object)val.Name).ToString() + "]");
				Clan playerClan = Clan.PlayerClan;
				object obj;
				if (playerClan == null)
				{
					obj = null;
				}
				else
				{
					Kingdom kingdom = playerClan.Kingdom;
					obj = ((kingdom == null) ? null : ((object)kingdom.Name)?.ToString());
				}
				string text7 = (string)obj;
				bool isPlayerKingdom = !string.IsNullOrEmpty(text7) && ((object)val.Name)?.ToString() == text7;
				(string, string, string, string) kingdomConfig = config.GetKingdomConfig(((object)val.Name)?.ToString(), isPlayerKingdom);
				text = kingdomConfig.Item1;
				text2 = kingdomConfig.Item2;
				text3 = kingdomConfig.Item3;
				text4 = kingdomConfig.Item4;
				string text8 = SerializeBanner(val.Banner);
				map.TryAdd(text8, ((object)val.Name).ToString());
				string key = text8;
				Clan rulingClan = val.RulingClan;
				map2.TryAdd(key, (rulingClan != null) ? ((object)rulingClan.Name).ToString() : null);
				if (doLog)
				{
					Log.write("> Initial Kingdom Banner loaded from game: [" + text8 + "]");
				}
				if (doLog)
				{
					Log.write("> Initial Kingdom primary color loaded from game: [" + BannerManager.GetColorId(val.PrimaryBannerColor) + "]");
				}
				if (doLog)
				{
					Log.write("> Initial Kingdom secondary color loaded from game: [" + BannerManager.GetColorId(val.SecondaryBannerColor) + "]");
				}
				if (text3 != null)
				{
					uint num = PocColorModSetColors.parseColor(text3);
					FieldInfo field = typeof(Kingdom).GetField("<PrimaryBannerColor>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
					TypedReference obj2 = __makeref(val);
					field.SetValueDirect(obj2, num);
					FieldInfo field2 = typeof(Kingdom).GetField("<Color>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
					TypedReference obj3 = __makeref(val);
					field2.SetValueDirect(obj3, num);
					if (doLog)
					{
						Log.write("> updating primaryColor for kingdom: [" + SerializeBanner(val.Banner) + "]");
					}
					Banner banner = val.Banner;
					if (banner != null)
					{
						banner.ChangePrimaryColor(num);
					}
					if (doLog)
					{
						Log.write("> updated Banner: [" + SerializeBanner(val.Banner) + "]");
					}
				}
				if (text4 != null)
				{
					uint num2 = PocColorModSetColors.parseColor(text4);
					FieldInfo field3 = typeof(Kingdom).GetField("<SecondaryBannerColor>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
					TypedReference obj4 = __makeref(val);
					field3.SetValueDirect(obj4, num2);
					FieldInfo field4 = typeof(Kingdom).GetField("<Color2>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
					TypedReference obj5 = __makeref(val);
					field4.SetValueDirect(obj5, num2);
					if (doLog)
					{
						Log.write("> updating SecondaryColor for kingdom: [" + SerializeBanner(val.Banner) + "]");
					}
					Banner banner2 = val.Banner;
					if (banner2 != null)
					{
						banner2.ChangeIconColors(num2);
					}
					if (doLog)
					{
						Log.write("> updated Banner: [" + SerializeBanner(val.Banner) + "]");
					}
				}
				if (text != null)
				{
					if (doLog)
					{
						Log.write("> updating banner for kingdom: [" + ((object)val.Name).ToString() + ", " + text + "]");
					}
					Banner value = new Banner(text);
					FieldInfo field5 = typeof(Kingdom).GetField("<Banner>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
					TypedReference obj6 = __makeref(val);
					field5.SetValueDirect(obj6, value);
					if (doLog)
					{
						Log.write("> updated KingdomBanner from Config: [" + SerializeBanner(val.Banner) + "]");
					}
				}
				text = SerializeBanner(val.Banner);
				Log.write("> adding new cache for banner: [" + text + "]");
				map.TryAdd(text, ((object)val.Name).ToString());
				string key2 = text;
				Clan rulingClan2 = val.RulingClan;
				map2.TryAdd(key2, (rulingClan2 != null) ? ((object)rulingClan2.Name).ToString() : null);
				foreach (Clan item in (List<Clan>)(object)val.Clans)
				{
					try
					{
						Log.write("====> kingdom [" + ((object)val.Name).ToString() + "], clan [" + ((object)item.Name).ToString() + "]");
						Clan playerClan2 = Clan.PlayerClan;
						string text9 = ((playerClan2 == null) ? null : ((object)playerClan2.Name)?.ToString());
						Clan playerClan3 = Clan.PlayerClan;
						object obj7;
						if (playerClan3 == null)
						{
							obj7 = null;
						}
						else
						{
							Kingdom kingdom2 = playerClan3.Kingdom;
							obj7 = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
						}
						text7 = (string)obj7;
						bool isPlayerClan = !string.IsNullOrEmpty(text9) && ((object)item.Name)?.ToString() == text9;
						isPlayerKingdom = !string.IsNullOrEmpty(text7) && ((object)val.Name)?.ToString() == text7;
						Log.write("> clan: [" + ((object)item.Name)?.ToString() + "] kingdom: " + ((object)val.Name)?.ToString() + ", isplayerKingdom: " + isPlayerKingdom + ", isPlayerClan:" + isPlayerClan);
						(flag, flag2, text5, text6, text3, text4) = config.GetClanConfig(((object)val.Name)?.ToString(), ((object)item.Name)?.ToString(), isPlayerKingdom, isPlayerClan);
						Log.write("> clan: [" + ((object)item.Name)?.ToString() + "] follow colors: " + flag + ", primary: " + text3 + ", secondary:" + text4);
						if (doLog)
						{
							Log.write("> Initial clan Banner loaded from game: [" + SerializeBanner(item.Banner) + "]");
						}
						if (doLog)
						{
							Log.write("> Initial clan primary loaded from game: [" + BannerManager.GetColorId(item.Color) + "]");
						}
						if (doLog)
						{
							Log.write("> Initial clan secondary loaded from game: [" + BannerManager.GetColorId(item.Color2) + "]");
						}
						if (text5 != null)
						{
							Log.write("> updating banner for clan: [" + ((object)item.Name).ToString() + ", " + text5 + "]");
							Banner value2 = new Banner(text5);
							Traverse.Create((object)item).Field("_banner").SetValue((object)value2);
						}
						if (text3 != null)
						{
							Log.write("> updating primary color for clan");
							uint num3 = PocColorModSetColors.parseColor(text3);
							Traverse.Create((object)item).Field("Color").SetValue((object)num3);
							Banner banner3 = item.Banner;
							if (banner3 != null)
							{
								banner3.ChangePrimaryColor(num3);
							}
						}
						if (text4 != null)
						{
							Log.write("> updating secondary color for clan");
							uint num4 = PocColorModSetColors.parseColor(text4);
							Traverse.Create((object)item).Field("Color2").SetValue((object)num4);
							Banner banner4 = item.Banner;
							if (banner4 != null)
							{
								banner4.ChangeIconColors(num4);
							}
						}
						if (string.IsNullOrEmpty(text3) && string.IsNullOrEmpty(text4) && flag)
						{
							Log.write("> updating colors of clan from Kingdom");
							Traverse.Create((object)item).Method("UpdateBannerColorsAccordingToKingdom", Array.Empty<object>()).GetValue();
						}
						text8 = SerializeBanner(item.Banner);
						Log.write("> adding new cache for banner: [" + text8 + "]");
						map2.TryAdd(text8, ((object)item.Name).ToString());
						map.TryAdd(text8, (val == null) ? null : ((object)val.Name)?.ToString());
					}
					catch (Exception ex3)
					{
						Log.write(ex3.Message);
					}
				}
			}
			foreach (Clan item2 in (List<Clan>)(object)Campaign.Current.Clans)
			{
				Kingdom kingdom3 = item2.Kingdom;
				Log.write("====> clan [" + ((object)item2.Name).ToString() + "]");
				Clan playerClan4 = Clan.PlayerClan;
				string text10 = ((playerClan4 == null) ? null : ((object)playerClan4.Name)?.ToString());
				bool isPlayerClan2 = !string.IsNullOrEmpty(text10) && ((object)item2.Name)?.ToString() == text10;
				Clan playerClan5 = Clan.PlayerClan;
				object obj8;
				if (playerClan5 == null)
				{
					obj8 = null;
				}
				else
				{
					Kingdom kingdom4 = playerClan5.Kingdom;
					obj8 = ((kingdom4 == null) ? null : ((object)kingdom4.Name)?.ToString());
				}
				string text11 = (string)obj8;
				bool isPlayerKingdom2 = !string.IsNullOrEmpty(text11) && ((kingdom3 == null) ? null : ((object)kingdom3.Name)?.ToString()) == text11;
				Log.write("> clan: [" + ((object)item2.Name)?.ToString() + "] kingdom: " + ((kingdom3 == null) ? null : ((object)kingdom3.Name)?.ToString()) + ", isplayerKingdom: " + isPlayerKingdom2 + ", isPlayerClan:" + isPlayerClan2);
				(flag, flag2, text5, text6, text3, text4) = config.GetClanConfig((kingdom3 == null) ? null : ((object)kingdom3.Name)?.ToString(), ((object)item2.Name)?.ToString(), isPlayerKingdom2, isPlayerClan2);
				Log.write("> clan: [" + ((object)item2.Name)?.ToString() + "] follow: " + flag + ", follow Background: " + flag2 + ", primary: " + text3 + ", secondary:" + text4);
				if (doLog)
				{
					Log.write("> Initial clan Banner loaded from game: [" + SerializeBanner(item2.Banner) + "]");
				}
				if (doLog)
				{
					Log.write("> Initial clan primary loaded from game: [" + BannerManager.GetColorId(item2.Color) + "]");
				}
				if (doLog)
				{
					Log.write("> Initial clan secondary loaded from game: [" + BannerManager.GetColorId(item2.Color2) + "]");
				}
				if (text5 != null)
				{
					Log.write("> updating banner for clan: [" + ((object)item2.Name).ToString() + ", " + text5 + "]");
					Banner value3 = new Banner(text5);
					Traverse.Create((object)item2).Field("_banner").SetValue((object)value3);
				}
				if (text3 != null)
				{
					Log.write("> updating primary color for clan");
					uint num5 = PocColorModSetColors.parseColor(text3);
					Traverse.Create((object)item2).Field("Color").SetValue((object)num5);
					Banner banner5 = item2.Banner;
					if (banner5 != null)
					{
						banner5.ChangePrimaryColor(num5);
					}
				}
				if (text4 != null)
				{
					Log.write("> updating secondary color for clan");
					uint num6 = PocColorModSetColors.parseColor(text4);
					Traverse.Create((object)item2).Field("Color2").SetValue((object)num6);
					Banner banner6 = item2.Banner;
					if (banner6 != null)
					{
						banner6.ChangeIconColors(num6);
					}
				}
				if (string.IsNullOrEmpty(text3) && string.IsNullOrEmpty(text4) && (flag || flag2))
				{
					Log.write("> updating colors of clan from Kingdom");
					Traverse.Create((object)item2).Method("UpdateBannerColorsAccordingToKingdom", Array.Empty<object>()).GetValue();
				}
				string text12 = SerializeBanner(item2.Banner);
				Log.write("> adding new cache for banner: [" + text12 + "]");
				map2.TryAdd(text12, ((object)item2.Name).ToString());
				map.TryAdd(text12, (kingdom3 != null) ? ((object)kingdom3.Name)?.ToString() : null);
			}
			bannerClanCache = map2;
			bannerKingdomCache = map;
			Log.write("Build Cache -- done");
			if (!config.generateTemplate)
			{
				return;
			}
			Log.write("Building json template ...");
			PocColorModConfig pocColorModConfig = new PocColorModConfig();
			pocColorModConfig.kingdoms = new Map<string, PocColorModConfigKingdom>();
			pocColorModConfig.clans = new Map<string, PocColorModConfigClan>();
			foreach (Kingdom item3 in (List<Kingdom>)(object)Campaign.Current.Kingdoms)
			{
				PocColorModConfigKingdom pocColorModConfigKingdom = new PocColorModConfigKingdom();
				pocColorModConfigKingdom.clans = new Map<string, PocColorModConfigClan>();
				pocColorModConfigKingdom.mode = 2;
				pocColorModConfigKingdom.color = new string[2] { "color1", "color2" };
				pocColorModConfigKingdom.color2 = new string[2] { "color1", "color2" };
				pocColorModConfigKingdom.banner = "dummy banner";
				pocColorModConfigKingdom.banners = new string[3] { "banner1", "banner2", "banner3" };
				pocColorModConfigKingdom.FollowKingdomColors = "false";
				pocColorModConfigKingdom.kingdomBanner = "the.super.kingdom.banner";
				pocColorModConfigKingdom.primaryColor = "88";
				pocColorModConfigKingdom.secondaryColor = "40";
				pocColorModConfigKingdom.shields = new string[3] { "shield1", "shield2", "shield3" };
				foreach (Clan item4 in (List<Clan>)(object)item3.Clans)
				{
					PocColorModConfigClan pocColorModConfigClan = new PocColorModConfigClan();
					pocColorModConfigClan.mode = 2;
					pocColorModConfigClan.color = new string[2] { "color1", "color2" };
					pocColorModConfigClan.color2 = new string[2] { "color1", "color2" };
					pocColorModConfigClan.units = new Map<string, PocColorModConfigUnit>();
					pocColorModConfigClan.banner = "dummy banner";
					pocColorModConfigClan.banners = new string[3] { "banner1", "banner2", "banner3" };
					pocColorModConfigClan.FollowKingdomColors = "false";
					pocColorModConfigClan.clanBanner = "the.super.kingdom.banner";
					pocColorModConfigClan.primaryColor = "83";
					pocColorModConfigClan.secondaryColor = "40";
					pocColorModConfigClan.shields = new string[3] { "shield1", "shield2", "shield3" };
					foreach (Hero item5 in (List<Hero>)(object)item4.Heroes)
					{
						PocColorModConfigUnit pocColorModConfigUnit = new PocColorModConfigUnit();
						pocColorModConfigUnit.mode = 3;
						pocColorModConfigUnit.color = new string[2] { "color1", "color2" };
						pocColorModConfigUnit.color2 = new string[2] { "color1", "color2" };
						pocColorModConfigUnit.banner = "dummy banner";
						pocColorModConfigUnit.banners = new string[3] { "banner1", "banner2", "banner3" };
						pocColorModConfigUnit.shields = new string[3] { "shield1", "shield2", "shield3" };
						pocColorModConfigClan.units.TryAdd(((object)item5.Name).ToString(), pocColorModConfigUnit);
					}
					foreach (Hero item6 in (List<Hero>)(object)item4.Heroes)
					{
						PocColorModConfigUnit pocColorModConfigUnit2 = new PocColorModConfigUnit();
						pocColorModConfigUnit2.mode = 3;
						pocColorModConfigUnit2.color = new string[2] { "color1", "color2" };
						pocColorModConfigUnit2.color2 = new string[2] { "color1", "color2" };
						pocColorModConfigUnit2.banner = "dummy banner";
						pocColorModConfigUnit2.banners = new string[3] { "banner1", "banner2", "banner3" };
						pocColorModConfigUnit2.shields = new string[3] { "shield1", "shield2", "shield3" };
						pocColorModConfigClan.units.TryAdd(((object)item6.Name).ToString(), pocColorModConfigUnit2);
					}
					pocColorModConfigKingdom.clans.TryAdd(((object)item4.Name).ToString(), pocColorModConfigClan);
				}
				pocColorModConfig.kingdoms.TryAdd(((object)item3.Name).ToString(), pocColorModConfigKingdom);
			}
			foreach (Clan item7 in (List<Clan>)(object)Campaign.Current.Clans)
			{
				if (item7.Kingdom != null)
				{
					continue;
				}
				PocColorModConfigClan pocColorModConfigClan2 = new PocColorModConfigClan();
				pocColorModConfigClan2.mode = 2;
				pocColorModConfigClan2.color = new string[2] { "color1", "color2" };
				pocColorModConfigClan2.color2 = new string[2] { "color1", "color2" };
				pocColorModConfigClan2.units = new Map<string, PocColorModConfigUnit>();
				pocColorModConfigClan2.banner = "dummy banner";
				pocColorModConfigClan2.banners = new string[3] { "banner1", "banner2", "banner3" };
				pocColorModConfigClan2.FollowKingdomColors = "false";
				pocColorModConfigClan2.clanBanner = "the.super.kingdom.banner";
				pocColorModConfigClan2.primaryColor = "83";
				pocColorModConfigClan2.secondaryColor = "40";
				pocColorModConfigClan2.units = new Map<string, PocColorModConfigUnit>();
				foreach (Hero item8 in (List<Hero>)(object)item7.Heroes)
				{
					PocColorModConfigUnit pocColorModConfigUnit3 = new PocColorModConfigUnit();
					pocColorModConfigUnit3.mode = 3;
					pocColorModConfigUnit3.color = new string[2] { "color1", "color2" };
					pocColorModConfigUnit3.color2 = new string[2] { "color1", "color2" };
					pocColorModConfigUnit3.banner = "dummy banner";
					pocColorModConfigUnit3.banners = new string[3] { "banner1", "banner2", "banner3" };
					pocColorModConfigUnit3.shields = new string[3] { "shield1", "shield2", "shield3" };
					pocColorModConfigClan2.units.TryAdd(((object)item8.Name).ToString(), pocColorModConfigUnit3);
				}
				foreach (Hero item9 in (List<Hero>)(object)item7.Heroes)
				{
					PocColorModConfigUnit pocColorModConfigUnit4 = new PocColorModConfigUnit();
					pocColorModConfigUnit4.mode = 3;
					pocColorModConfigUnit4.color = new string[2] { "color1", "color2" };
					pocColorModConfigUnit4.color2 = new string[2] { "color1", "color2" };
					pocColorModConfigUnit4.banner = "dummy banner";
					pocColorModConfigUnit4.banners = new string[3] { "banner1", "banner2", "banner3" };
					pocColorModConfigUnit4.shields = new string[3] { "shield1", "shield2", "shield3" };
					pocColorModConfigClan2.units.TryAdd(((object)item9.Name).ToString(), pocColorModConfigUnit4);
				}
				pocColorModConfig.clans.TryAdd(((object)item7.Name).ToString(), pocColorModConfigClan2);
			}
			JsonSerializer val2 = new JsonSerializer
			{
				NullValueHandling = (NullValueHandling)1
			};
			using (StreamWriter streamWriter = new StreamWriter("..\\..\\modules\\PocColor\\config.json.full.template"))
			{
				val2.Serialize((TextWriter)streamWriter, (object)pocColorModConfig);
			}
			Log.write("Building json template - Done ");
		}
		catch (Exception ex4)
		{
			Log.write(ex4.Message);
		}
	}
}
