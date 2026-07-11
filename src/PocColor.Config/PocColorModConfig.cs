using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace PocColor.Config;

public class PocColorModConfig : UnitsContainerSection
{
	public static Random rnd = new Random();

	public bool log { get; set; } = false;


	public bool generateTemplate { get; set; } = false;


	public PocColorModConfigColorEntry defaultConfig { get; set; }

	public Map<string, PocColorModConfigKingdom> kingdoms { get; set; }

	public Map<string, PocColorModConfigClan> clans { get; set; }

	public Map<string, PocColorModConfigUnit> units { get; set; }

	public PocColorModGroupEntry[] groups { get; set; }

	public (bool, bool, string, string, string, string) GetClanConfig(string kingdom, string clan, bool isPlayerKingdom, bool isPlayerClan)
	{
		PocColorModConfigKingdom pocColorModConfigKingdom = null;
		PocColorModConfigClan pocColorModConfigClan = null;
		PocColorModConfigClan pocColorModConfigClan2 = null;
		string text = null;
		string text2 = "false";
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		bool result = false;
		bool result2 = false;
		try
		{
			pocColorModConfigKingdom = kingdoms?[kingdom];
			if (pocColorModConfigKingdom == null && isPlayerKingdom)
			{
				pocColorModConfigKingdom = kingdoms?["PlayerKingdom"];
			}
			pocColorModConfigClan2 = clans?[clan];
			if (pocColorModConfigClan2 == null && isPlayerClan)
			{
				pocColorModConfigClan2 = clans?["PlayerClan"];
			}
			pocColorModConfigClan = pocColorModConfigKingdom?.clans?[clan];
			if (pocColorModConfigClan == null && isPlayerClan)
			{
				pocColorModConfigClan = pocColorModConfigKingdom?.clans?["PlayerClan"];
			}
			text3 = pocColorModConfigKingdom?.clanBanner ?? text3;
			text3 = pocColorModConfigClan2?.clanBanner ?? text3;
			text3 = pocColorModConfigClan?.clanBanner ?? text3;
			text4 = pocColorModConfigKingdom?.clanShield ?? text4;
			text4 = pocColorModConfigClan2?.clanShield ?? text4;
			text4 = pocColorModConfigClan?.clanShield ?? text4;
			text = defaultConfig?.FollowKingdomColors ?? text;
			text = pocColorModConfigKingdom?.FollowKingdomColors ?? text;
			text = pocColorModConfigClan2?.FollowKingdomColors ?? text;
			text = pocColorModConfigClan?.FollowKingdomColors ?? text;
			if (text == null)
			{
				text = ((text3 == null) ? "true" : "false");
			}
			bool flag = bool.TryParse(text.ToLower(), out result);
			text2 = defaultConfig?.FollowKingdomBackgroundOnly ?? text2;
			text2 = pocColorModConfigKingdom?.FollowKingdomBackgroundOnly ?? text2;
			text2 = pocColorModConfigClan2?.FollowKingdomBackgroundOnly ?? text2;
			text2 = pocColorModConfigClan?.FollowKingdomBackgroundOnly ?? text2;
			flag = bool.TryParse(text2.ToLower(), out result2);
			text5 = pocColorModConfigClan2?.primaryColor ?? text5;
			text5 = pocColorModConfigClan?.primaryColor ?? text5;
			text6 = pocColorModConfigClan2?.secondaryColor ?? text6;
			text6 = pocColorModConfigClan?.secondaryColor ?? text6;
			Map<string, string[]>[] array = defaultConfig?.vars ?? null;
			array = pocColorModConfigKingdom?.vars ?? array;
			array = pocColorModConfigClan2?.vars ?? array;
			array = pocColorModConfigClan?.vars ?? array;
			Map<string, string> map = null;
			if (array != null)
			{
				map = GetVarsValues(array);
			}
			if (map != null)
			{
				text3 = resolveBanner(text3, map);
				text4 = resolveBanner(text4, map);
			}
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
		return (result, result2, text3, text4, text5, text6);
	}

	public (string, string, string, string) GetKingdomConfig(string kingdom, bool isPlayerKingdom)
	{
		PocColorModConfigKingdom pocColorModConfigKingdom = null;
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		try
		{
			pocColorModConfigKingdom = kingdoms?[kingdom];
			if (pocColorModConfigKingdom == null && isPlayerKingdom)
			{
				pocColorModConfigKingdom = kingdoms?["PlayerKingdom"];
			}
			text = pocColorModConfigKingdom?.kingdomBanner ?? text;
			text2 = pocColorModConfigKingdom?.kingdomShield ?? text2;
			text3 = pocColorModConfigKingdom?.primaryColor ?? text3;
			text4 = pocColorModConfigKingdom?.secondaryColor ?? text4;
			Map<string, string[]>[] array = defaultConfig?.vars ?? null;
			array = pocColorModConfigKingdom?.vars ?? array;
			Map<string, string> map = null;
			if (array != null)
			{
				map = GetVarsValues(array);
			}
			if (map != null)
			{
				text = resolveBanner(text, map);
				text2 = resolveBanner(text2, map);
			}
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
		return (text, text2, text3, text4);
	}

	public (bool, int, string[], string[], string[], string[]) GetRulerConfig(string clan, int modeOrig, string[] colorOrig, string[] colorOrig2, string[] bannerOrig, string[] shieldOrig, PocColorModConfigKingdom kConfig, PocColorModConfigClan kcConfig, PocColorModConfigUnit kuConfig, PocColorModConfigUnit kcuConfig, PocColorModConfigClan cConfig, PocColorModConfigUnit cuConfig, PocColorModConfigUnit uConfig)
	{
		int num = modeOrig;
		string[] array = colorOrig;
		string[] array2 = colorOrig2;
		string[] item = bannerOrig;
		string[] item2 = shieldOrig;
		string text = null;
		string text2 = null;
		string text3 = defaultConfig?.BearRulerColors ?? "true";
		string text4 = defaultConfig?.BearRulerBanner ?? "true";
		string text5 = defaultConfig?.BearRulerShield ?? "true";
		bool flag = false;
		bool flag2 = false;
		string text6 = null;
		try
		{
			if (clan != null && !clan.Equals(""))
			{
				foreach (Clan item3 in (List<Clan>)(object)Campaign.Current.Clans)
				{
					if (((object)item3.Name).ToString().Equals(clan))
					{
						Kingdom kingdom = item3.Kingdom;
						if (((kingdom != null) ? kingdom.RulingClan : null) == item3)
						{
							flag2 = true;
							text6 = PocColorMod.SerializeBanner(item3.Kingdom.Banner);
						}
					}
				}
			}
			if (!flag2)
			{
				return (false, num, array, array2, item, item2);
			}
			text3 = kConfig?.BearRulerColors ?? text3;
			text3 = cConfig?.BearRulerColors ?? text3;
			text3 = kcConfig?.BearRulerColors ?? text3;
			text3 = uConfig?.BearRulerColors ?? text3;
			text3 = kuConfig?.BearRulerColors ?? text3;
			text3 = cuConfig?.BearRulerColors ?? text3;
			text3 = kcuConfig?.BearRulerColors ?? text3;
			bool.TryParse(text3.ToLower(), out var result);
			text4 = kConfig?.BearRulerBanner ?? text4;
			text4 = cConfig?.BearRulerBanner ?? text4;
			text4 = kcConfig?.BearRulerBanner ?? text4;
			text4 = uConfig?.BearRulerBanner ?? text4;
			text4 = kuConfig?.BearRulerBanner ?? text4;
			text4 = cuConfig?.BearRulerBanner ?? text4;
			text4 = kcuConfig?.BearRulerBanner ?? text4;
			bool.TryParse(text4.ToLower(), out var result2);
			text5 = kConfig?.BearRulerShield ?? text5;
			text5 = cConfig?.BearRulerShield ?? text5;
			text5 = kcConfig?.BearRulerShield ?? text5;
			text5 = uConfig?.BearRulerShield ?? text5;
			text5 = kuConfig?.BearRulerShield ?? text5;
			text5 = cuConfig?.BearRulerShield ?? text5;
			text5 = kcuConfig?.BearRulerShield ?? text5;
			bool.TryParse(text5.ToLower(), out var result3);
			if (result)
			{
				num = kConfig?.rulerMode ?? 2;
				num = cConfig?.rulerMode ?? num;
				num = kcConfig?.rulerMode ?? num;
				num = uConfig?.rulerMode ?? num;
				num = kuConfig?.rulerMode ?? num;
				num = cuConfig?.rulerMode ?? num;
				num = kcuConfig?.rulerMode ?? num;
				array = kConfig?.rulerColor;
				array = cConfig?.rulerColor ?? array;
				array = kcConfig?.rulerColor ?? array;
				array = uConfig?.rulerColor ?? array;
				array = kuConfig?.rulerColor ?? array;
				array = cuConfig?.rulerColor ?? array;
				array = kcuConfig?.rulerColor ?? array;
				array2 = kConfig?.rulerColor2;
				array2 = cConfig?.rulerColor2 ?? array2;
				array2 = kcConfig?.rulerColor2 ?? array2;
				array2 = uConfig?.rulerColor2 ?? array2;
				array2 = kuConfig?.rulerColor2 ?? array2;
				array2 = cuConfig?.rulerColor2 ?? array2;
				array2 = kcuConfig?.rulerColor2 ?? array2;
			}
			if (result2)
			{
				text = text6;
				text = kConfig?.rulerBanner ?? text;
				text = cConfig?.rulerBanner ?? text;
				text = kcConfig?.rulerBanner ?? text;
				text = uConfig?.rulerBanner ?? text;
				text = kuConfig?.rulerBanner ?? text;
				text = cuConfig?.rulerBanner ?? text;
				text = kcuConfig?.rulerBanner ?? text;
				item = new string[1] { text };
			}
			if (result3)
			{
				text2 = text6;
				text2 = kConfig?.rulerShield ?? text2;
				text2 = cConfig?.rulerShield ?? text2;
				text2 = kcConfig?.rulerShield ?? text2;
				text2 = uConfig?.rulerShield ?? text2;
				text2 = kuConfig?.rulerShield ?? text2;
				text2 = cuConfig?.rulerShield ?? text2;
				text2 = kcuConfig?.rulerShield ?? text2;
				item2 = new string[1] { text2 };
			}
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
		return (true, num, array, array2, item, item2);
	}

	public (int, string[], string[], string[], string[]) GetConfig(string kingdom, string clan, string unit, bool isPlayerKingdom, bool isPlayerClan, bool isPlayer, bool isKing, bool isLeader, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		int num = 0;
		string[] array = null;
		string[] array2 = null;
		string text = null;
		string[] array3 = null;
		string[] array4 = null;
		string[] array5 = null;
		PocColorModConfigKingdom kConfig = null;
		PocColorModConfigClan kcConfig = null;
		PocColorModConfigUnit kuConfig = null;
		PocColorModConfigUnit kcuConfig = null;
		PocColorModConfigClan cConfig = null;
		PocColorModConfigUnit cuConfig = null;
		PocColorModConfigUnit uConfig = null;
		try
		{
			(PocColorModConfigKingdom, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit) configScopes = getConfigScopes(kConfig, kcConfig, kuConfig, kcuConfig, cConfig, cuConfig, uConfig, kingdom, clan, unit, isPlayerKingdom, isPlayerClan, isPlayer, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
			kConfig = configScopes.Item1;
			kcConfig = configScopes.Item2;
			kuConfig = configScopes.Item3;
			kcuConfig = configScopes.Item4;
			cConfig = configScopes.Item5;
			cuConfig = configScopes.Item6;
			uConfig = configScopes.Item7;
			Map<string, string[]>[] array6 = defaultConfig?.vars ?? null;
			array6 = kConfig?.vars ?? array6;
			array6 = cConfig?.vars ?? array6;
			array6 = kcConfig?.vars ?? array6;
			array6 = uConfig?.vars ?? array6;
			array6 = kuConfig?.vars ?? array6;
			array6 = cuConfig?.vars ?? array6;
			array6 = kcuConfig?.vars ?? array6;
			num = defaultConfig?.mode ?? 2;
			num = kConfig?.mode ?? num;
			num = cConfig?.mode ?? num;
			num = kcConfig?.mode ?? num;
			num = uConfig?.mode ?? num;
			num = kuConfig?.mode ?? num;
			num = cuConfig?.mode ?? num;
			num = kcuConfig?.mode ?? num;
			array = defaultConfig?.color;
			array = kConfig?.color ?? array;
			array = cConfig?.color ?? array;
			array = kcConfig?.color ?? array;
			array = uConfig?.color ?? array;
			array = kuConfig?.color ?? array;
			array = cuConfig?.color ?? array;
			array = kcuConfig?.color ?? array;
			array2 = defaultConfig?.color2;
			array2 = kConfig?.color2 ?? array2;
			array2 = cConfig?.color2 ?? array2;
			array2 = kcConfig?.color2 ?? array2;
			array2 = uConfig?.color2 ?? array2;
			array2 = kuConfig?.color2 ?? array2;
			array2 = cuConfig?.color2 ?? array2;
			array2 = kcuConfig?.color2 ?? array2;
			text = kConfig?.banner;
			text = cConfig?.banner ?? text;
			text = kcConfig?.banner ?? text;
			text = uConfig?.banner ?? text;
			text = kuConfig?.banner ?? text;
			text = cuConfig?.banner ?? text;
			text = kcuConfig?.banner ?? text;
			array3 = kConfig?.shields;
			array3 = cConfig?.shields ?? array3;
			array3 = kcConfig?.shields ?? array3;
			array3 = uConfig?.shields ?? array3;
			array3 = kuConfig?.shields ?? array3;
			array3 = cuConfig?.shields ?? array3;
			array3 = kcuConfig?.shields ?? array3;
			array4 = kConfig?.banners;
			array4 = cConfig?.banners ?? array4;
			array4 = kcConfig?.banners ?? array4;
			array4 = uConfig?.banners ?? array4;
			array4 = kuConfig?.banners ?? array4;
			array4 = cuConfig?.banners ?? array4;
			array4 = kcuConfig?.banners ?? array4;
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple = parseUnitsDetails(uConfig, isMounted, isRanged, tier, culture, array6, num, array, array2, text, array3, array4, null, null);
			array6 = tuple.Item1;
			num = tuple.Item2;
			array = tuple.Item3;
			array2 = tuple.Item4;
			text = tuple.Item5;
			array3 = tuple.Item6;
			array4 = tuple.Item7;
			string[] item = tuple.Rest.Item1;
			string[] item2 = tuple.Rest.Item2;
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple2 = parseUnitsDetails(kuConfig, isMounted, isRanged, tier, culture, array6, num, array, array2, text, array3, array4, null, null);
			array6 = tuple2.Item1;
			num = tuple2.Item2;
			array = tuple2.Item3;
			array2 = tuple2.Item4;
			text = tuple2.Item5;
			array3 = tuple2.Item6;
			array4 = tuple2.Item7;
			item = tuple2.Rest.Item1;
			item2 = tuple2.Rest.Item2;
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple3 = parseUnitsDetails(cuConfig, isMounted, isRanged, tier, culture, array6, num, array, array2, text, array3, array4, null, null);
			array6 = tuple3.Item1;
			num = tuple3.Item2;
			array = tuple3.Item3;
			array2 = tuple3.Item4;
			text = tuple3.Item5;
			array3 = tuple3.Item6;
			array4 = tuple3.Item7;
			item = tuple3.Rest.Item1;
			item2 = tuple3.Rest.Item2;
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple4 = parseUnitsDetails(kcuConfig, isMounted, isRanged, tier, culture, array6, num, array, array2, text, array3, array4, null, null);
			array6 = tuple4.Item1;
			num = tuple4.Item2;
			array = tuple4.Item3;
			array2 = tuple4.Item4;
			text = tuple4.Item5;
			array3 = tuple4.Item6;
			array4 = tuple4.Item7;
			item = tuple4.Rest.Item1;
			item2 = tuple4.Rest.Item2;
			if (!string.IsNullOrEmpty(text) && array4 != null && array4.Length != 0)
			{
				array5 = new string[array4.Length + 1];
				array4.CopyTo(array5, 0);
				array5[array4.Length] = text;
			}
			else if (array4 != null && array4.Length != 0)
			{
				array5 = array4;
			}
			else if (!string.IsNullOrEmpty(text))
			{
				array5 = new string[1] { text };
			}
			bool flag = false;
			(bool, int, string[], string[], string[], string[]) rulerConfig = GetRulerConfig(clan, num, array, array2, array5, array3, kConfig, kcConfig, kuConfig, kcuConfig, cConfig, cuConfig, uConfig);
			flag = rulerConfig.Item1;
			num = rulerConfig.Item2;
			array = rulerConfig.Item3;
			array2 = rulerConfig.Item4;
			array5 = rulerConfig.Item5;
			array3 = rulerConfig.Item6;
			Map<string, string> values = null;
			if (array6 != null)
			{
				values = GetVarsValues(array6);
			}
			if (values != null)
			{
				array5 = Array.ConvertAll(array5, (string x) => resolveBanner(x, values));
				array3 = Array.ConvertAll(array3, (string x) => resolveBanner(x, values));
			}
		}
		catch (Exception ex)
		{
			Log.write("Error while retrieving config: " + ex.Message);
		}
		return (num, array, array2, array5, array3);
	}

	private (Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) parseUnitsDetails(ConfigSectionUnit unitsConfig, bool isMounted, bool isRanged, int tier, string culture, Map<string, string[]>[] vars, int mode, string[] color, string[] color2, string banner, string[] shields, string[] banners, string[] combatshields, string[] combatbanners)
	{
		if (unitsConfig == null)
		{
			return (vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
		}
		if (unitsConfig.cultures != null && culture != null)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple = parseCriteria(unitsConfig.cultures[culture], vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
			vars = tuple.Item1;
			mode = tuple.Item2;
			color = tuple.Item3;
			color2 = tuple.Item4;
			banner = tuple.Item5;
			shields = tuple.Item6;
			banners = tuple.Item7;
			combatshields = tuple.Rest.Item1;
			combatbanners = tuple.Rest.Item2;
		}
		(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple2 = parseTypeCriteria(unitsConfig, isMounted, isRanged, vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
		vars = tuple2.Item1;
		mode = tuple2.Item2;
		color = tuple2.Item3;
		color2 = tuple2.Item4;
		banner = tuple2.Item5;
		shields = tuple2.Item6;
		banners = tuple2.Item7;
		combatshields = tuple2.Rest.Item1;
		combatbanners = tuple2.Rest.Item2;
		if (unitsConfig.tiers != null)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple3 = parseCriteria(unitsConfig.tiers[tier], vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
			vars = tuple3.Item1;
			mode = tuple3.Item2;
			color = tuple3.Item3;
			color2 = tuple3.Item4;
			banner = tuple3.Item5;
			shields = tuple3.Item6;
			banners = tuple3.Item7;
			combatshields = tuple3.Rest.Item1;
			combatbanners = tuple3.Rest.Item2;
		}
		if (unitsConfig.cultures != null && culture != null && unitsConfig.cultures[culture] != null)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple4 = parseTypeCriteria(unitsConfig.cultures[culture], isMounted, isRanged, vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
			vars = tuple4.Item1;
			mode = tuple4.Item2;
			color = tuple4.Item3;
			color2 = tuple4.Item4;
			banner = tuple4.Item5;
			shields = tuple4.Item6;
			banners = tuple4.Item7;
			combatshields = tuple4.Rest.Item1;
			combatbanners = tuple4.Rest.Item2;
			if (unitsConfig.cultures[culture].tiers != null)
			{
				(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple5 = parseCriteria(unitsConfig.cultures[culture].tiers[tier], vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
				vars = tuple5.Item1;
				mode = tuple5.Item2;
				color = tuple5.Item3;
				color2 = tuple5.Item4;
				banner = tuple5.Item5;
				shields = tuple5.Item6;
				banners = tuple5.Item7;
				combatshields = tuple5.Rest.Item1;
				combatbanners = tuple5.Rest.Item2;
			}
		}
		if (unitsConfig.tiers != null)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple6 = parseTypeCriteria(unitsConfig.tiers[tier], isMounted, isRanged, vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
			vars = tuple6.Item1;
			mode = tuple6.Item2;
			color = tuple6.Item3;
			color2 = tuple6.Item4;
			banner = tuple6.Item5;
			shields = tuple6.Item6;
			banners = tuple6.Item7;
			combatshields = tuple6.Rest.Item1;
			combatbanners = tuple6.Rest.Item2;
		}
		if (unitsConfig.cultures != null && culture != null && unitsConfig.cultures[culture] != null && unitsConfig.cultures[culture].tiers != null)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple7 = parseTypeCriteria(unitsConfig.cultures[culture].tiers[tier], isMounted, isRanged, vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
			vars = tuple7.Item1;
			mode = tuple7.Item2;
			color = tuple7.Item3;
			color2 = tuple7.Item4;
			banner = tuple7.Item5;
			shields = tuple7.Item6;
			banners = tuple7.Item7;
			combatshields = tuple7.Rest.Item1;
			combatbanners = tuple7.Rest.Item2;
		}
		return (vars, mode, color, color2, banner, shields, banners, combatshields, combatbanners);
	}

	private (Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) parseCriteria(ConfigSection unitsConfig, Map<string, string[]>[] vars, int mode, string[] color, string[] color2, string banner, string[] shields, string[] banners, string[] combatShields, string[] combatBanners)
	{
		if (unitsConfig != null)
		{
			vars = unitsConfig.vars ?? vars;
			mode = unitsConfig.mode ?? mode;
			color = unitsConfig.color ?? color;
			color2 = unitsConfig.color2 ?? color2;
			banner = unitsConfig.banner ?? banner;
			banners = unitsConfig.banners ?? banners;
			shields = unitsConfig.shields ?? shields;
			combatShields = unitsConfig?.combatShields ?? combatShields;
			combatBanners = unitsConfig?.combatBanners ?? combatBanners;
		}
		return (vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
	}

	private (Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) parseTypeCriteria(ConfigSectionTier unitsConfig, bool isMounted, bool isRanged, Map<string, string[]>[] vars, int mode, string[] color, string[] color2, string banner, string[] shields, string[] banners, string[] combatShields, string[] combatBanners)
	{
		if (unitsConfig == null || unitsConfig.types == null)
		{
			return (vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
		}
		if (isMounted)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple = parseCriteria(unitsConfig.types["Mounted"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple.Item1;
			mode = tuple.Item2;
			color = tuple.Item3;
			color2 = tuple.Item4;
			banner = tuple.Item5;
			shields = tuple.Item6;
			banners = tuple.Item7;
			combatShields = tuple.Rest.Item1;
			combatBanners = tuple.Rest.Item2;
		}
		else
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple2 = parseCriteria(unitsConfig.types["Foot"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple2.Item1;
			mode = tuple2.Item2;
			color = tuple2.Item3;
			color2 = tuple2.Item4;
			banner = tuple2.Item5;
			shields = tuple2.Item6;
			banners = tuple2.Item7;
			combatShields = tuple2.Rest.Item1;
			combatBanners = tuple2.Rest.Item2;
		}
		if (isRanged)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple3 = parseCriteria(unitsConfig.types["Ranged"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple3.Item1;
			mode = tuple3.Item2;
			color = tuple3.Item3;
			color2 = tuple3.Item4;
			banner = tuple3.Item5;
			shields = tuple3.Item6;
			banners = tuple3.Item7;
			combatShields = tuple3.Rest.Item1;
			combatBanners = tuple3.Rest.Item2;
		}
		else
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple4 = parseCriteria(unitsConfig.types["Melee"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple4.Item1;
			mode = tuple4.Item2;
			color = tuple4.Item3;
			color2 = tuple4.Item4;
			banner = tuple4.Item5;
			shields = tuple4.Item6;
			banners = tuple4.Item7;
			combatShields = tuple4.Rest.Item1;
			combatBanners = tuple4.Rest.Item2;
		}
		if (isMounted && isRanged)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple5 = parseCriteria(unitsConfig.types["HorseArcher"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple5.Item1;
			mode = tuple5.Item2;
			color = tuple5.Item3;
			color2 = tuple5.Item4;
			banner = tuple5.Item5;
			shields = tuple5.Item6;
			banners = tuple5.Item7;
			combatShields = tuple5.Rest.Item1;
			combatBanners = tuple5.Rest.Item2;
		}
		if (!isMounted && isRanged)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple6 = parseCriteria(unitsConfig.types["Archer"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple6.Item1;
			mode = tuple6.Item2;
			color = tuple6.Item3;
			color2 = tuple6.Item4;
			banner = tuple6.Item5;
			shields = tuple6.Item6;
			banners = tuple6.Item7;
			combatShields = tuple6.Rest.Item1;
			combatBanners = tuple6.Rest.Item2;
		}
		if (isMounted && !isRanged)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple7 = parseCriteria(unitsConfig.types["Cavalry"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple7.Item1;
			mode = tuple7.Item2;
			color = tuple7.Item3;
			color2 = tuple7.Item4;
			banner = tuple7.Item5;
			shields = tuple7.Item6;
			banners = tuple7.Item7;
			combatShields = tuple7.Rest.Item1;
			combatBanners = tuple7.Rest.Item2;
		}
		if (!isMounted && !isRanged)
		{
			(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple8 = parseCriteria(unitsConfig.types["Infantry"], vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
			vars = tuple8.Item1;
			mode = tuple8.Item2;
			color = tuple8.Item3;
			color2 = tuple8.Item4;
			banner = tuple8.Item5;
			shields = tuple8.Item6;
			banners = tuple8.Item7;
			combatShields = tuple8.Rest.Item1;
			combatBanners = tuple8.Rest.Item2;
		}
		return (vars, mode, color, color2, banner, shields, banners, combatShields, combatBanners);
	}

	public (int, string[], string[], string[], string[]) GetBattleConfig(string kingdom, string clan, string unit, bool isPlayerKingdom, bool isPlayerClan, bool isPlayer, bool isKing, bool isLeader, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		int num = 0;
		string[] array = null;
		string[] array2 = null;
		string[] array3 = null;
		string[] array4 = null;
		string[] array5 = null;
		string[] array6 = null;
		PocColorModConfigKingdom kConfig = null;
		PocColorModConfigClan kcConfig = null;
		PocColorModConfigUnit kuConfig = null;
		PocColorModConfigUnit kcuConfig = null;
		PocColorModConfigClan cConfig = null;
		PocColorModConfigUnit cuConfig = null;
		PocColorModConfigUnit uConfig = null;
		(PocColorModConfigKingdom, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit) configScopes = getConfigScopes(kConfig, kcConfig, kuConfig, kcuConfig, cConfig, cuConfig, uConfig, kingdom, clan, unit, isPlayerKingdom, isPlayerClan, isPlayer, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
		kConfig = configScopes.Item1;
		kcConfig = configScopes.Item2;
		kuConfig = configScopes.Item3;
		kcuConfig = configScopes.Item4;
		cConfig = configScopes.Item5;
		cuConfig = configScopes.Item6;
		uConfig = configScopes.Item7;
		(int, string[], string[], string[], string[]) config = GetConfig(kingdom, clan, unit, isPlayerKingdom, isPlayerClan, isPlayer, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
		num = config.Item1;
		array = config.Item2;
		array2 = config.Item3;
		array4 = config.Item4;
		array3 = config.Item5;
		Map<string, string[]>[] array7 = defaultConfig?.vars ?? null;
		array7 = kConfig?.vars ?? array7;
		array7 = cConfig?.vars ?? array7;
		array7 = kcConfig?.vars ?? array7;
		array7 = uConfig?.vars ?? array7;
		array7 = kuConfig?.vars ?? array7;
		array7 = cuConfig?.vars ?? array7;
		array7 = kcuConfig?.vars ?? array7;
		array5 = kConfig?.combatShields;
		array5 = cConfig?.combatShields ?? array5;
		array5 = kcConfig?.combatShields ?? array5;
		array5 = uConfig?.combatShields ?? array5;
		array5 = kuConfig?.combatShields ?? array5;
		array5 = cuConfig?.combatShields ?? array5;
		array5 = kcuConfig?.combatShields ?? array5;
		array6 = kConfig?.combatBanners ?? array6;
		array6 = cConfig?.combatBanners ?? array6;
		array6 = kcConfig?.combatBanners ?? array6;
		array6 = uConfig?.combatBanners ?? array6;
		array6 = kuConfig?.combatBanners ?? array6;
		array6 = cuConfig?.combatBanners ?? array6;
		array6 = kcuConfig?.combatBanners ?? array6;
		(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple = parseUnitsDetails(uConfig, isMounted, isRanged, tier, culture, array7, num, array, array2, null, array3, array4, array5, array6);
		array7 = tuple.Item1;
		num = tuple.Item2;
		array = tuple.Item3;
		array2 = tuple.Item4;
		string item = tuple.Item5;
		array3 = tuple.Item6;
		array4 = tuple.Item7;
		array5 = tuple.Rest.Item1;
		array6 = tuple.Rest.Item2;
		(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple2 = parseUnitsDetails(kuConfig, isMounted, isRanged, tier, culture, array7, num, array, array2, null, array3, array4, array5, array6);
		array7 = tuple2.Item1;
		num = tuple2.Item2;
		array = tuple2.Item3;
		array2 = tuple2.Item4;
		item = tuple2.Item5;
		array3 = tuple2.Item6;
		array4 = tuple2.Item7;
		array5 = tuple2.Rest.Item1;
		array6 = tuple2.Rest.Item2;
		(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple3 = parseUnitsDetails(cuConfig, isMounted, isRanged, tier, culture, array7, num, array, array2, null, array3, array4, array5, array6);
		array7 = tuple3.Item1;
		num = tuple3.Item2;
		array = tuple3.Item3;
		array2 = tuple3.Item4;
		item = tuple3.Item5;
		array3 = tuple3.Item6;
		array4 = tuple3.Item7;
		array5 = tuple3.Rest.Item1;
		array6 = tuple3.Rest.Item2;
		(Map<string, string[]>[], int, string[], string[], string, string[], string[], string[], string[]) tuple4 = parseUnitsDetails(kcuConfig, isMounted, isRanged, tier, culture, array7, num, array, array2, null, array3, array4, array5, array6);
		array7 = tuple4.Item1;
		num = tuple4.Item2;
		array = tuple4.Item3;
		array2 = tuple4.Item4;
		item = tuple4.Item5;
		array3 = tuple4.Item6;
		array4 = tuple4.Item7;
		array5 = tuple4.Rest.Item1;
		array6 = tuple4.Rest.Item2;
		Map<string, string> values = null;
		if (array7 != null)
		{
			values = GetVarsValues(array7);
		}
		if (array5 != null && array5.Length != 0)
		{
			array3 = array5;
		}
		if (array6 != null && array6.Length != 0)
		{
			array4 = array6;
		}
		if (values != null)
		{
			array4 = Array.ConvertAll(array4, (string x) => resolveBanner(x, values));
			array3 = Array.ConvertAll(array3, (string x) => resolveBanner(x, values));
		}
		return (num, array, array2, array4, array3);
	}

	public Map<string, string[]>[] GetVars(string kingdom, string clan, string unit, bool isPlayerKingdom, bool isPlayerClan, bool isPlayer, bool isKing, bool isLeader, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		PocColorModConfigKingdom kConfig = null;
		PocColorModConfigClan kcConfig = null;
		PocColorModConfigUnit kuConfig = null;
		PocColorModConfigUnit kcuConfig = null;
		PocColorModConfigClan cConfig = null;
		PocColorModConfigUnit cuConfig = null;
		PocColorModConfigUnit uConfig = null;
		(PocColorModConfigKingdom, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit) configScopes = getConfigScopes(kConfig, kcConfig, kuConfig, kcuConfig, cConfig, cuConfig, uConfig, kingdom, clan, unit, isPlayerKingdom, isPlayerClan, isPlayer, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
		kConfig = configScopes.Item1;
		kcConfig = configScopes.Item2;
		kuConfig = configScopes.Item3;
		kcuConfig = configScopes.Item4;
		cConfig = configScopes.Item5;
		cuConfig = configScopes.Item6;
		uConfig = configScopes.Item7;
		Map<string, string[]>[] array = defaultConfig?.vars ?? null;
		array = kConfig?.vars ?? array;
		array = cConfig?.vars ?? array;
		array = kcConfig?.vars ?? array;
		array = uConfig?.vars ?? array;
		array = kuConfig?.vars ?? array;
		array = cuConfig?.vars ?? array;
		return kcuConfig?.vars ?? array;
	}

	public string[] GetVarList(Map<string, string[]>[] vars)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Map<string, string[]> map in vars)
		{
			hashSet.UnionWith(map.Keys);
		}
		string[] array = new string[hashSet.Count];
		hashSet.CopyTo(array);
		return array;
	}

	public Map<string, string> GetVarsValues(Map<string, string[]>[] vars)
	{
		Map<string, string> map = new Map<string, string>();
		foreach (Map<string, string[]> map2 in vars)
		{
			int num = 100000;
			foreach (string key in map2.Keys)
			{
				string[] array = map2[key];
				num = Math.Min(num, array.Length);
			}
			int num2 = rnd.Next(0, num);
			foreach (string key2 in map2.Keys)
			{
				string[] array2 = map2[key2];
				map[key2] = array2[num2];
			}
		}
		return map;
	}

	public string resolveBanner(string bannerText, Map<string, string> values)
	{
		string text = bannerText;
		foreach (string key in values.Keys)
		{
			text = text.Replace(key, values[key]);
		}
		return text;
	}

	public (PocColorModConfigKingdom, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit, PocColorModConfigClan, PocColorModConfigUnit, PocColorModConfigUnit) getConfigScopes(PocColorModConfigKingdom kConfig, PocColorModConfigClan kcConfig, PocColorModConfigUnit kuConfig, PocColorModConfigUnit kcuConfig, PocColorModConfigClan cConfig, PocColorModConfigUnit cuConfig, PocColorModConfigUnit uConfig, string kingdom, string clan, string unit, bool isPlayerKingdom, bool isPlayerClan, bool isPlayer, bool isKing, bool isLeader, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		kConfig = kingdoms?[kingdom];
		if (kConfig == null && isPlayerKingdom)
		{
			kConfig = kingdoms?["PlayerKingdom"];
		}
		cConfig = clans?[clan];
		if (cConfig == null && isPlayerClan)
		{
			cConfig = clans?["PlayerClan"];
		}
		kcConfig = kConfig?.clans?[clan];
		if (kcConfig == null && isPlayerClan)
		{
			kcConfig = kConfig?.clans?["PlayerClan"];
		}
		uConfig = parseUnits(this, unit, isPlayer, isKing, isLeader, isHero);
		kuConfig = parseUnits(kConfig, unit, isPlayer, isKing, isLeader, isHero);
		cuConfig = parseUnits(cConfig, unit, isPlayer, isKing, isLeader, isHero);
		kcuConfig = parseUnits(kcConfig, unit, isPlayer, isKing, isLeader, isHero);
		return (kConfig, kcConfig, kuConfig, kcuConfig, cConfig, cuConfig, uConfig);
	}

	private PocColorModConfigUnit parseUnits(UnitsContainerSection section, string unit, bool isPlayer, bool isKing, bool isLeader, bool isHero)
	{
		PocColorModConfigUnit pocColorModConfigUnit = section?.units?[unit];
		if (pocColorModConfigUnit == null)
		{
			if (isPlayer)
			{
				pocColorModConfigUnit = section?.units?["Player"];
			}
			if (pocColorModConfigUnit == null)
			{
				if (isKing)
				{
					pocColorModConfigUnit = section?.units?["King"];
				}
				if (pocColorModConfigUnit == null)
				{
					if (isLeader)
					{
						pocColorModConfigUnit = section?.units?["Leader"];
					}
					if (pocColorModConfigUnit == null)
					{
						if (isHero)
						{
							pocColorModConfigUnit = section?.units?["Hero"];
						}
						else if (pocColorModConfigUnit == null && !isHero)
						{
							pocColorModConfigUnit = section?.units?["Trooper"];
						}
					}
				}
			}
		}
		return pocColorModConfigUnit;
	}

	public void parseGroups()
	{
		if (groups != null)
		{
			PocColorModGroupEntry[] array = groups;
			foreach (PocColorModGroupEntry pocColorModGroupEntry in array)
			{
				Map<string, PocColorModConfigUnit> map = pocColorModGroupEntry.toUnits();
				if (units != null)
				{
					foreach (string key in units.Keys)
					{
						map[key] = units[key];
					}
				}
				units = map;
			}
		}
		if (clans != null)
		{
			foreach (string key2 in clans.Keys)
			{
				PocColorModConfigClan pocColorModConfigClan = clans[key2];
				if (pocColorModConfigClan.groups == null)
				{
					continue;
				}
				PocColorModGroupEntry[] array2 = pocColorModConfigClan.groups;
				foreach (PocColorModGroupEntry pocColorModGroupEntry2 in array2)
				{
					Map<string, PocColorModConfigUnit> map2 = pocColorModGroupEntry2.toUnits();
					if (clans[key2].units != null)
					{
						foreach (string key3 in clans[key2].units.Keys)
						{
							map2[key3] = clans[key2].units[key3];
						}
					}
					clans[key2].units = map2;
				}
			}
		}
		if (kingdoms == null)
		{
			return;
		}
		foreach (string key4 in kingdoms.Keys)
		{
			PocColorModConfigKingdom pocColorModConfigKingdom = kingdoms[key4];
			if (pocColorModConfigKingdom.groups != null)
			{
				PocColorModGroupEntry[] array3 = pocColorModConfigKingdom.groups;
				foreach (PocColorModGroupEntry pocColorModGroupEntry3 in array3)
				{
					Map<string, PocColorModConfigUnit> map3 = pocColorModGroupEntry3.toUnits();
					if (kingdoms[key4].units != null)
					{
						foreach (string key5 in kingdoms[key4].units.Keys)
						{
							map3[key5] = kingdoms[key4].units[key5];
						}
					}
					kingdoms[key4].units = map3;
				}
			}
			if (pocColorModConfigKingdom.clans == null)
			{
				continue;
			}
			foreach (string key6 in pocColorModConfigKingdom.clans.Keys)
			{
				PocColorModConfigClan pocColorModConfigClan2 = pocColorModConfigKingdom.clans[key6];
				if (pocColorModConfigClan2.groups == null)
				{
					continue;
				}
				PocColorModGroupEntry[] array4 = pocColorModConfigClan2.groups;
				foreach (PocColorModGroupEntry pocColorModGroupEntry4 in array4)
				{
					Map<string, PocColorModConfigUnit> map4 = pocColorModGroupEntry4.toUnits();
					if (pocColorModConfigKingdom.clans[key6].units != null)
					{
						foreach (string key7 in pocColorModConfigKingdom.clans[key6].units.Keys)
						{
							map4[key7] = pocColorModConfigKingdom.clans[key6].units[key7];
						}
					}
					kingdoms[key4].clans[key6].units = map4;
				}
			}
		}
	}
}
