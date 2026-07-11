namespace PocColor.Config;

public class PocColorModGroupEntry : PocColorModConfigUnit
{
	public string[] units { get; set; }

	public PocColorModConfigUnit toUnit()
	{
		PocColorModConfigUnit pocColorModConfigUnit = new PocColorModConfigUnit();
		pocColorModConfigUnit.mode = base.mode;
		pocColorModConfigUnit.color = base.color;
		pocColorModConfigUnit.color2 = base.color2;
		pocColorModConfigUnit.banner = base.banner;
		pocColorModConfigUnit.banners = base.banners;
		pocColorModConfigUnit.shields = base.shields;
		pocColorModConfigUnit.combatBanners = base.combatBanners;
		pocColorModConfigUnit.combatShields = base.combatShields;
		pocColorModConfigUnit.BearRulerColors = base.BearRulerColors;
		pocColorModConfigUnit.BearRulerBanner = base.BearRulerBanner;
		pocColorModConfigUnit.BearRulerShield = base.BearRulerShield;
		pocColorModConfigUnit.rulerMode = base.rulerMode;
		pocColorModConfigUnit.rulerColor = base.rulerColor;
		pocColorModConfigUnit.rulerColor2 = base.rulerColor2;
		pocColorModConfigUnit.rulerBanner = base.rulerBanner;
		pocColorModConfigUnit.rulerShield = base.rulerShield;
		pocColorModConfigUnit.cultures = base.cultures;
		pocColorModConfigUnit.tiers = base.tiers;
		pocColorModConfigUnit.types = base.types;
		return pocColorModConfigUnit;
	}

	public Map<string, PocColorModConfigUnit> toUnits()
	{
		Map<string, PocColorModConfigUnit> map = new Map<string, PocColorModConfigUnit>();
		PocColorModConfigUnit value = toUnit();
		if (units != null)
		{
			string[] array = units;
			foreach (string key in array)
			{
				map[key] = value;
			}
		}
		return map;
	}
}
