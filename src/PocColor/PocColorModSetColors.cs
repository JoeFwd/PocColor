using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Helpers;
using PocColor.Config;
using SandBox.GauntletUI.BannerEditor;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TroopSelection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Scripts;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.ObjectSystem;

namespace PocColor;

internal class PocColorModSetColors
{
	[HarmonyPatch(typeof(AgentBuildData), "Banner")]
	internal class PocColorModColorsFromBanner
	{
		public static bool Prefix(ref AgentBuildData __instance, ref Banner banner)
		{
			try
			{
				savedColorData = null;
				BasicCharacterObject agentCharacter = __instance.AgentCharacter;
				string charName = ((agentCharacter == null) ? null : ((object)((MBObjectBase)agentCharacter).GetName())?.ToString());
				if (banner != null)
				{
					CharacterObject val = CharacterObject.FindFirst((Predicate<CharacterObject>)((CharacterObject c) => ((object)((BasicCharacterObject)c).Name).ToString().Equals(charName)));
					int tier = ((val != null) ? val.Tier : 0);
					(int, uint?, uint?, string, string) battleColorFromConfig = getBattleColorFromConfig(null, ref banner, charName, agentCharacter.IsHero, agentCharacter.IsMounted, agentCharacter.IsRanged, tier, ((object)agentCharacter.Culture.Name).ToString());
					int item = battleColorFromConfig.Item1;
					uint? item2 = battleColorFromConfig.Item2;
					uint? item3 = battleColorFromConfig.Item3;
					string item4 = battleColorFromConfig.Item4;
					string item5 = battleColorFromConfig.Item5;
					savedColorData = new ColorData(item, item2, item3, item4, item5);
					if (item2.HasValue && item3.HasValue)
					{
						__instance.AgentData.ClothingColor1(item2.Value);
						__instance.AgentData.ClothingColor2(item3.Value);
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(AgentBuildData), "ClothingColor1")]
	internal class PocColorModColor1
	{
		public static bool Prefix(ref AgentBuildData __instance, ref uint color, ref AgentBuildData __result)
		{
			try
			{
				BasicCharacterObject agentCharacter = __instance.AgentCharacter;
				Banner agentBanner = __instance.AgentBanner;
				if (agentCharacter != null && agentBanner != null)
				{
					__result = __instance;
					return false;
				}
				uint color2 = __instance.AgentClothingColor2;
				updateAgentBuildDataColors(ref __instance, ref color, ref color2, first: true);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(AgentBuildData), "ClothingColor2")]
	internal class PocColorModColor2
	{
		public static bool Prefix(ref AgentBuildData __instance, ref uint color, ref AgentBuildData __result)
		{
			try
			{
				BasicCharacterObject agentCharacter = __instance.AgentCharacter;
				Banner agentBanner = __instance.AgentBanner;
				if (agentCharacter != null && agentBanner != null)
				{
					__result = __instance;
					return false;
				}
				uint color2 = __instance.AgentClothingColor1;
				updateAgentBuildDataColors(ref __instance, ref color2, ref color, first: false);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(ClanPartyItemVM), "RefreshValues")]
	internal class PocColorModClanPartyItemVM
	{
		public static bool Prefix(ClanPartyItemVM __instance, ref ColorData __state)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Expected O, but got Unknown
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Expected O, but got Unknown
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Expected O, but got Unknown
			try
			{
				string text = "";
				Banner banner = null;
				CharacterObject val = (CharacterObject)Traverse.Create((object)__instance).Field("_leader").GetValue();
				Clan clan = __instance.Party.LeaderHero.Clan;
				text = ((object)clan.Name).ToString();
				string name = __instance.Name;
				bool isRanged = ((BasicCharacterObject)val).IsRanged;
				bool isMounted = ((BasicCharacterObject)val).IsMounted;
				int tier = val.Tier;
				string culture = ((object)((BasicCultureObject)val.Culture).Name).ToString();
				(int, uint?, uint?, string, string) colorFromConfig = getColorFromConfig(text, ref banner, name, isHero: true, isMounted, isRanged, tier, culture);
				int item = colorFromConfig.Item1;
				uint? item2 = colorFromConfig.Item2;
				uint? item3 = colorFromConfig.Item3;
				string item4 = colorFromConfig.Item4;
				string item5 = colorFromConfig.Item5;
				__state = new ColorData(item, item2, item3, item4, item5);
				CharacterViewModel characterModel = __instance.CharacterModel;
				characterModel.BannerCodeText = ((object)banner).ToString();
				ImageIdentifierVM val2 = (ImageIdentifierVM)Traverse.Create((object)__instance).Field("_leaderVisual").GetValue();
				ImageIdentifier val3 = (ImageIdentifier)Traverse.Create((object)val2).Field("_imageIdentifierCode").GetValue();
				CharacterCode val4 = CharacterCode.CreateFrom(val3.Id);
				if (item2.HasValue)
				{
					val4.Color1 = item2.Value;
				}
				if (item3.HasValue)
				{
					val4.Color2 = item3.Value;
				}
				string text2 = val4.CreateNewCodeString();
				ImageIdentifierVM value = new ImageIdentifierVM(text2, (ImageIdentifierType)5);
				Traverse.Create((object)__instance).Field("_leaderVisual").SetValue((object)value);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}

		public static void Postfix(ClanPartyItemVM __instance, ref ColorData __state)
		{
			try
			{
				CharacterViewModel characterModel = __instance.CharacterModel;
				string bannerCodeText = null;
				if (__state.newshield != null)
				{
					bannerCodeText = __state.newshield;
				}
				else if (__state.newbanner != null)
				{
					bannerCodeText = __state.newbanner;
				}
				characterModel.BannerCodeText = bannerCodeText;
				if (__state.c1.HasValue)
				{
					Traverse.Create((object)characterModel).Field("_armorColor1").SetValue((object)__state.c1.Value);
				}
				if (__state != null)
				{
					Traverse.Create((object)characterModel).Field("_armorColor2").SetValue((object)__state.c2.Value);
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(ClanPartyMemberItemVM), "RefreshValues")]
	internal class PocColorModClanPartyMemberItemVM
	{
		public static bool Prefix(ClanPartyMemberItemVM __instance)
		{
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Expected O, but got Unknown
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Expected O, but got Unknown
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Expected O, but got Unknown
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Expected O, but got Unknown
			try
			{
				string clan = "";
				Banner banner = null;
				Hero heroObject = __instance.HeroObject;
				if (heroObject != null)
				{
					clan = ((object)heroObject.Clan.Name).ToString();
					banner = heroObject.ClanBanner;
				}
				string name = __instance.Name;
				CharacterObject characterObject = heroObject.CharacterObject;
				bool isRanged = ((BasicCharacterObject)characterObject).IsRanged;
				bool isMounted = ((BasicCharacterObject)characterObject).IsMounted;
				int tier = characterObject.Tier;
				string culture = ((object)((BasicCultureObject)characterObject.Culture).Name).ToString();
				var (num, num2, num3, text, text2) = getColorFromConfig(clan, ref banner, name, isHero: true, isMounted, isRanged, tier, culture);
				if (text2 != null)
				{
					banner = new Banner(text2);
				}
				else if (text != null)
				{
					banner = new Banner(text);
				}
				__instance.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(banner), true);
				ImageIdentifierVM val = (ImageIdentifierVM)Traverse.Create((object)__instance).Field("_visual").GetValue();
				ImageIdentifier val2 = (ImageIdentifier)Traverse.Create((object)val).Field("_imageIdentifierCode").GetValue();
				CharacterCode val3 = CharacterCode.CreateFrom(val2.Id);
				if (num2.HasValue)
				{
					val3.Color1 = num2.Value;
				}
				if (num3.HasValue)
				{
					val3.Color2 = num3.Value;
				}
				string text3 = val3.CreateNewCodeString();
				ImageIdentifierVM value = new ImageIdentifierVM(text3, (ImageIdentifierType)5);
				Traverse.Create((object)__instance).Field("_visual").SetValue((object)value);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PartyCharacterVM), "RefreshValues")]
	internal class PocColorModPartyCharacterVM
	{
		public static void Postfix(PartyCharacterVM __instance)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Expected O, but got Unknown
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Expected O, but got Unknown
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Expected O, but got Unknown
			try
			{
				string clan = "";
				Banner banner = null;
				PartyVM val = (PartyVM)Traverse.Create((object)__instance).Field("_partyVm").GetValue();
				if (val != null)
				{
					HeroViewModel selectedCharacter = val.SelectedCharacter;
					Hero val2 = (Hero)Traverse.Create((object)selectedCharacter).Field("_hero").GetValue();
					if (val2 != null)
					{
						clan = ((object)val2.Clan.Name).ToString();
						banner = val2.ClanBanner;
					}
				}
				string name = __instance.Name;
				CharacterObject character = __instance.Character;
				bool isHero = ((BasicCharacterObject)character).IsHero;
				bool isRanged = ((BasicCharacterObject)character).IsRanged;
				bool isMounted = ((BasicCharacterObject)character).IsMounted;
				int tier = character.Tier;
				string culture = ((object)((BasicCultureObject)character.Culture).Name).ToString();
				var (num, num2, num3, text, text2) = getColorFromConfig(clan, ref banner, name, isHero, isMounted, isRanged, tier, culture);
				if (text2 != null)
				{
					banner = new Banner(text2);
				}
				else if (text != null)
				{
					banner = new Banner(text);
				}
				ImageIdentifierVM code = __instance.Code;
				ImageIdentifier val3 = (ImageIdentifier)Traverse.Create((object)code).Field("_imageIdentifierCode").GetValue();
				CharacterCode val4 = CharacterCode.CreateFrom(val3.Id);
				if (num2.HasValue)
				{
					val4.Color1 = num2.Value;
				}
				if (num3.HasValue)
				{
					val4.Color2 = num3.Value;
				}
				string text3 = val4.CreateNewCodeString();
				ImageIdentifierVM code2 = new ImageIdentifierVM(text3, (ImageIdentifierType)5);
				__instance.Code = code2;
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class PocColorModPartyCharacterVMCharacter
	{
		public static void Postfix(PartyCharacterVM __instance)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Expected O, but got Unknown
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Expected O, but got Unknown
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Expected O, but got Unknown
			try
			{
				string clan = "";
				Banner banner = null;
				PartyVM val = (PartyVM)Traverse.Create((object)__instance).Field("_partyVm").GetValue();
				if (val != null)
				{
					HeroViewModel selectedCharacter = val.SelectedCharacter;
					Hero val2 = (Hero)Traverse.Create((object)selectedCharacter).Field("_hero").GetValue();
					if (val2 != null)
					{
						clan = ((object)val2.Clan.Name).ToString();
						banner = val2.ClanBanner;
					}
				}
				string name = __instance.Name;
				CharacterObject character = __instance.Character;
				bool isHero = ((BasicCharacterObject)character).IsHero;
				bool isRanged = ((BasicCharacterObject)character).IsRanged;
				bool isMounted = ((BasicCharacterObject)character).IsMounted;
				int tier = character.Tier;
				string culture = ((object)((BasicCultureObject)character.Culture).Name).ToString();
				var (num, num2, num3, text, text2) = getColorFromConfig(clan, ref banner, name, isHero, isMounted, isRanged, tier, culture);
				if (text2 != null)
				{
					banner = new Banner(text2);
				}
				else if (text != null)
				{
					banner = new Banner(text);
				}
				ImageIdentifierVM code = __instance.Code;
				ImageIdentifier val3 = (ImageIdentifier)Traverse.Create((object)code).Field("_imageIdentifierCode").GetValue();
				CharacterCode val4 = CharacterCode.CreateFrom(val3.Id);
				if (num2.HasValue)
				{
					val4.Color1 = num2.Value;
				}
				if (num3.HasValue)
				{
					val4.Color2 = num3.Value;
				}
				string text3 = val4.CreateNewCodeString();
				ImageIdentifierVM code2 = new ImageIdentifierVM(text3, (ImageIdentifierType)5);
				__instance.Code = code2;
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(GameMenuPartyItemVM), "RefreshValues")]
	internal class PocColorModGameMenuPartyItemVM
	{
		public static bool Prefix(GameMenuPartyItemVM __instance)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Expected O, but got Unknown
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Expected O, but got Unknown
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Expected O, but got Unknown
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Expected O, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Expected O, but got Unknown
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Expected O, but got Unknown
			try
			{
				string clan = "";
				Banner banner = null;
				PartyBase val = (PartyBase)Traverse.Create((object)__instance).Field("Party").GetValue();
				if (val != null)
				{
					Hero owner = val.Owner;
					if (owner != null)
					{
						Clan clan2 = owner.Clan;
						clan = ((clan2 != null) ? ((object)clan2.Name).ToString() : null) ?? "";
						banner = owner.ClanBanner;
					}
				}
				CharacterObject val2 = (CharacterObject)Traverse.Create((object)__instance).Field("Character").GetValue();
				string text = "";
				bool flag = false;
				bool flag2 = false;
				int num = 0;
				string text2 = "";
				if (val2 == null)
				{
					val2 = PartyBaseHelper.GetVisualPartyLeader(val);
				}
				text = ((val2 != null) ? ((object)((BasicCharacterObject)val2).Name).ToString() : null) ?? "";
				bool isHero = ((BasicCharacterObject)val2).IsHero;
				flag = ((BasicCharacterObject)val2).IsRanged;
				flag2 = ((BasicCharacterObject)val2).IsMounted;
				num = val2.Tier;
				text2 = ((object)((BasicCultureObject)val2.Culture).Name).ToString();
				var (num2, num3, num4, text3, text4) = getColorFromConfig(clan, ref banner, text, isHero, flag2, flag, num, text2);
				if (text4 != null)
				{
					banner = new Banner(text4);
				}
				else if (text3 != null)
				{
					banner = new Banner(text3);
				}
				if (banner != null)
				{
					__instance.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(banner), true);
				}
				ImageIdentifierVM val3 = (ImageIdentifierVM)Traverse.Create((object)__instance).Field("_visual").GetValue();
				CharacterCode val4 = CharacterCode.CreateFrom(val3.Id);
				if (num3.HasValue)
				{
					val4.Color1 = num3.Value;
				}
				if (num4.HasValue)
				{
					val4.Color2 = num4.Value;
				}
				string text5 = val4.CreateNewCodeString();
				ImageIdentifierVM value = new ImageIdentifierVM(text5, (ImageIdentifierType)5);
				Traverse.Create((object)__instance).Field("_visual").SetValue((object)value);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(OrderOfBattleHeroItemVM), "RefreshValues")]
	internal class PocColorModOrderOfBattleHeroItemVM
	{
		public static void Postfix(OrderOfBattleHeroItemVM __instance)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Expected O, but got Unknown
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Expected O, but got Unknown
			try
			{
				Agent val = (Agent)Traverse.Create((object)__instance).Field("Agent").GetValue();
				string charName = null;
				if (val != null)
				{
					charName = ((val != null) ? val.Name : null) ?? charName;
				}
				string clan = "";
				Banner banner = null;
				CharacterObject val2 = null;
				if (charName == null)
				{
					return;
				}
				Hero val3 = Hero.FindFirst((Func<Hero, bool>)((Hero h) => ((object)h.Name).ToString().Equals(charName)));
				if (val3 != null)
				{
					clan = ((object)val3.Clan.Name).ToString();
					banner = val3.ClanBanner;
					val2 = val3.CharacterObject;
				}
				bool flag = false;
				bool flag2 = false;
				int num = 0;
				string text = "";
				charName = ((val2 != null) ? ((object)((BasicCharacterObject)val2).Name).ToString() : null) ?? "";
				flag = val2 != null && ((BasicCharacterObject)val2).IsRanged;
				flag2 = val2 != null && ((BasicCharacterObject)val2).IsMounted;
				num = ((val2 != null) ? val2.Tier : 0);
				text = ((val2 != null) ? ((object)((BasicCultureObject)val2.Culture).Name).ToString() : null) ?? "";
				(int, uint?, uint?, string, string) colorFromConfig = getColorFromConfig(clan, ref banner, charName, isHero: true, flag2, flag, num, text);
				int item = colorFromConfig.Item1;
				uint? item2 = colorFromConfig.Item2;
				uint? item3 = colorFromConfig.Item3;
				string item4 = colorFromConfig.Item4;
				string item5 = colorFromConfig.Item5;
				ImageIdentifierVM val4 = (ImageIdentifierVM)Traverse.Create((object)__instance).Field("_imageIdentifier").GetValue();
				if (val4 != null)
				{
					CharacterCode val5 = CharacterCode.CreateFrom(val4.Id);
					if (item2.HasValue)
					{
						val5.Color1 = item2.Value;
					}
					if (item3.HasValue)
					{
						val5.Color2 = item3.Value;
					}
					string text2 = val5.CreateNewCodeString();
					ImageIdentifierVM imageIdentifier = new ImageIdentifierVM(text2, (ImageIdentifierType)5);
					__instance.ImageIdentifier = imageIdentifier;
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(OrderOfBattleHeroItemVM), "RefreshInformation")]
	internal class PocColorModOrderOfBattleHeroItemVMRefreshInformation
	{
		public static void Postfix(OrderOfBattleHeroItemVM __instance)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Expected O, but got Unknown
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Expected O, but got Unknown
			try
			{
				Agent val = (Agent)Traverse.Create((object)__instance).Field("Agent").GetValue();
				string charName = null;
				if (val != null)
				{
					charName = ((val != null) ? val.Name : null) ?? charName;
				}
				string clan = "";
				Banner banner = null;
				CharacterObject val2 = null;
				if (charName == null)
				{
					return;
				}
				Hero val3 = Hero.FindFirst((Func<Hero, bool>)((Hero h) => ((object)h.Name).ToString().Equals(charName)));
				if (val3 != null)
				{
					clan = ((object)val3.Clan.Name).ToString();
					banner = val3.ClanBanner;
					val2 = val3.CharacterObject;
				}
				bool flag = false;
				bool flag2 = false;
				int num = 0;
				string text = "";
				charName = ((val2 != null) ? ((object)((BasicCharacterObject)val2).Name).ToString() : null) ?? "";
				flag = val2 != null && ((BasicCharacterObject)val2).IsRanged;
				flag2 = val2 != null && ((BasicCharacterObject)val2).IsMounted;
				num = ((val2 != null) ? val2.Tier : 0);
				text = ((val2 != null) ? ((object)((BasicCultureObject)val2.Culture).Name).ToString() : null) ?? "";
				(int, uint?, uint?, string, string) colorFromConfig = getColorFromConfig(clan, ref banner, charName, isHero: true, flag2, flag, num, text);
				int item = colorFromConfig.Item1;
				uint? item2 = colorFromConfig.Item2;
				uint? item3 = colorFromConfig.Item3;
				string item4 = colorFromConfig.Item4;
				string item5 = colorFromConfig.Item5;
				ImageIdentifierVM val4 = (ImageIdentifierVM)Traverse.Create((object)__instance).Field("_imageIdentifier").GetValue();
				if (val4 != null)
				{
					CharacterCode val5 = CharacterCode.CreateFrom(val4.Id);
					if (item2.HasValue)
					{
						val5.Color1 = item2.Value;
					}
					if (item3.HasValue)
					{
						val5.Color2 = item3.Value;
					}
					string text2 = val5.CreateNewCodeString();
					ImageIdentifierVM imageIdentifier = new ImageIdentifierVM(text2, (ImageIdentifierType)5);
					__instance.ImageIdentifier = imageIdentifier;
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(CampaignSceneNotificationHelper), "CreateNotificationCharacterFromHero")]
	internal class PocColorModDefaultCreateNotificationCharacterFromHero
	{
		public static bool Prefix(ref SceneNotificationCharacter __result, Hero hero, Equipment overridenEquipment = null, bool useCivilian = false, BodyProperties overriddenBodyProperties = default(BodyProperties), bool useHorse = false, uint overriddenColor1 = uint.MaxValue, uint overriddenColor2 = uint.MaxValue)
		{
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Expected O, but got Unknown
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				object obj;
				if (hero == null)
				{
					obj = null;
				}
				else
				{
					Clan clan = hero.Clan;
					obj = ((clan != null) ? ((object)clan.Name).ToString() : null);
				}
				if (obj == null)
				{
					obj = "";
				}
				string clan2 = (string)obj;
				Banner banner = hero.ClanBanner;
				string unitName = ((object)hero.Name).ToString();
				bool isRanged = ((BasicCharacterObject)hero.CharacterObject).IsRanged;
				bool isMounted = ((BasicCharacterObject)hero.CharacterObject).IsMounted;
				int tier = hero.CharacterObject.Tier;
				string culture = ((object)((BasicCultureObject)hero.Culture).Name).ToString();
				var (num, num2, num3, text, text2) = getColorFromConfig(clan2, ref banner, unitName, isHero: true, isMounted, isRanged, tier, culture);
				if (text2 != null)
				{
					banner = new Banner(text2);
				}
				else if (text != null)
				{
					banner = new Banner(text);
				}
				IFaction mapFaction = hero.MapFaction;
				if (overriddenColor1 == uint.MaxValue)
				{
					overriddenColor1 = ((mapFaction != null) ? mapFaction.Color : ((BasicCultureObject)hero.CharacterObject.Culture).Color);
				}
				if (overriddenColor2 == uint.MaxValue)
				{
					overriddenColor2 = ((mapFaction != null) ? mapFaction.Color2 : ((BasicCultureObject)hero.CharacterObject.Culture).Color2);
				}
				if (num2.HasValue)
				{
					overriddenColor1 = num2.Value;
				}
				if (num3.HasValue)
				{
					overriddenColor2 = num3.Value;
				}
				if (overridenEquipment == null)
				{
					overridenEquipment = (useCivilian ? hero.CivilianEquipment : hero.BattleEquipment);
				}
				__result = new SceneNotificationCharacter((BasicCharacterObject)(object)hero.CharacterObject, overridenEquipment, overriddenBodyProperties, useCivilian, overriddenColor1, overriddenColor2, useHorse);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
				return true;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(MapConversationTableau), "SpawnOpponentLeader")]
	internal class PocColorModMapConversationTableauOppo
	{
		public static void Postfix(MapConversationTableau __instance)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Expected O, but got Unknown
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Expected O, but got Unknown
			try
			{
				List<AgentVisuals> list = (List<AgentVisuals>)Traverse.Create((object)__instance).Field("_agentVisuals").GetValue();
				MapConversationTableauData val = (MapConversationTableauData)Traverse.Create((object)__instance).Field("_data").GetValue();
				PartyBase party = val.ConversationPartnerData.Party;
				CharacterObject character = val.ConversationPartnerData.Character;
				MobileParty mobileParty = party.MobileParty;
				string clan = "";
				if (mobileParty != null)
				{
					Clan actualClan = party.MobileParty.ActualClan;
					clan = ((actualClan != null) ? ((object)actualClan.Name).ToString() : null);
				}
				AgentVisuals val2 = list[list.Count - 1];
				bool flag = false;
				bool flag2 = false;
				int num = 0;
				string text = "";
				flag = character != null && ((BasicCharacterObject)character).IsRanged;
				flag2 = character != null && ((BasicCharacterObject)character).IsMounted;
				num = ((character != null) ? character.Tier : 0);
				text = ((character != null) ? ((object)((BasicCultureObject)character.Culture).Name).ToString() : null) ?? "";
				Banner banner = party.Banner;
				var (num2, num3, num4, text2, text3) = getBattleColorFromConfig(clan, ref banner, ((object)((BasicCharacterObject)character).Name).ToString(), ((BasicCharacterObject)character).IsHero, ((BasicCharacterObject)character).IsMounted, flag, num, text);
				if (num3.HasValue && num4.HasValue)
				{
					val2.SetClothingColors(num3.Value, num4.Value);
				}
				list[list.Count - 1] = val2;
				Traverse.Create((object)__instance).Field("_agentVisuals").SetValue((object)list);
				AgentVisualsData copyAgentVisualsData = val2.GetCopyAgentVisualsData();
				if (text3 != null)
				{
					banner = new Banner(text3);
				}
				else if (text2 != null)
				{
					banner = new Banner(text2);
				}
				copyAgentVisualsData.Banner(banner);
				val2.Refresh(false, copyAgentVisualsData, false);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(MapConversationTableau), "SpawnOpponentBodyguardCharacter")]
	internal class PocColorModMapConversationTableauOppoBodyGuards
	{
		public static void Postfix(MapConversationTableau __instance, CharacterObject character)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Expected O, but got Unknown
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Expected O, but got Unknown
			try
			{
				MapConversationTableauData val = (MapConversationTableauData)Traverse.Create((object)__instance).Field("_data").GetValue();
				PartyBase party = val.ConversationPartnerData.Party;
				MobileParty mobileParty = party.MobileParty;
				string clan = "";
				if (mobileParty != null)
				{
					Clan actualClan = party.MobileParty.ActualClan;
					clan = ((actualClan != null) ? ((object)actualClan.Name).ToString() : null);
				}
				List<AgentVisuals> list = (List<AgentVisuals>)Traverse.Create((object)__instance).Field("_agentVisuals").GetValue();
				AgentVisuals val2 = list[list.Count - 1];
				bool flag = false;
				bool flag2 = false;
				int num = 0;
				string text = "";
				flag = character != null && ((BasicCharacterObject)character).IsRanged;
				flag2 = character != null && ((BasicCharacterObject)character).IsMounted;
				num = ((character != null) ? character.Tier : 0);
				text = ((character != null) ? ((object)((BasicCultureObject)character.Culture).Name).ToString() : null) ?? "";
				Banner banner = party.Banner;
				var (num2, num3, num4, text2, text3) = getBattleColorFromConfig(clan, ref banner, ((object)((BasicCharacterObject)character).Name).ToString(), ((BasicCharacterObject)character).IsHero, ((BasicCharacterObject)character).IsMounted, flag, num, text);
				if (num3.HasValue && num4.HasValue)
				{
					val2.SetClothingColors(num3.Value, num4.Value);
				}
				list[list.Count - 1] = val2;
				Traverse.Create((object)__instance).Field("_agentVisuals").SetValue((object)list);
				AgentVisualsData copyAgentVisualsData = val2.GetCopyAgentVisualsData();
				if (text3 != null)
				{
					banner = new Banner(text3);
				}
				else if (text2 != null)
				{
					banner = new Banner(text2);
				}
				copyAgentVisualsData.Banner(banner);
				val2.Refresh(false, copyAgentVisualsData, false);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(TroopSelectionItemVM))]
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	[HarmonyPatch(new Type[]
	{
		typeof(TroopRosterElement),
		typeof(Action<TroopSelectionItemVM>),
		typeof(Action<TroopSelectionItemVM>)
	})]
	internal class PocColorModTroopSelectionItemVM
	{
		public static void Postfix(TroopSelectionItemVM __instance)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Expected O, but got Unknown
			try
			{
				TroopRosterElement troop = __instance.Troop;
				CharacterObject character = troop.Character;
				string clan = "";
				string text = __instance.Name;
				Banner banner = null;
				if (character != null)
				{
					if (character.HeroObject != null)
					{
						text = ((object)character.HeroObject.Name).ToString();
						clan = ((object)character.HeroObject.Clan.Name).ToString();
						banner = character.HeroObject.ClanBanner;
					}
					else
					{
						text = ((object)((BasicCharacterObject)character).Name).ToString();
						clan = ((object)Clan.PlayerClan.Name).ToString();
					}
				}
				if (text == null)
				{
					return;
				}
				text = ((character != null) ? ((object)((BasicCharacterObject)character).Name).ToString() : null) ?? "";
				bool isRanged = character != null && ((BasicCharacterObject)character).IsRanged;
				bool isMounted = character != null && ((BasicCharacterObject)character).IsMounted;
				int tier = ((character != null) ? character.Tier : 0);
				string culture = ((character != null) ? ((object)((BasicCultureObject)character.Culture).Name).ToString() : null) ?? "";
				(int, uint?, uint?, string, string) colorFromConfig = getColorFromConfig(clan, ref banner, text, isHero: true, isMounted, isRanged, tier, culture);
				int item = colorFromConfig.Item1;
				uint? item2 = colorFromConfig.Item2;
				uint? item3 = colorFromConfig.Item3;
				string item4 = colorFromConfig.Item4;
				string item5 = colorFromConfig.Item5;
				ImageIdentifierVM visual = __instance.Visual;
				if (visual != null)
				{
					CharacterCode val = CharacterCode.CreateFrom(visual.Id);
					if (item2.HasValue)
					{
						val.Color1 = item2.Value;
					}
					if (item3.HasValue)
					{
						val.Color2 = item3.Value;
					}
					string text2 = val.CreateNewCodeString();
					ImageIdentifierVM visual2 = new ImageIdentifierVM(text2, (ImageIdentifierType)5);
					__instance.Visual = visual2;
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class PocColorModVMColor1
	{
		public static bool Prefix(CharacterViewModel __instance, ref uint value)
		{
			try
			{
				CharacterObject val = null;
				if (__instance.CharStringId != null)
				{
					val = CharacterObject.Find(__instance.CharStringId);
				}
				string unitName = ((val != null) ? ((object)((BasicCharacterObject)val).Name).ToString() : null) ?? "";
				bool isHero = val != null && ((BasicCharacterObject)val).IsHero;
				bool isMounted = val != null && ((BasicCharacterObject)val).IsMounted;
				bool isRanged = val != null && ((BasicCharacterObject)val).IsRanged;
				int tier = ((val != null) ? val.Tier : 0);
				string culture = ((val != null) ? ((object)((BasicCultureObject)val.Culture).Name).ToString() : null) ?? "";
				object obj;
				if (val == null)
				{
					obj = null;
				}
				else
				{
					Hero heroObject = val.HeroObject;
					if (heroObject == null)
					{
						obj = null;
					}
					else
					{
						Clan clan = heroObject.Clan;
						obj = ((clan != null) ? ((object)clan.Name).ToString() : null);
					}
				}
				string clan2 = (string)obj;
				if (__instance.BannerCodeText != null)
				{
					string bannerStr = __instance.BannerCodeText;
					var (num, num2, num3, text, text2) = getColorFromConfig(clan2, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
					if (num2.HasValue && num3.HasValue)
					{
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class PocColorModVMColor2
	{
		public static bool Prefix(CharacterViewModel __instance, ref uint value)
		{
			try
			{
				CharacterObject val = null;
				if (__instance.CharStringId != null)
				{
					val = CharacterObject.Find(__instance.CharStringId);
				}
				string unitName = ((val != null) ? ((object)((BasicCharacterObject)val).Name).ToString() : null) ?? "";
				bool isHero = val != null && ((BasicCharacterObject)val).IsHero;
				bool isMounted = val != null && ((BasicCharacterObject)val).IsMounted;
				bool isRanged = val != null && ((BasicCharacterObject)val).IsRanged;
				int tier = ((val != null) ? val.Tier : 0);
				string culture = ((val != null) ? ((object)((BasicCultureObject)val.Culture).Name).ToString() : null) ?? "";
				object obj;
				if (val == null)
				{
					obj = null;
				}
				else
				{
					Hero heroObject = val.HeroObject;
					if (heroObject == null)
					{
						obj = null;
					}
					else
					{
						Clan clan = heroObject.Clan;
						obj = ((clan != null) ? ((object)clan.Name).ToString() : null);
					}
				}
				string clan2 = (string)obj;
				if (__instance.BannerCodeText != null)
				{
					string bannerStr = __instance.BannerCodeText;
					var (num, num2, num3, text, text2) = getColorFromConfig(clan2, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
					if (num2.HasValue && num3.HasValue)
					{
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PartyVM), "RefreshCurrentCharacterInformation")]
	internal class PocColorModPartyVM
	{
		public static void Postfix(ref PartyVM __instance)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			try
			{
				Hero val = (Hero)Traverse.Create((object)__instance.CurrentCharacter).Field("_hero").GetValue();
				string clan = ((object)MobileParty.MainParty.ActualClan.Name).ToString();
				CharacterObject val2;
				if (val == null)
				{
					val2 = CharacterObject.Find(((CharacterViewModel)__instance.SelectedCharacter).CharStringId);
				}
				else
				{
					val2 = val.CharacterObject;
					clan = ((object)val.Clan.Name).ToString();
				}
				string text = ((object)((BasicCharacterObject)val2).Name).ToString();
				bool isHero = ((BasicCharacterObject)val2).IsHero;
				HeroViewModel selectedCharacter = __instance.SelectedCharacter;
				string bannerStr = ((selectedCharacter != null) ? ((CharacterViewModel)selectedCharacter).BannerCodeText : null);
				if (bannerStr != null)
				{
					Banner banner = new Banner(bannerStr);
					var (mode, num, num2, newbanner, newshield) = getColorFromConfig(clan, ref banner, ref bannerStr, text, isHero, ((BasicCharacterObject)val2).IsMounted, ((BasicCharacterObject)val2).IsRanged, val2.Tier, ((object)((BasicCultureObject)val2.Culture).Name).ToString());
					if (num.HasValue && num2.HasValue)
					{
						((CharacterViewModel)__instance.SelectedCharacter).BannerCodeText = bannerStr;
						Traverse.Create((object)__instance.SelectedCharacter).Field("_armorColor1").SetValue((object)num);
						Traverse.Create((object)__instance.SelectedCharacter).Field("_armorColor2").SetValue((object)num2);
						((ViewModel)__instance.SelectedCharacter).OnPropertyChanged("ArmorColor1");
						((ViewModel)__instance.SelectedCharacter).OnPropertyChanged("ArmorColor2");
						colorDataCache[text] = new ColorData(mode, num, num2, newbanner, newshield);
					}
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(HeroViewModel), "FillFrom")]
	internal class PocColorModHeroFillFrom
	{
		public static void Postfix(ref HeroViewModel __instance, ref Hero hero)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			try
			{
				CharacterObject characterObject = hero.CharacterObject;
				string unitName = ((object)((BasicCharacterObject)characterObject).Name).ToString();
				string text = ((object)hero.Clan.Name).ToString();
				HeroViewModel obj = __instance;
				string bannerStr = ((obj != null) ? ((CharacterViewModel)obj).BannerCodeText : null);
				if (bannerStr != null || text != null)
				{
					Banner banner = new Banner(bannerStr);
					var (num, num2, num3, text2, text3) = getColorFromConfig(text, ref banner, ref bannerStr, unitName, isHero: true, ((BasicCharacterObject)characterObject).IsMounted, ((BasicCharacterObject)characterObject).IsRanged, characterObject.Tier, ((object)((BasicCultureObject)characterObject.Culture).Name).ToString());
					if (num2.HasValue && num3.HasValue)
					{
						((CharacterViewModel)__instance).BannerCodeText = bannerStr;
						Traverse.Create((object)__instance).Field("_armorColor1").SetValue((object)num2);
						Traverse.Create((object)__instance).Field("_armorColor2").SetValue((object)num3);
						((ViewModel)__instance).OnPropertyChanged("ArmorColor1");
						((ViewModel)__instance).OnPropertyChanged("ArmorColor2");
					}
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(ClanLordItemVM), "UpdateProperties")]
	internal class PocColorModClanLordItemVM
	{
		public static void Postfix(ref ClanLordItemVM __instance)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			try
			{
				ClanLordItemVM obj = __instance;
				object obj2;
				if (obj == null)
				{
					obj2 = null;
				}
				else
				{
					HeroViewModel heroModel = obj.HeroModel;
					obj2 = ((heroModel != null) ? ((CharacterViewModel)heroModel).BannerCodeText : null);
				}
				string text = (string)obj2;
				if (text != null)
				{
					__instance.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(new Banner(text)), true);
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(ClanLordItemVM), "RefreshValues")]
	internal class PocColorModClanLordItemVMRefreshValues
	{
		public static bool Prefix(ref ClanLordItemVM __instance)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Expected O, but got Unknown
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Expected O, but got Unknown
			try
			{
				ImageIdentifierVM val = (ImageIdentifierVM)Traverse.Create((object)__instance).Field("_visual").GetValue();
				ImageIdentifier val2 = (ImageIdentifier)Traverse.Create((object)val).Field("_imageIdentifierCode").GetValue();
				CharacterCode val3 = CharacterCode.CreateFrom(val2.Id);
				Hero hero = __instance.GetHero();
				Banner banner = hero.ClanBanner;
				string unitName = ((object)hero.Name).ToString();
				string clan = ((object)hero.Clan.Name).ToString();
				bool isRanged = ((BasicCharacterObject)hero.CharacterObject).IsRanged;
				bool isMounted = ((BasicCharacterObject)hero.CharacterObject).IsMounted;
				int tier = hero.CharacterObject.Tier;
				var (num, num2, num3, text, text2) = getColorFromConfig(clan, ref banner, unitName, isHero: true, isMounted, isRanged, tier, ((object)((BasicCultureObject)hero.Culture).Name).ToString());
				if (text2 != null)
				{
					banner = new Banner(text2);
				}
				else if (text != null)
				{
					banner = new Banner(text);
				}
				if (num2.HasValue)
				{
					val3.Color1 = num2.Value;
				}
				if (num3.HasValue)
				{
					val3.Color2 = num3.Value;
				}
				__instance.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(banner), true);
				string text3 = val3.CreateNewCodeString();
				ImageIdentifierVM value = new ImageIdentifierVM(text3, (ImageIdentifierType)5);
				Traverse.Create((object)__instance).Field("_visual").SetValue((object)value);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(HeroVM), "RefreshValues")]
	internal class PocColorModHeroVM
	{
		public static bool Prefix(ref HeroVM __instance)
		{
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Expected O, but got Unknown
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Expected O, but got Unknown
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Expected O, but got Unknown
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Expected O, but got Unknown
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Expected O, but got Unknown
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Expected O, but got Unknown
			try
			{
				Hero hero = __instance.Hero;
				if (!__instance.Hero.IsKnownToPlayer)
				{
					return true;
				}
				Banner banner = ((hero != null) ? hero.ClanBanner : null);
				string unitName = ((hero != null) ? ((object)hero.Name).ToString() : null) ?? "";
				Clan val = ((hero != null) ? hero.Clan : null);
				string clan = null;
				if (val != null)
				{
					clan = ((hero != null) ? ((object)hero.Clan.Name).ToString() : null) ?? null;
				}
				CharacterObject val2 = ((hero != null) ? hero.CharacterObject : null);
				if (banner != null || val != null)
				{
					var (num, num2, num3, text, text2) = getColorFromConfig(clan, ref banner, unitName, isHero: true, ((BasicCharacterObject)val2).IsMounted, ((BasicCharacterObject)val2).IsRanged, val2.Tier, ((object)((BasicCultureObject)val2.Culture).Name).ToString());
					if (text2 != null)
					{
						banner = new Banner(text2);
					}
					else if (text != null)
					{
						banner = new Banner(text);
					}
					__instance.ClanBanner = new ImageIdentifierVM(banner);
					__instance.ClanBanner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(new Banner(banner)), true);
					CharacterCode characterCode = CampaignUIHelper.GetCharacterCode(hero.CharacterObject, false);
					if (num2.HasValue)
					{
						characterCode.Color1 = num2.Value;
					}
					if (num3.HasValue)
					{
						characterCode.Color2 = num3.Value;
					}
					string text3 = characterCode.CreateNewCodeString();
					__instance.ImageIdentifier = new ImageIdentifierVM(text3, (ImageIdentifierType)5);
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(SPInventoryVM), "UpdateCurrentCharacterIfPossible")]
	internal class PocColorModSPInventoryVM
	{
		public static void Postfix(ref SPInventoryVM __instance)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			try
			{
				CharInfo charInfo = null;
				CharacterObject val = (CharacterObject)Traverse.Create((object)__instance).Field("_currentCharacter").GetValue();
				string unitName = ((val == null) ? null : ((object)((BasicCharacterObject)val).Name)?.ToString());
				bool isHero = val != null && ((BasicCharacterObject)val).IsHero;
				bool isMounted = false;
				bool isRanged = false;
				int tier = 0;
				string culture = "";
				string charStringId = ((CharacterViewModel)__instance.MainCharacter).CharStringId;
				if (charStringId != null)
				{
					CharacterObject val2 = CharacterObject.Find(charStringId);
					unitName = ((object)((BasicCharacterObject)val2).Name).ToString();
					isHero = ((BasicCharacterObject)val2).IsHero;
					isMounted = ((BasicCharacterObject)val2).IsMounted;
					isRanged = ((BasicCharacterObject)val2).IsRanged;
					tier = val2.Tier;
					culture = ((object)((BasicCultureObject)val2.Culture).Name).ToString();
				}
				HeroViewModel mainCharacter = __instance.MainCharacter;
				string bannerStr = ((mainCharacter != null) ? ((CharacterViewModel)mainCharacter).BannerCodeText : null);
				object obj;
				if (val == null)
				{
					obj = null;
				}
				else
				{
					Hero heroObject = val.HeroObject;
					if (heroObject == null)
					{
						obj = null;
					}
					else
					{
						Clan clan = heroObject.Clan;
						obj = ((clan != null) ? ((object)clan.Name).ToString() : null);
					}
				}
				string clan2 = (string)obj;
				if (bannerStr != null)
				{
					Banner val3 = new Banner(bannerStr);
					var (num, num2, num3, text, text2) = getColorFromConfig(clan2, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
					if (num2.HasValue && num3.HasValue)
					{
						((CharacterViewModel)__instance.MainCharacter).BannerCodeText = bannerStr;
						Traverse.Create((object)__instance.MainCharacter).Field("_armorColor1").SetValue((object)num2);
						Traverse.Create((object)__instance.MainCharacter).Field("_armorColor2").SetValue((object)num3);
						((ViewModel)__instance.MainCharacter).OnPropertyChanged("ArmorColor1");
						((ViewModel)__instance.MainCharacter).OnPropertyChanged("ArmorColor2");
					}
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(Agent), "EquipItemsFromSpawnEquipment")]
	internal class PocColorModAgentEquipItemsFromSpawnEquipment
	{
		public static bool Prefix(ref Agent __instance)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Expected O, but got Unknown
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Expected O, but got Unknown
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Invalid comparison between Unknown and I4
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Agent obj = __instance;
				if (((obj != null) ? obj.Equipment : null) == null)
				{
					return true;
				}
				Agent obj2 = __instance;
				MissionWeapon? val = ((obj2 != null) ? new MissionWeapon?(obj2.Equipment[(EquipmentIndex)4]) : null);
				int num;
				MissionWeapon val2;
				if (!val.HasValue)
				{
					num = 0;
				}
				else
				{
					val2 = val.GetValueOrDefault();
					num = (((MissionWeapon)(ref val2)).IsBanner() ? 1 : 0);
				}
				bool flag = (byte)num != 0;
				int num2;
				if (!val.HasValue)
				{
					num2 = 0;
				}
				else
				{
					val2 = val.GetValueOrDefault();
					num2 = (((MissionWeapon)(ref val2)).IsShield() ? 1 : 0);
				}
				bool flag2 = (byte)num2 != 0;
				string clan;
				if (val.HasValue)
				{
					object value;
					if (!val.HasValue)
					{
						value = null;
					}
					else
					{
						val2 = val.GetValueOrDefault();
						value = ((object)((MissionWeapon)(ref val2)).Item)?.ToString();
					}
					if ("mod_banner_1".Equals((string)value) || flag)
					{
						MissionWeapon value2 = val.Value;
						Banner banner = ((MissionWeapon)(ref value2)).Banner;
						string bannerStr = ((banner != null) ? PocColorMod.SerializeBanner(banner) : null);
						if (bannerStr != null)
						{
							clan = PocColorMod.bannerClanCache[bannerStr] ?? "";
							string text = PocColorMod.bannerKingdomCache[bannerStr] ?? "";
							Clan val3 = Clan.FindFirst((Predicate<Clan>)isClan);
							Banner val4 = banner;
							Banner val5 = banner;
							if (val3 != null)
							{
								val4 = val3.Banner;
							}
							string charName = __instance.Name;
							CharacterObject val6 = CharacterObject.FindFirst((Predicate<CharacterObject>)((CharacterObject c) => ((object)((BasicCharacterObject)c).Name).ToString().Equals(charName)));
							uint? num4;
							uint? num5;
							string text2;
							string text3;
							if (savedColorData == null)
							{
								int num3;
								(num3, num4, num5, text2, text3) = getBattleColorFromConfig(null, ref bannerStr, __instance.Name, __instance.IsHero, ((BasicCharacterObject)val6).IsMounted, ((BasicCharacterObject)val6).IsRanged, val6.Tier, ((object)((BasicCultureObject)val6.Culture).Name).ToString());
							}
							else
							{
								int mode = savedColorData.mode;
								uint? c2 = savedColorData.c1;
								uint? c3 = savedColorData.c2;
								string newbanner = savedColorData.newbanner;
								text3 = savedColorData.newshield;
								text2 = newbanner;
								num5 = c3;
								num4 = c2;
								int num3 = mode;
							}
							if (text2 != null)
							{
								val4 = new Banner(text2);
							}
							if (text3 != null)
							{
								val5 = new Banner(text3);
							}
							for (EquipmentIndex val7 = (EquipmentIndex)0; (int)val7 < 4; val7 = (EquipmentIndex)(val7 + 1))
							{
								MissionEquipment equipment = __instance.Equipment;
								EquipmentIndex val8 = val7;
								val2 = __instance.Equipment[val7];
								ItemObject item = ((MissionWeapon)(ref val2)).Item;
								val2 = __instance.Equipment[val7];
								equipment[val8] = new MissionWeapon(item, ((MissionWeapon)(ref val2)).ItemModifier, val5);
							}
							FieldInfo field = typeof(MissionWeapon).GetField("<Banner>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
							TypedReference obj3 = __makeref(value2);
							field.SetValueDirect(obj3, val4);
							string text4 = ((object)((MissionWeapon)(ref value2)).Item).ToString();
							__instance.Equipment[(EquipmentIndex)4] = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>(text4), (ItemModifier)null, val4);
							if (num4.HasValue && num5.HasValue)
							{
								__instance.SetClothingColor1(num4.Value);
								__instance.SetClothingColor2(num5.Value);
							}
						}
					}
				}
				bool isClan(Clan c)
				{
					return ((object)c.Name).ToString() == clan;
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Agent), "EquipWeaponWithNewEntity")]
	internal class PocColorModAgentEquipNewEntity
	{
		public static bool Prefix(ref Agent __instance, ref MissionWeapon weapon)
		{
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Expected O, but got Unknown
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Expected O, but got Unknown
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Invalid comparison between Unknown and I4
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Invalid comparison between Unknown and I4
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_0324: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_038d: Unknown result type (might be due to invalid IL or missing references)
			//IL_038f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				string text = ((object)((MissionWeapon)(ref weapon)).GetModifiedItemName())?.ToString() ?? "";
				Banner banner = ((MissionWeapon)(ref weapon)).Banner;
				string bannerStr = ((banner != null) ? PocColorMod.SerializeBanner(banner) : null);
				string clan;
				if (bannerStr != null)
				{
					clan = PocColorMod.bannerClanCache[bannerStr] ?? "";
					string text2 = PocColorMod.bannerKingdomCache[bannerStr] ?? "";
					Clan val = Clan.FindFirst((Predicate<Clan>)isClan);
					Banner val2 = banner;
					Banner val3 = banner;
					if (val != null)
					{
						val2 = val.Banner;
					}
					string charName = __instance.Name;
					CharacterObject val4 = CharacterObject.FindFirst((Predicate<CharacterObject>)((CharacterObject c) => ((object)((BasicCharacterObject)c).Name).ToString().Equals(charName)));
					uint? num2;
					uint? num3;
					string text3;
					string text4;
					if (savedColorData == null)
					{
						int num;
						(num, num2, num3, text3, text4) = getBattleColorFromConfig(clan, ref bannerStr, __instance.Name, __instance.IsHero, ((BasicCharacterObject)val4).IsMounted, ((BasicCharacterObject)val4).IsRanged, val4.Tier, ((object)((BasicCultureObject)val4.Culture).Name).ToString());
					}
					else
					{
						int mode = savedColorData.mode;
						uint? c2 = savedColorData.c1;
						uint? c3 = savedColorData.c2;
						string newbanner = savedColorData.newbanner;
						text4 = savedColorData.newshield;
						text3 = newbanner;
						num3 = c3;
						num2 = c2;
						int num = mode;
					}
					if (text3 != null)
					{
						val2 = new Banner(text3);
					}
					if (text4 != null)
					{
						val3 = new Banner(text4);
					}
					MissionWeapon val7;
					if (text.Equals("Campaign Banner Small") || text.Equals("Mod Banner 1") || text.Equals("Mod Banner 2") || ((MissionWeapon)(ref weapon)).IsBanner())
					{
						for (EquipmentIndex val5 = (EquipmentIndex)0; (int)val5 < 5; val5 = (EquipmentIndex)(val5 + 1))
						{
							MissionEquipment equipment = __instance.Equipment;
							EquipmentIndex val6 = val5;
							val7 = __instance.Equipment[val5];
							ItemObject item = ((MissionWeapon)(ref val7)).Item;
							val7 = __instance.Equipment[val5];
							equipment[val6] = new MissionWeapon(item, ((MissionWeapon)(ref val7)).ItemModifier, val3);
						}
						FieldInfo field = typeof(MissionWeapon).GetField("<Banner>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
						TypedReference obj = __makeref(weapon);
						field.SetValueDirect(obj, val2);
						string text5 = ((object)((MissionWeapon)(ref weapon)).Item).ToString();
						weapon = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>(text5), (ItemModifier)null, val2);
						if (num2.HasValue && num3.HasValue)
						{
							__instance.SetClothingColor1(num2.Value);
							__instance.SetClothingColor2(num3.Value);
						}
					}
					else if (((MissionWeapon)(ref weapon)).IsShield())
					{
						FieldInfo field2 = typeof(MissionWeapon).GetField("<Banner>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
						TypedReference obj2 = __makeref(weapon);
						field2.SetValueDirect(obj2, val3);
						for (EquipmentIndex val8 = (EquipmentIndex)0; (int)val8 < 5; val8 = (EquipmentIndex)(val8 + 1))
						{
							val7 = __instance.Equipment[val8];
							ItemObject item2 = ((MissionWeapon)(ref val7)).Item;
							string text6 = ((item2 == null) ? null : ((object)item2.Name)?.ToString());
							if (text6.Equals("Campaign Banner Small") || text6.Equals("Mod Banner 1") || text6.Equals("Mod Banner 2") || ((MissionWeapon)(ref weapon)).IsBanner())
							{
								val7 = __instance.Equipment[val8];
								string text7 = ((object)((MissionWeapon)(ref val7)).Item).ToString();
								__instance.Equipment[val8] = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>(text7), (ItemModifier)null, val2);
								break;
							}
						}
						if (num2.HasValue && num3.HasValue)
						{
							__instance.SetClothingColor1(num2.Value);
							__instance.SetClothingColor2(num3.Value);
						}
					}
				}
				savedColorData = null;
				bool isClan(Clan c)
				{
					return ((object)c.Name).ToString() == clan;
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(AgentVisuals), "AddSkinArmorWeaponMultiMeshesToEntity")]
	internal class PocColorModAgentVisualsAddMeshes
	{
		public static void Postfix(ref AgentVisuals __instance, uint teamColor1, uint teamColor2, bool needBatchedVersion)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Expected O, but got Unknown
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Expected O, but got Unknown
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Thread currentThread = Thread.CurrentThread;
				AgentVisualsData val = (AgentVisualsData)Traverse.Create((object)__instance).Field("_data").GetValue();
				Banner bannerData = val.BannerData;
				int num = 0;
				uint? num2 = null;
				uint? num3 = null;
				string text = null;
				string text2 = null;
				Banner val2 = bannerData;
				if (savedColorData != null)
				{
					num = savedColorData.mode;
					num2 = savedColorData.c1;
					num3 = savedColorData.c2;
					text = savedColorData.newbanner;
					text2 = savedColorData.newshield;
				}
				if (text != null)
				{
					val2 = new Banner(text);
				}
				if (val2 == null)
				{
					return;
				}
				BodyProperties bodyPropertiesData = val.BodyPropertiesData;
				int hashCode = ((object)(BodyProperties)(ref bodyPropertiesData)).GetHashCode();
				MissionWeapon val5 = default(MissionWeapon);
				for (int i = 0; i < 5; i++)
				{
					EquipmentElement val3 = val.EquipmentData[i];
					if (((EquipmentElement)(ref val3)).IsEmpty)
					{
						continue;
					}
					ItemObject item = ((EquipmentElement)(ref val3)).Item;
					string text3 = ((item == null) ? null : ((object)item.Name)?.ToString()) ?? "";
					val3 = val.EquipmentData[i];
					ItemModifier itemModifier = ((EquipmentElement)(ref val3)).ItemModifier;
					Banner val4 = null;
					string text4 = "";
					if (((EquipmentElement)(ref val3)).Item != null)
					{
						ItemTypeEnum itemType = ((EquipmentElement)(ref val3)).Item.ItemType;
						text4 = ((object)(ItemTypeEnum)(ref itemType)).ToString();
					}
					if (text3.Equals("Campaign Banner Small") || text3.Equals("Mod Banner 1") || text3.Equals("Mod Banner 2") || text4.Equals("Banner"))
					{
						val4 = val2;
					}
					else if (text4.Equals("Shield") && !string.IsNullOrEmpty(text2))
					{
						val4 = new Banner(text2);
					}
					if (val4 != null)
					{
						((MissionWeapon)(ref val5))._002Ector(item, itemModifier, val4);
						if (val.AddColorRandomnessData)
						{
							((MissionWeapon)(ref val5)).SetRandomGlossMultiplier(hashCode);
						}
						WeaponData weaponData = ((MissionWeapon)(ref val5)).GetWeaponData(needBatchedVersion);
						WeaponData ammoWeaponData = ((MissionWeapon)(ref val5)).GetAmmoWeaponData(needBatchedVersion);
						val.AgentVisuals.AddWeaponToAgentEntity(i, ref weaponData, ((MissionWeapon)(ref val5)).GetWeaponStatsData(), ref ammoWeaponData, ((MissionWeapon)(ref val5)).GetAmmoWeaponStatsData(), val.GetCachedWeaponEntity((EquipmentIndex)i));
						((WeaponData)(ref weaponData)).DeinitializeManagedPointers();
						((WeaponData)(ref ammoWeaponData)).DeinitializeManagedPointers();
					}
				}
				if (num2.HasValue && num3.HasValue)
				{
					val.ClothColor1(num2.Value);
					val.ClothColor2(num3.Value);
				}
				val.AgentVisuals.SetWieldedWeaponIndices(val.RightWieldedItemIndexData, val.LeftWieldedItemIndexData);
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(AgentVisuals), "Create")]
	internal class PocColorModAgentVisualsCreate
	{
		public static bool Prefix(ref AgentVisualsData data, ref string name)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			try
			{
				if (name.StartsWith("PartyIcon"))
				{
					string text = name.Substring(10);
					data.CharacterObjectStringId(text);
					if (data.BannerData == null && savedColorData != null && savedColorData.newshield != null)
					{
						data.Banner(new Banner(savedColorData.newshield));
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CharacterTableau), "RefreshCharacterTableau")]
	internal class PocColorModCharacterTableauRefresh
	{
		public static bool Prefix(ref CharacterTableau __instance)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Expected O, but got Unknown
			string text = (string)Traverse.Create((object)__instance).Field("_charStringId").GetValue();
			CharacterObject val = null;
			string text2 = "";
			bool isHero = false;
			bool isMounted = false;
			bool isRanged = false;
			int tier = 0;
			string culture = "";
			if (text != null)
			{
				val = CharacterObject.Find(text);
				text2 = ((object)((BasicCharacterObject)val).Name).ToString();
				isHero = ((BasicCharacterObject)val).IsHero;
				isMounted = ((BasicCharacterObject)val).IsMounted;
				isRanged = ((BasicCharacterObject)val).IsRanged;
				tier = val.Tier;
				culture = ((object)((BasicCultureObject)val.Culture).Name).ToString();
			}
			Banner banner = (Banner)Traverse.Create((object)__instance).Field("_banner").GetValue();
			object obj;
			if (val == null)
			{
				obj = null;
			}
			else
			{
				Hero heroObject = val.HeroObject;
				if (heroObject == null)
				{
					obj = null;
				}
				else
				{
					Clan clan = heroObject.Clan;
					obj = ((clan != null) ? ((object)clan.Name).ToString() : null);
				}
			}
			string clan2 = (string)obj;
			ColorData colorData = colorDataCache[text2];
			if (colorData != null)
			{
				savedColorData = colorData;
				colorDataCache[text2] = null;
			}
			else
			{
				(int, uint?, uint?, string, string) colorFromConfig = getColorFromConfig(clan2, ref banner, text2, isHero, isMounted, isRanged, tier, culture);
				int item = colorFromConfig.Item1;
				uint? item2 = colorFromConfig.Item2;
				uint? item3 = colorFromConfig.Item3;
				string item4 = colorFromConfig.Item4;
				string item5 = colorFromConfig.Item5;
				savedColorData = new ColorData(item, item2, item3, item4, item5);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CharacterTableau), "UpdateBannerItem")]
	internal class PocColorModCharacterTableau
	{
		public static bool Prefix(ref CharacterTableau __instance, ref string __state)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			try
			{
				string text = (string)Traverse.Create((object)__instance).Field("_charStringId").GetValue();
				CharacterObject val = null;
				if (text != null)
				{
					val = CharacterObject.Find(text);
				}
				Banner val2 = (Banner)Traverse.Create((object)__instance).Field("_banner").GetValue();
				if (savedColorData != null)
				{
					int mode = savedColorData.mode;
					uint? c = savedColorData.c1;
					uint? c2 = savedColorData.c2;
					string newbanner = savedColorData.newbanner;
					string newshield = savedColorData.newshield;
					string bannerCode = newbanner;
					uint? num = c2;
					uint? num2 = c;
					int num3 = mode;
					__state = newshield;
					__instance.SetBannerCode(bannerCode);
					if (num2.HasValue)
					{
						__instance.SetArmorColor1(num2.Value);
					}
					if (num.HasValue)
					{
						__instance.SetArmorColor2(num.Value);
					}
					savedColorData = null;
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}

		public static void Postfix(ref CharacterTableau __instance, ref string __state)
		{
			__instance.SetBannerCode(__state);
		}
	}

	[HarmonyPatch(typeof(CharacterSpawner), "SpawnMount")]
	internal class PocColorModMountSpawnMount
	{
		public static bool Prefix(ref CharacterSpawner __instance, CharacterCode characterCode, bool __state)
		{
			__state = false;
			if (savedColorData == null)
			{
				savedColorData = new ColorData(0, characterCode.Color1, characterCode.Color2, null, null);
				__state = true;
			}
			return true;
		}

		public static void Postfix(ref CharacterSpawner __instance, bool __state)
		{
			if (__state)
			{
				savedColorData = null;
			}
		}
	}

	[HarmonyPatch(typeof(MountVisualCreator), "AddMountMesh")]
	internal class PocColorModMountVisualCreatorAddMountMeshToEntity
	{
		public static bool Prefix(ref List<MetaMesh> __result, MBAgentVisuals agentVisual, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Invalid comparison between Unknown and I4
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Invalid comparison between Unknown and I4
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Invalid comparison between Unknown and I4
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				List<MetaMesh> list = new List<MetaMesh>();
				HorseComponent horseComponent = mountItem.HorseComponent;
				uint num = uint.MaxValue;
				MetaMesh multiMesh = ItemCollectionElementViewExtensions.GetMultiMesh(mountItem, false, false, true);
				if (string.IsNullOrEmpty(mountCreationKeyStr))
				{
					mountCreationKeyStr = MountCreationKey.GetRandomMountKeyString(mountItem, MBRandom.RandomInt());
				}
				MountCreationKey val = MountCreationKey.FromString(mountCreationKeyStr);
				if ((int)mountItem.ItemType == 1)
				{
					MountVisualCreator.SetHorseColors(multiMesh, val);
				}
				if (horseComponent.HorseMaterialNames != null && ((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames).Count > 0)
				{
					MountVisualCreator.SetMaterialProperties(mountItem, multiMesh, val, ref num);
				}
				int nondeterministicRandomInt = MBRandom.NondeterministicRandomInt;
				SetVoiceDefinition(agent, nondeterministicRandomInt);
				MetaMesh val2 = null;
				if (harnessItem != null)
				{
					val2 = ItemCollectionElementViewExtensions.GetMultiMesh(harnessItem, false, false, true);
					if (harnessItem.IsUsingTeamColor)
					{
						for (int i = 0; i < val2.MeshCount; i++)
						{
							Mesh meshAtIndex = val2.GetMeshAtIndex(i);
							if (!meshAtIndex.HasTag("no_team_color") && savedColorData != null)
							{
								int mode = savedColorData.mode;
								uint? c = savedColorData.c1;
								uint? c2 = savedColorData.c2;
								string newbanner = savedColorData.newbanner;
								string newshield = savedColorData.newshield;
								string text = newbanner;
								uint? num2 = c2;
								uint? num3 = c;
								int num4 = mode;
								if (num3.HasValue)
								{
									meshAtIndex.Color = num3.Value;
								}
								if (num2.HasValue)
								{
									meshAtIndex.Color2 = num2.Value;
								}
							}
						}
					}
				}
				foreach (KeyValuePair<string, bool> additionalMeshesName in horseComponent.AdditionalMeshesNameList)
				{
					if (additionalMeshesName.Key.Length <= 0)
					{
						continue;
					}
					string text2 = additionalMeshesName.Key;
					if (harnessItem == null || !additionalMeshesName.Value)
					{
						MetaMesh copy = MetaMesh.GetCopy(text2, true, false);
						if (num != uint.MaxValue)
						{
							copy.SetFactor1Linear(num);
						}
						list.Add(copy);
						continue;
					}
					ArmorComponent armorComponent = harnessItem.ArmorComponent;
					if (armorComponent != null && (int)armorComponent.ManeCoverType == 3)
					{
						continue;
					}
					ArmorComponent armorComponent2 = harnessItem.ArmorComponent;
					if (armorComponent2 != null && (int)armorComponent2.ManeCoverType > 0)
					{
						string text3 = text2;
						HorseHarnessCoverTypes? obj;
						if (harnessItem == null)
						{
							obj = null;
						}
						else
						{
							ArmorComponent armorComponent3 = harnessItem.ArmorComponent;
							obj = ((armorComponent3 != null) ? new HorseHarnessCoverTypes?(armorComponent3.ManeCoverType) : null);
						}
						HorseHarnessCoverTypes? val3 = obj;
						text2 = text3 + "_" + val3.ToString();
					}
					MetaMesh copy2 = MetaMesh.GetCopy(text2, true, false);
					if (num != uint.MaxValue)
					{
						copy2.SetFactor1Linear(num);
					}
					list.Add(copy2);
				}
				if ((NativeObject)(object)multiMesh != (NativeObject)null)
				{
					list.Add(multiMesh);
				}
				if ((NativeObject)(object)val2 != (NativeObject)null)
				{
					if ((NativeObject)(object)agentVisual != (NativeObject)null)
					{
						MetaMesh val4 = null;
						if (NativeConfig.CharacterDetail > 2 && harnessItem.ArmorComponent != null)
						{
							val4 = MetaMesh.GetCopy(harnessItem.ArmorComponent.ReinsRopeMesh, false, true);
						}
						ArmorComponent armorComponent4 = harnessItem.ArmorComponent;
						MetaMesh copy3 = MetaMesh.GetCopy((armorComponent4 != null) ? armorComponent4.ReinsMesh : null, false, true);
						if ((NativeObject)(object)val4 != (NativeObject)null && (NativeObject)(object)copy3 != (NativeObject)null)
						{
							agentVisual.AddHorseReinsClothMesh(copy3, val4);
							((NativeObject)val4).ManualInvalidate();
						}
						if ((NativeObject)(object)copy3 != (NativeObject)null)
						{
							list.Add(copy3);
						}
					}
					else if (harnessItem.ArmorComponent != null)
					{
						MetaMesh copy4 = MetaMesh.GetCopy(harnessItem.ArmorComponent.ReinsMesh, true, true);
						if ((NativeObject)(object)copy4 != (NativeObject)null)
						{
							list.Add(copy4);
						}
					}
					list.Add(val2);
				}
				__result = list;
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
				return true;
			}
			return false;
		}

		private static void SetVoiceDefinition(Agent agent, int seedForRandomVoiceTypeAndPitch)
		{
			try
			{
				MBAgentVisuals val = ((agent != null) ? agent.AgentVisuals : null);
				if ((NativeObject)(object)val != (NativeObject)null)
				{
					string soundAndCollisionInfoClassName = agent.GetSoundAndCollisionInfoClassName();
					int num = ((!string.IsNullOrEmpty(soundAndCollisionInfoClassName)) ? SkinVoiceManager.GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassName(soundAndCollisionInfoClassName) : 0);
					if (num == 0)
					{
						val.SetVoiceDefinitionIndex(-1, 0f);
						return;
					}
					int num2 = MathF.Abs(seedForRandomVoiceTypeAndPitch);
					float num3 = (float)num2 * 4.656613E-10f;
					int[] array = new int[num];
					SkinVoiceManager.GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassName(soundAndCollisionInfoClassName, array);
					int num4 = array[num2 % num];
					val.SetVoiceDefinitionIndex(num4, num3);
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(PartyVisual), "AddMobileIconComponents")]
	internal class PocColorModPartyVisualAddMobileIconComponents
	{
		public static bool Prefix(PartyVisual __instance, PartyBase party)
		{
			try
			{
				Banner banner = party.Banner;
				IFaction mapFaction = party.MapFaction;
				Clan actualClan = party.MobileParty.ActualClan;
				string clan = "";
				if (actualClan != null)
				{
					clan = ((object)actualClan.Name).ToString();
				}
				CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(party);
				string unitName = ((object)((BasicCharacterObject)visualPartyLeader).Name).ToString();
				(int, uint?, uint?, string, string) colorFromConfig = getColorFromConfig(clan, ref banner, unitName, ((BasicCharacterObject)visualPartyLeader).IsHero, ((BasicCharacterObject)visualPartyLeader).IsMounted, ((BasicCharacterObject)visualPartyLeader).IsRanged, visualPartyLeader.Tier, ((object)((BasicCultureObject)visualPartyLeader.Culture).Name).ToString());
				int item = colorFromConfig.Item1;
				uint? item2 = colorFromConfig.Item2;
				uint? item3 = colorFromConfig.Item3;
				string item4 = colorFromConfig.Item4;
				string item5 = colorFromConfig.Item5;
				savedColorData = new ColorData(item, item2, item3, item4, item5);
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}

		public static void Postfix()
		{
			savedColorData = null;
		}
	}

	[HarmonyPatch(typeof(PartyVisual), "AddCharacterToPartyIcon")]
	internal class PocColorModPartyVisual
	{
		public static bool Prefix(PartyVisual __instance, CharacterObject characterObject, ref string bannerKey, ref uint teamColor1, ref uint teamColor2)
		{
			try
			{
				string text = ((characterObject != null) ? ((object)((BasicCharacterObject)characterObject).Name).ToString() : null);
				if (savedColorData != null)
				{
					if (savedColorData.newshield != null && bannerKey != null)
					{
						bannerKey = savedColorData.newshield;
					}
					else if (savedColorData.newbanner != null && bannerKey != null)
					{
						bannerKey = savedColorData.newbanner;
					}
					if (savedColorData.c1.HasValue && savedColorData.c2.HasValue)
					{
						teamColor1 = savedColorData.c1.Value;
						teamColor2 = savedColorData.c2.Value;
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class PocColorModPartyBanner
	{
		public static bool Prefix(ref PartyNameplateVM __instance, ref ImageIdentifierVM value)
		{
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Expected O, but got Unknown
			try
			{
				if (__instance == null)
				{
					return true;
				}
				if (__instance.Party == null)
				{
					return true;
				}
				Clan actualClan = __instance.Party.ActualClan;
				if (actualClan == null)
				{
					return true;
				}
				string clan = ((object)actualClan.Name).ToString();
				string text = null;
				string text2 = null;
				bool isMounted = false;
				bool isRanged = false;
				bool isHero = false;
				bool flag = ((object)__instance.Party.Name).ToString().Equals("Looters");
				if (__instance.Party.LeaderHero != null && !flag)
				{
					Hero leaderHero = __instance.Party.LeaderHero;
					text = PocColorMod.SerializeBanner((leaderHero != null) ? leaderHero.ClanBanner : null);
					text2 = ((object)__instance.Party.LeaderHero.Name).ToString();
					isHero = true;
					string iconFromConfig = getIconFromConfig(clan, ref text, text2, isHero, isMounted, isRanged, 0, null);
					value = new ImageIdentifierVM(BannerCode.CreateFrom(iconFromConfig), true);
				}
				else if (__instance.Party.HomeSettlement != null && !flag)
				{
					string bannerStr = PocColorMod.SerializeBanner(__instance.Party.HomeSettlement.OwnerClan.Banner);
					string unitName = ((object)__instance.Party.HomeSettlement.Owner.Name).ToString();
					string iconFromConfig2 = getIconFromConfig(((object)__instance.Party.HomeSettlement.OwnerClan.Name).ToString(), ref bannerStr, unitName, isHero, isMounted, isRanged, 0, null);
					value = new ImageIdentifierVM(BannerCode.CreateFrom(iconFromConfig2), true);
				}
			}
			catch (Exception ex)
			{
				Log.write(ex.Message);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Clan), "UpdateBannerColorsAccordingToKingdom")]
	internal class PocColorModOverrideBanner
	{
		public static bool Prefix(ref Clan __instance, ref string __state)
		{
			try
			{
				if (PocColorMod.config == null)
				{
					return true;
				}
				__state = PocColorMod.SerializeBanner(__instance.Banner);
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
			return true;
		}

		public static void Postfix(ref Clan __instance, string __state)
		{
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Expected O, but got Unknown
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Expected O, but got Unknown
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Expected O, but got Unknown
			try
			{
				if (PocColorMod.config == null)
				{
					return;
				}
				Kingdom kingdom = __instance.Kingdom;
				Clan playerClan = Clan.PlayerClan;
				string text = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
				Clan playerClan2 = Clan.PlayerClan;
				object obj;
				if (playerClan2 == null)
				{
					obj = null;
				}
				else
				{
					Kingdom kingdom2 = playerClan2.Kingdom;
					obj = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
				}
				string text2 = (string)obj;
				bool isPlayerClan = !string.IsNullOrEmpty(text) && ((object)__instance.Name)?.ToString() == text;
				bool isPlayerKingdom = !string.IsNullOrEmpty(text2) && ((kingdom == null) ? null : ((object)kingdom.Name)?.ToString()) == text2;
				(bool, bool, string, string, string, string) clanConfig = PocColorMod.config.GetClanConfig((kingdom == null) ? null : ((object)kingdom.Name)?.ToString(), ((object)__instance.Name)?.ToString(), isPlayerKingdom, isPlayerClan);
				bool item = clanConfig.Item1;
				bool item2 = clanConfig.Item2;
				string item3 = clanConfig.Item3;
				string item4 = clanConfig.Item4;
				string item5 = clanConfig.Item5;
				string item6 = clanConfig.Item6;
				uint num = __instance.Color;
				uint num2 = __instance.Color2;
				if (item5 != null)
				{
					num = parseColor(item5);
				}
				if (item6 != null)
				{
					num2 = parseColor(item6);
				}
				if (!item && !item2 && item3 == null)
				{
					Traverse.Create((object)__instance).Field("_banner").SetValue((object)new Banner(__state));
				}
				if (!item && !item2 && item3 != null)
				{
					Banner value = new Banner(item3);
					Traverse.Create((object)__instance).Field("_banner").SetValue((object)value);
					if (item5 != null)
					{
						Banner banner = __instance.Banner;
						if (banner != null)
						{
							banner.ChangePrimaryColor(num);
						}
					}
					if (item6 != null)
					{
						Banner banner2 = __instance.Banner;
						if (banner2 != null)
						{
							banner2.ChangeIconColors(num2);
						}
					}
				}
				if ((item || item2) && item3 != null)
				{
					Banner value2 = new Banner(item3);
					Traverse.Create((object)__instance).Field("_banner").SetValue((object)value2);
					Banner banner3 = __instance.Banner;
					if (banner3 != null)
					{
						banner3.ChangePrimaryColor(kingdom.PrimaryBannerColor);
					}
					if (!item2)
					{
						Banner banner4 = __instance.Banner;
						if (banner4 != null)
						{
							banner4.ChangeIconColors(kingdom.SecondaryBannerColor);
						}
					}
				}
				if (__instance.Banner != null && PocColorMod.bannerClanCache != null)
				{
					string key = PocColorMod.SerializeBanner(__instance.Banner);
					Map<string, string> bannerClanCache = PocColorMod.bannerClanCache;
					Clan obj2 = __instance;
					bannerClanCache.TryAdd(key, (obj2 != null) ? ((object)obj2.Name).ToString() : null);
					if (kingdom != null)
					{
						PocColorMod.bannerKingdomCache.TryAdd(key, (kingdom != null) ? ((object)kingdom.Name).ToString() : null);
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error caught:" + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(BannerEditorVM), "SetClanRelatedRules")]
	internal class BannerEditorVMSetClanRules
	{
		private static bool Prefix(BannerEditorVM __instance, ref bool canChangeBackgroundColor)
		{
			canChangeBackgroundColor = true;
			return true;
		}
	}

	[HarmonyPatch(typeof(GauntletBannerEditorScreen), "OnDone")]
	internal class BannerEditorGauntletScreen_OnDone
	{
		private static void Postfix(ref GauntletBannerEditorScreen __instance)
		{
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			try
			{
				Clan value = Traverse.Create((object)__instance).Field<Clan>("_clan").Value;
				Kingdom kingdom = value.Kingdom;
				Clan playerClan = Clan.PlayerClan;
				string text = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
				Clan playerClan2 = Clan.PlayerClan;
				object obj;
				if (playerClan2 == null)
				{
					obj = null;
				}
				else
				{
					Kingdom kingdom2 = playerClan2.Kingdom;
					obj = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
				}
				string text2 = (string)obj;
				bool isPlayerClan = !string.IsNullOrEmpty(text) && ((object)value.Name)?.ToString() == text;
				bool isPlayerKingdom = !string.IsNullOrEmpty(text2) && ((kingdom == null) ? null : ((object)kingdom.Name)?.ToString()) == text2;
				(bool, bool, string, string, string, string) clanConfig = PocColorMod.config.GetClanConfig((kingdom == null) ? null : ((object)kingdom.Name)?.ToString(), ((object)value.Name)?.ToString(), isPlayerKingdom, isPlayerClan);
				bool item = clanConfig.Item1;
				bool item2 = clanConfig.Item2;
				string item3 = clanConfig.Item3;
				string item4 = clanConfig.Item4;
				string item5 = clanConfig.Item5;
				string item6 = clanConfig.Item6;
				uint color = value.Color;
				uint color2 = value.Color2;
				if (item3 != null)
				{
					Banner value2 = new Banner(item3);
					Traverse.Create((object)value).Field("_banner").SetValue((object)value2);
				}
				if (item5 != null)
				{
					color = parseColor(item5);
					Banner banner = value.Banner;
					if (banner != null)
					{
						banner.ChangePrimaryColor(color);
					}
				}
				if (item6 != null)
				{
					color2 = parseColor(item6);
					Banner banner2 = value.Banner;
					if (banner2 != null)
					{
						banner2.ChangeIconColors(color2);
					}
				}
			}
			catch (Exception ex)
			{
				Log.write("Error while applying colors: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(Banner), "Serialize")]
	internal class PocColorModFixBannerSerialize
	{
		public static bool Prefix(ref Banner __instance, ref string __result)
		{
			try
			{
				__result = PocColorMod.SerializeBanner(__instance);
				return false;
			}
			catch (Exception ex)
			{
				Log.write("Error caught while Serializing banner:" + ex.Message);
			}
			return true;
		}
	}

	public const int DEFAULT_MODE = 0;

	public const int RANDOMIZED = 1;

	public const int RANDOMIZED_LINKED = 11;

	public const int SHIELD_LINKED = 2;

	public const int SHIELD_LINKED_RANDOM = 21;

	public const int SHIELD_LINKED_RANDOM_FULL = 22;

	public const int BANNER_LINKED = 4;

	public const int BANNER_LINKED_RANDOM = 41;

	public const int BANNER_LINKED_RANDOM_FULL = 42;

	public const int OVERRIDE = 3;

	public const int COLOR_COUNT = 158;

	public static Random rnd = new Random();

	public static Map<string, Integer> extraColorMap = new Map<string, Integer>();

	public static Map<string, ColorData> colorDataCache = new Map<string, ColorData>();

	public static ColorData savedColorData;

	public static string DUMMY_BANNER1 = "30.82.88.3734.3734.768.768.1.0.0.522.40.40.924.924.762.775.0.0.0";

	public static string DUMMY_BANNER2 = "30.116.83.3734.3734.768.768.1.0.0.514.40.40.924.924.762.775.0.0.0";

	public static string DUMMY_BANNER3 = "4.82.88.1638.1638.768.768.1.0.0.517.116.40.924.924.762.775.0.0.0";

	public static uint parseColor(string colorStr)
	{
		if (colorStr.StartsWith("#"))
		{
			string value = "FF" + colorStr.Substring(1, 6);
			return Convert.ToUInt32(value, 16);
		}
		try
		{
			int num = int.Parse(colorStr);
			return BannerManager.GetColor(num);
		}
		catch (Exception)
		{
			return BannerManager.GetColor(40);
		}
	}

	public static uint getRandomColor()
	{
		int num = rnd.Next(0, 158);
		return BannerManager.GetColor(num);
	}

	public static uint getRandomColorFromColors(string[] colors)
	{
		int num = rnd.Next(0, colors.Count());
		return parseColor(colors[num]);
	}

	public static string getRandomBannerFromBanners(string[] banners)
	{
		int num = rnd.Next(0, banners.Count());
		return banners[num];
	}

	public static (uint?, uint?) getRandomPairColorsFromColors(string[] colors, string[] colors2)
	{
		int maxValue = Math.Min(colors.Count(), colors2.Count());
		int num = rnd.Next(0, maxValue);
		return (parseColor(colors[num]), parseColor(colors2[num]));
	}

	private static (int, uint?, uint?, string, string) getColorFromConfig(string clan, ref Banner banner, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		string bannerStr = ((banner != null) ? PocColorMod.SerializeBanner(banner) : null);
		return getColorFromConfig(clan, ref banner, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
	}

	private static (int, uint?, uint?, string, string) getColorFromConfig(string clan, ref string bannerStr, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Banner banner = ((bannerStr == null) ? ((Banner)null) : new Banner(bannerStr));
		return getColorFromConfig(clan, ref banner, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
	}

	private static (int, uint?, uint?, string, string) getBattleColorFromConfig(string clan, ref Banner banner, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		string bannerStr = ((banner != null) ? PocColorMod.SerializeBanner(banner) : null);
		return getBattleColorFromConfig(clan, ref banner, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
	}

	private static (int, uint?, uint?, string, string) getBattleColorFromConfig(string clan, ref string bannerStr, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Banner banner = ((bannerStr == null) ? ((Banner)null) : new Banner(bannerStr));
		return getBattleColorFromConfig(clan, ref banner, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
	}

	private static (string, string) getBannerFromConfig(string clan, string bannerStr, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		string item = null;
		string item2 = null;
		try
		{
			bool flag = unitName == ((object)Hero.MainHero.Name).ToString();
			string text = "";
			Clan val = null;
			Kingdom val2 = null;
			if (clan == null)
			{
				clan = PocColorMod.bannerClanCache[bannerStr] ?? "";
			}
			if (clan == null)
			{
				text = PocColorMod.bannerKingdomCache[bannerStr] ?? "";
			}
			else
			{
				val = Clan.FindFirst((Predicate<Clan>)isClan);
				if (val != null)
				{
					Kingdom kingdom = val.Kingdom;
					text = ((kingdom != null) ? ((object)kingdom.Name).ToString() : null);
				}
			}
			for (int i = 0; i < ((List<Kingdom>)(object)Campaign.Current.Kingdoms).Count; i++)
			{
				val2 = ((((object)((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i].Name).ToString() == text) ? ((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i] : null);
				if (val2 != null)
				{
					break;
				}
			}
			bool isKing = false;
			bool isLeader = false;
			if (unitName != null)
			{
				object obj;
				if (val2 == null)
				{
					obj = null;
				}
				else
				{
					Hero leader = val2.Leader;
					obj = ((leader == null) ? null : ((object)leader.Name)?.ToString());
				}
				isKing = (string)obj == unitName;
			}
			if (unitName != null)
			{
				object obj2;
				if (val == null)
				{
					obj2 = null;
				}
				else
				{
					Hero leader2 = val.Leader;
					obj2 = ((leader2 != null) ? ((object)leader2.Name).ToString() : null);
				}
				isLeader = (string)obj2 == unitName;
			}
			Clan playerClan = Clan.PlayerClan;
			string text2 = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
			Clan playerClan2 = Clan.PlayerClan;
			object obj3;
			if (playerClan2 == null)
			{
				obj3 = null;
			}
			else
			{
				Kingdom kingdom2 = playerClan2.Kingdom;
				obj3 = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
			}
			string text3 = (string)obj3;
			if (flag && clan != text2 && !string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(clan))
			{
				PocColorMod.bannerClanCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerKingdomCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerClanCache.TryAdd(bannerStr, text2);
				clan = text2;
				if (text3 != null)
				{
					PocColorMod.bannerKingdomCache.TryAdd(bannerStr, text3);
					text = text3;
				}
			}
			bool isPlayerClan = (!string.IsNullOrEmpty(text2) && clan == text2) || (flag && !string.IsNullOrEmpty(clan));
			bool isPlayerKingdom = !string.IsNullOrEmpty(text3) && text == text3;
			if (PocColorMod.doLog)
			{
				Log.write("==> character is: [" + unitName + "] of clan [" + clan + "] of kingdom [" + text + "] isPlayerKingdom [" + isPlayerKingdom + "] isPlayerClan [" + isPlayerClan + "] isPlayer [" + flag + "] isKing [" + isKing + "] isLeader [" + isLeader + "] isHero [" + isHero + "] isMounted [" + isMounted + "] isRanged [" + isRanged + "] tier [" + tier + "] culture [" + culture + "]");
			}
			(int, string[], string[], string[], string[]) config = PocColorMod.config.GetConfig(text, clan, unitName, isPlayerKingdom, isPlayerClan, flag, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
			int item3 = config.Item1;
			string[] item4 = config.Item2;
			string[] item5 = config.Item3;
			string[] item6 = config.Item4;
			string[] item7 = config.Item5;
			string text4 = ((item4 != null) ? string.Join(",", item4) : "");
			string text5 = ((item5 != null) ? string.Join(",", item5) : "");
			string text6 = ((item7 != null) ? string.Join(",", item7) : "");
			string text7 = ((item6 != null) ? string.Join(",", item6) : "");
			if (PocColorMod.doLog)
			{
				Log.write("> Applying colors according to: mode[" + item3 + "] colors[" + text4 + "] colors2[" + text5 + "] newBanner[" + text7 + "] shields[" + text6 + "]");
			}
			if (item6 != null && item6.Length != 0)
			{
				item = getRandomBannerFromBanners(item6);
			}
			if (item7 != null && item7.Length != 0)
			{
				item2 = getRandomBannerFromBanners(item7);
			}
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
		return (item, item2);
		bool isClan(Clan c)
		{
			return ((object)c.Name).ToString() == clan;
		}
	}

	private static void updateCache(string bannerStr, string unitName)
	{
		try
		{
			bool flag = unitName == ((object)Hero.MainHero.Name).ToString();
			string text = PocColorMod.bannerClanCache[bannerStr] ?? "";
			Clan playerClan = Clan.PlayerClan;
			string text2 = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
			Clan playerClan2 = Clan.PlayerClan;
			object obj;
			if (playerClan2 == null)
			{
				obj = null;
			}
			else
			{
				Kingdom kingdom = playerClan2.Kingdom;
				obj = ((kingdom == null) ? null : ((object)kingdom.Name)?.ToString());
			}
			string text3 = (string)obj;
			if (flag && text != text2 && text2 != null)
			{
				PocColorMod.bannerClanCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerKingdomCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerClanCache.TryAdd(bannerStr, text2);
				text = text2;
				if (text3 != null)
				{
					PocColorMod.bannerKingdomCache.TryAdd(bannerStr, text3);
				}
			}
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
	}

	private static string getIconFromConfig(string clan, ref string bannerStr, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		bool flag = unitName == ((object)Hero.MainHero.Name).ToString();
		string text = "";
		if (clan == null)
		{
			clan = PocColorMod.bannerClanCache[bannerStr] ?? "";
		}
		if (clan == null)
		{
			text = PocColorMod.bannerKingdomCache[bannerStr] ?? "";
		}
		else
		{
			Clan val = Clan.FindFirst((Predicate<Clan>)isClan);
			if (val != null)
			{
				Kingdom kingdom = val.Kingdom;
				text = ((kingdom != null) ? ((object)kingdom.Name).ToString() : null) ?? "";
			}
		}
		Clan playerClan = Clan.PlayerClan;
		string text2 = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
		Clan playerClan2 = Clan.PlayerClan;
		object obj;
		if (playerClan2 == null)
		{
			obj = null;
		}
		else
		{
			Kingdom kingdom2 = playerClan2.Kingdom;
			obj = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
		}
		string text3 = (string)obj;
		bool isPlayerKingdom = !string.IsNullOrEmpty(text3) && text == text3;
		bool isPlayerClan = (!string.IsNullOrEmpty(text2) && clan == text2) || (flag && !string.IsNullOrEmpty(clan));
		bool flag2;
		bool flag3;
		string text4;
		string text5;
		string text6;
		string text7;
		(flag2, flag3, text4, text5, text6, text7) = PocColorMod.config.GetClanConfig(text, clan, isPlayerKingdom, isPlayerClan);
		if (text5 != null)
		{
			return text5;
		}
		(text4, text5, text6, text7) = PocColorMod.config.GetKingdomConfig(text, isPlayerKingdom);
		if (text5 != null)
		{
			return text5;
		}
		var (num, num2, num3, text8, text9) = getColorFromConfig(clan, ref bannerStr, unitName, isHero, isMounted, isRanged, tier, culture);
		if (text9 != null)
		{
			return text9;
		}
		return bannerStr;
		bool isClan(Clan cl)
		{
			return ((object)cl.Name).ToString() == clan;
		}
	}

	private static (int, uint?, uint?, string, string) getColorFromConfig(string clan, ref Banner banner, ref string bannerStr, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Expected O, but got Unknown
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0719: Expected O, but got Unknown
		uint? num = null;
		uint? item = null;
		string text = null;
		string text2 = null;
		int num2 = 2;
		try
		{
			bool flag = unitName == ((object)Hero.MainHero.Name).ToString();
			string text3 = "";
			Clan val = null;
			Kingdom val2 = null;
			if (clan == null)
			{
				clan = PocColorMod.bannerClanCache[bannerStr] ?? "";
			}
			if (clan == null)
			{
				text3 = PocColorMod.bannerKingdomCache[bannerStr] ?? "";
			}
			else
			{
				val = Clan.FindFirst((Predicate<Clan>)isClan);
				if (val != null)
				{
					Kingdom kingdom = val.Kingdom;
					text3 = ((kingdom != null) ? ((object)kingdom.Name).ToString() : null);
				}
			}
			for (int i = 0; i < ((List<Kingdom>)(object)Campaign.Current.Kingdoms).Count; i++)
			{
				val2 = ((((object)((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i].Name).ToString() == text3) ? ((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i] : null);
				if (val2 != null)
				{
					break;
				}
			}
			bool isKing = false;
			bool isLeader = false;
			if (unitName != null)
			{
				object obj;
				if (val2 == null)
				{
					obj = null;
				}
				else
				{
					Hero leader = val2.Leader;
					obj = ((leader == null) ? null : ((object)leader.Name)?.ToString());
				}
				isKing = (string)obj == unitName;
			}
			if (unitName != null)
			{
				object obj2;
				if (val == null)
				{
					obj2 = null;
				}
				else
				{
					Hero leader2 = val.Leader;
					obj2 = ((leader2 != null) ? ((object)leader2.Name).ToString() : null);
				}
				isLeader = (string)obj2 == unitName;
			}
			Clan playerClan = Clan.PlayerClan;
			string text4 = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
			Clan playerClan2 = Clan.PlayerClan;
			object obj3;
			if (playerClan2 == null)
			{
				obj3 = null;
			}
			else
			{
				Kingdom kingdom2 = playerClan2.Kingdom;
				obj3 = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
			}
			string text5 = (string)obj3;
			if (flag && clan != text4 && !string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(clan))
			{
				PocColorMod.bannerClanCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerKingdomCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerClanCache.TryAdd(bannerStr, text4);
				clan = text4;
				if (text5 != null)
				{
					PocColorMod.bannerKingdomCache.TryAdd(bannerStr, text5);
					text3 = text5;
				}
			}
			bool isPlayerClan = (!string.IsNullOrEmpty(text4) && clan == text4) || (flag && !string.IsNullOrEmpty(clan));
			bool isPlayerKingdom = !string.IsNullOrEmpty(text5) && text3 == text5;
			if (PocColorMod.doLog)
			{
				Log.write("==> character is: [" + unitName + "] of clan [" + clan + "] of kingdom [" + text3 + "] isPlayerKingdom [" + isPlayerKingdom + "] isPlayerClan [" + isPlayerClan + "] isPlayer [" + flag + "] isKing [" + isKing + "] isLeader [" + isLeader + "] isHero [" + isHero + "] isMounted [" + isMounted + "] isRanged [" + isRanged + "] tier [" + tier + "] culture [" + culture + "]");
			}
			(int, string[], string[], string[], string[]) config = PocColorMod.config.GetConfig(text3, clan, unitName, isPlayerKingdom, isPlayerClan, flag, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
			int item2 = config.Item1;
			string[] array = config.Item2;
			string[] array2 = config.Item3;
			string[] item3 = config.Item4;
			string[] item4 = config.Item5;
			num2 = item2;
			string text6 = ((array != null) ? string.Join(",", array) : "");
			string text7 = ((array2 != null) ? string.Join(",", array2) : "");
			string text8 = ((item4 != null) ? string.Join(",", item4) : "");
			string text9 = ((item3 != null) ? string.Join(",", item3) : "");
			if (PocColorMod.doLog)
			{
				Log.write("> Applying colors according to: mode[" + num2 + "] colors[" + text6 + "] colors2[" + text7 + "] newBanner[" + text9 + "] shields[" + text8 + "]");
			}
			if (item3 != null && item3.Length != 0)
			{
				text = (bannerStr = getRandomBannerFromBanners(item3));
				banner = new Banner(text);
			}
			if (num2 == 4 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 1)
			{
				int colorId = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
				int colorId2 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
				num = BannerManager.GetColor(colorId);
				item = BannerManager.GetColor(colorId2);
			}
			else if (num2 == 41 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 1)
			{
				int colorId3 = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
				int colorId4 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
				string[] colors = new string[2]
				{
					colorId3.ToString(),
					colorId4.ToString()
				};
				string[] colors2 = new string[2]
				{
					colorId4.ToString(),
					colorId3.ToString()
				};
				(num, item) = getRandomPairColorsFromColors(colors, colors2);
			}
			else if (num2 == 42 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 0)
			{
				HashSet<string> hashSet = new HashSet<string>();
				for (int j = 0; j < ((List<BannerData>)(object)banner.BannerDataList).Count; j++)
				{
					hashSet.Add(((List<BannerData>)(object)banner.BannerDataList)[j].ColorId.ToString());
				}
				num = getRandomColorFromColors(hashSet.ToArray());
				item = getRandomColorFromColors(hashSet.ToArray());
			}
			if (item4 != null && item4.Length != 0)
			{
				text2 = (bannerStr = getRandomBannerFromBanners(item4));
				banner = new Banner(text2);
			}
			if (num2 == 2 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 1)
			{
				int colorId5 = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
				int colorId6 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
				num = BannerManager.GetColor(colorId5);
				item = BannerManager.GetColor(colorId6);
			}
			else
			{
				switch (num2)
				{
				case 1:
					if ((array == null || Extensions.IsEmpty<string>((IEnumerable<string>)array)) && (array2 == null || Extensions.IsEmpty<string>((IEnumerable<string>)array2)))
					{
						num = getRandomColor();
						item = getRandomColor();
					}
					else
					{
						if (array == null)
						{
							array = array2;
						}
						if (array2 == null)
						{
							array2 = array;
						}
						num = getRandomColorFromColors(array);
						array2 = ((array2 == null) ? array : array2);
						item = getRandomColorFromColors(array2);
					}
					goto end_IL_0026;
				case 11:
					if ((array == null || Extensions.IsEmpty<string>((IEnumerable<string>)array)) && (array2 == null || Extensions.IsEmpty<string>((IEnumerable<string>)array2)))
					{
						num = getRandomColor();
						item = num;
					}
					else
					{
						if (array == null)
						{
							array = array2;
						}
						if (array2 == null)
						{
							array2 = array;
						}
						(num, item) = getRandomPairColorsFromColors(array, array2);
					}
					goto end_IL_0026;
				case 3:
					num = parseColor(array[0]);
					item = parseColor(array[1]);
					goto end_IL_0026;
				case 21:
				{
					if (banner == null || banner.BannerDataList == null || ((List<BannerData>)(object)banner.BannerDataList).Count <= 1)
					{
						break;
					}
					int colorId7 = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
					int colorId8 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
					string[] colors3 = new string[2]
					{
						colorId7.ToString(),
						colorId8.ToString()
					};
					string[] colors4 = new string[2]
					{
						colorId8.ToString(),
						colorId7.ToString()
					};
					(num, item) = getRandomPairColorsFromColors(colors3, colors4);
					goto end_IL_0026;
				}
				}
				if (num2 == 22 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 0)
				{
					HashSet<string> hashSet2 = new HashSet<string>();
					for (int k = 0; k < ((List<BannerData>)(object)banner.BannerDataList).Count; k++)
					{
						hashSet2.Add(((List<BannerData>)(object)banner.BannerDataList)[k].ColorId.ToString());
					}
					num = getRandomColorFromColors(hashSet2.ToArray());
					item = getRandomColorFromColors(hashSet2.ToArray());
				}
			}
			end_IL_0026:;
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
		return (num2, num, item, text, text2);
		bool isClan(Clan c)
		{
			return ((object)c.Name).ToString() == clan;
		}
	}

	private static (int, uint?, uint?, string, string) getBattleColorFromConfig(string clan, ref Banner banner, ref string bannerStr, string unitName, bool isHero, bool isMounted, bool isRanged, int tier, string culture)
	{
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Expected O, but got Unknown
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0719: Expected O, but got Unknown
		uint? num = null;
		uint? item = null;
		string text = null;
		string text2 = null;
		int num2 = 2;
		try
		{
			bool flag = unitName == ((object)Hero.MainHero.Name).ToString();
			string text3 = "";
			Clan val = null;
			Kingdom val2 = null;
			if (clan == null)
			{
				clan = PocColorMod.bannerClanCache[bannerStr] ?? "";
			}
			if (clan == null)
			{
				text3 = PocColorMod.bannerKingdomCache[bannerStr] ?? "";
			}
			else
			{
				val = Clan.FindFirst((Predicate<Clan>)isClan);
				if (val != null)
				{
					Kingdom kingdom = val.Kingdom;
					text3 = ((kingdom != null) ? ((object)kingdom.Name).ToString() : null);
				}
			}
			for (int i = 0; i < ((List<Kingdom>)(object)Campaign.Current.Kingdoms).Count; i++)
			{
				val2 = ((((object)((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i].Name).ToString() == text3) ? ((List<Kingdom>)(object)Campaign.Current.Kingdoms)[i] : null);
				if (val2 != null)
				{
					break;
				}
			}
			bool isKing = false;
			bool isLeader = false;
			if (unitName != null)
			{
				object obj;
				if (val2 == null)
				{
					obj = null;
				}
				else
				{
					Hero leader = val2.Leader;
					obj = ((leader == null) ? null : ((object)leader.Name)?.ToString());
				}
				isKing = (string)obj == unitName;
			}
			if (unitName != null)
			{
				object obj2;
				if (val == null)
				{
					obj2 = null;
				}
				else
				{
					Hero leader2 = val.Leader;
					obj2 = ((leader2 != null) ? ((object)leader2.Name).ToString() : null);
				}
				isLeader = (string)obj2 == unitName;
			}
			Clan playerClan = Clan.PlayerClan;
			string text4 = ((playerClan == null) ? null : ((object)playerClan.Name)?.ToString());
			Clan playerClan2 = Clan.PlayerClan;
			object obj3;
			if (playerClan2 == null)
			{
				obj3 = null;
			}
			else
			{
				Kingdom kingdom2 = playerClan2.Kingdom;
				obj3 = ((kingdom2 == null) ? null : ((object)kingdom2.Name)?.ToString());
			}
			string text5 = (string)obj3;
			if (flag && clan != text4 && !string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(clan))
			{
				PocColorMod.bannerClanCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerKingdomCache.TryRemove(bannerStr, out var _);
				PocColorMod.bannerClanCache.TryAdd(bannerStr, text4);
				clan = text4;
				if (text5 != null)
				{
					PocColorMod.bannerKingdomCache.TryAdd(bannerStr, text5);
					text3 = text5;
				}
			}
			bool isPlayerClan = (!string.IsNullOrEmpty(text4) && clan == text4) || (flag && !string.IsNullOrEmpty(clan));
			bool isPlayerKingdom = !string.IsNullOrEmpty(text5) && text3 == text5;
			if (PocColorMod.doLog)
			{
				Log.write("==> character is: [" + unitName + "] of clan [" + clan + "] of kingdom [" + text3 + "] isPlayerKingdom [" + isPlayerKingdom + "] isPlayerClan [" + isPlayerClan + "] isPlayer [" + flag + "] isKing [" + isKing + "] isLeader [" + isLeader + "] isHero [" + isHero + "] isMounted [" + isMounted + "] isRanged [" + isRanged + "] tier [" + tier + "] culture [" + culture + "]");
			}
			(int, string[], string[], string[], string[]) battleConfig = PocColorMod.config.GetBattleConfig(text3, clan, unitName, isPlayerKingdom, isPlayerClan, flag, isKing, isLeader, isHero, isMounted, isRanged, tier, culture);
			int item2 = battleConfig.Item1;
			string[] array = battleConfig.Item2;
			string[] array2 = battleConfig.Item3;
			string[] item3 = battleConfig.Item4;
			string[] item4 = battleConfig.Item5;
			num2 = item2;
			string text6 = ((array != null) ? string.Join(",", array) : "");
			string text7 = ((array2 != null) ? string.Join(",", array2) : "");
			string text8 = ((item4 != null) ? string.Join(",", item4) : "");
			string text9 = ((item3 != null) ? string.Join(",", item3) : "");
			if (PocColorMod.doLog)
			{
				Log.write("> Applying colors according to: mode[" + num2 + "] colors[" + text6 + "] colors2[" + text7 + "] newBanner[" + text9 + "] shields[" + text8 + "]");
			}
			if (item3 != null && item3.Length != 0)
			{
				text = (bannerStr = getRandomBannerFromBanners(item3));
				banner = new Banner(text);
			}
			if (num2 == 4 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 1)
			{
				int colorId = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
				int colorId2 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
				num = BannerManager.GetColor(colorId);
				item = BannerManager.GetColor(colorId2);
			}
			else if (num2 == 41 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 1)
			{
				int colorId3 = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
				int colorId4 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
				string[] colors = new string[2]
				{
					colorId3.ToString(),
					colorId4.ToString()
				};
				string[] colors2 = new string[2]
				{
					colorId4.ToString(),
					colorId3.ToString()
				};
				(num, item) = getRandomPairColorsFromColors(colors, colors2);
			}
			else if (num2 == 42 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 0)
			{
				HashSet<string> hashSet = new HashSet<string>();
				for (int j = 0; j < ((List<BannerData>)(object)banner.BannerDataList).Count; j++)
				{
					hashSet.Add(((List<BannerData>)(object)banner.BannerDataList)[j].ColorId.ToString());
				}
				num = getRandomColorFromColors(hashSet.ToArray());
				item = getRandomColorFromColors(hashSet.ToArray());
			}
			if (item4 != null && item4.Length != 0)
			{
				text2 = (bannerStr = getRandomBannerFromBanners(item4));
				banner = new Banner(text2);
			}
			if (num2 == 2 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 1)
			{
				int colorId5 = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
				int colorId6 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
				num = BannerManager.GetColor(colorId5);
				item = BannerManager.GetColor(colorId6);
			}
			else
			{
				switch (num2)
				{
				case 1:
					if ((array == null || Extensions.IsEmpty<string>((IEnumerable<string>)array)) && (array2 == null || Extensions.IsEmpty<string>((IEnumerable<string>)array2)))
					{
						num = getRandomColor();
						item = getRandomColor();
					}
					else
					{
						if (array == null)
						{
							array = array2;
						}
						if (array2 == null)
						{
							array2 = array;
						}
						num = getRandomColorFromColors(array);
						array2 = ((array2 == null) ? array : array2);
						item = getRandomColorFromColors(array2);
					}
					goto end_IL_0026;
				case 11:
					if ((array == null || Extensions.IsEmpty<string>((IEnumerable<string>)array)) && (array2 == null || Extensions.IsEmpty<string>((IEnumerable<string>)array2)))
					{
						num = getRandomColor();
						item = num;
					}
					else
					{
						if (array == null)
						{
							array = array2;
						}
						if (array2 == null)
						{
							array2 = array;
						}
						(num, item) = getRandomPairColorsFromColors(array, array2);
					}
					goto end_IL_0026;
				case 3:
					num = parseColor(array[0]);
					item = parseColor(array[1]);
					goto end_IL_0026;
				case 21:
				{
					if (banner == null || banner.BannerDataList == null || ((List<BannerData>)(object)banner.BannerDataList).Count <= 1)
					{
						break;
					}
					int colorId7 = ((List<BannerData>)(object)banner.BannerDataList)[0].ColorId;
					int colorId8 = ((List<BannerData>)(object)banner.BannerDataList)[1].ColorId;
					string[] colors3 = new string[2]
					{
						colorId7.ToString(),
						colorId8.ToString()
					};
					string[] colors4 = new string[2]
					{
						colorId8.ToString(),
						colorId7.ToString()
					};
					(num, item) = getRandomPairColorsFromColors(colors3, colors4);
					goto end_IL_0026;
				}
				}
				if (num2 == 22 && banner != null && banner.BannerDataList != null && ((List<BannerData>)(object)banner.BannerDataList).Count > 0)
				{
					HashSet<string> hashSet2 = new HashSet<string>();
					for (int k = 0; k < ((List<BannerData>)(object)banner.BannerDataList).Count; k++)
					{
						hashSet2.Add(((List<BannerData>)(object)banner.BannerDataList)[k].ColorId.ToString());
					}
					num = getRandomColorFromColors(hashSet2.ToArray());
					item = getRandomColorFromColors(hashSet2.ToArray());
				}
			}
			end_IL_0026:;
		}
		catch (Exception ex)
		{
			Log.write(ex.Message);
		}
		return (num2, num, item, text, text2);
		bool isClan(Clan c)
		{
			return ((object)c.Name).ToString() == clan;
		}
	}

	public static bool updateAgentBuildDataColors(ref AgentBuildData __instance, ref uint color1, ref uint color2, bool first)
	{
		BasicCharacterObject agentCharacter = __instance.AgentCharacter;
		Banner banner = __instance.AgentBanner;
		int num = 2;
		try
		{
			if (agentCharacter != null)
			{
				if (banner != null)
				{
					string charName2 = ((object)((MBObjectBase)agentCharacter).GetName()).ToString();
					CharacterObject val = CharacterObject.FindFirst((Predicate<CharacterObject>)((CharacterObject c) => ((object)((BasicCharacterObject)c).Name).ToString().Equals(charName2)));
					uint? num2;
					uint? num3;
					string text;
					string text2;
					(num, num2, num3, text, text2) = getColorFromConfig(null, ref banner, ((object)((MBObjectBase)agentCharacter).GetName()).ToString(), agentCharacter.IsHero, agentCharacter.IsMounted, agentCharacter.IsRanged, val.Tier, ((object)agentCharacter.Culture.Name).ToString());
					if (num2.HasValue && num3.HasValue)
					{
						color1 = num2.Value;
						color2 = num3.Value;
						if (first)
						{
							extraColorMap.TryAdd(((MBObjectBase)agentCharacter).StringId, new Integer(color2));
						}
						else
						{
							extraColorMap.TryAdd(((MBObjectBase)agentCharacter).StringId, new Integer(color1));
						}
						Traverse.Create((object)__instance).Field("AgentBanner").SetValue((object)banner);
						__instance.AgentData.ClothingColor1(color1);
						__instance.AgentData.ClothingColor2(color2);
					}
				}
				else
				{
					IAgentOriginBase agentOrigin = __instance.AgentOrigin;
					if (agentOrigin != null)
					{
						banner = agentOrigin.Banner;
						if (banner != null)
						{
							string charName = ((object)((MBObjectBase)agentCharacter).GetName()).ToString();
							CharacterObject val2 = CharacterObject.FindFirst((Predicate<CharacterObject>)((CharacterObject c) => ((object)((BasicCharacterObject)c).Name).ToString().Equals(charName)));
							uint? num2;
							uint? num3;
							if (savedColorData == null)
							{
								string text;
								string text2;
								(num, num2, num3, text, text2) = getColorFromConfig(null, ref banner, ((object)((MBObjectBase)agentCharacter).GetName()).ToString(), agentCharacter.IsHero, agentCharacter.IsMounted, agentCharacter.IsRanged, 0, ((object)agentCharacter.Culture.Name).ToString());
							}
							else
							{
								int mode = savedColorData.mode;
								uint? c2 = savedColorData.c1;
								uint? c3 = savedColorData.c2;
								string newbanner = savedColorData.newbanner;
								string text2 = savedColorData.newshield;
								string text = newbanner;
								num3 = c3;
								num2 = c2;
								num = mode;
							}
							if (num2.HasValue && num3.HasValue)
							{
								color1 = num2.Value;
								color2 = num3.Value;
								Traverse.Create((object)__instance).Field("AgentBanner").SetValue((object)banner);
								__instance.AgentData.ClothingColor1(color1);
								__instance.AgentData.ClothingColor2(color2);
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.write("Error caught:" + ex.Message);
		}
		return true;
	}
}
