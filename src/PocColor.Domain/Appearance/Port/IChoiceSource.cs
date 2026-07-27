namespace PocColor.Domain.Appearance.Port;

public interface IChoiceSource
{
    int NextIndex(int exclusiveUpperBound);
}
