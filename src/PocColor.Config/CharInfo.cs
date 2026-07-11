namespace PocColor.Config;

public class CharInfo
{
	public string name { get; set; }

	public bool isHero { get; set; }

	public CharInfo(string name, bool isHero)
	{
		this.name = name;
		this.isHero = isHero;
	}
}
