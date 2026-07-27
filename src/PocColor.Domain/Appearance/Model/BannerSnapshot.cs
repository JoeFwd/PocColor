using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PocColor.Domain.Appearance.Model;

public sealed class BannerSnapshot
{
    public BannerSnapshot(string serializedCode, IReadOnlyList<uint> colors)
        : this(serializedCode, ToReferences(colors))
    {
    }

    public BannerSnapshot(string serializedCode, IReadOnlyList<ColorReference> colors)
    {
        SerializedCode = serializedCode;
        Colors = Copy(colors);
    }

    public string SerializedCode { get; }

    public IReadOnlyList<ColorReference> Colors { get; }

    private static IReadOnlyList<ColorReference> Copy(IReadOnlyList<ColorReference> colors)
    {
        int count = colors == null ? 0 : colors.Count;
        var copy = new ColorReference[count];
        for (int index = 0; index < count; index++)
        {
            copy[index] = colors[index];
        }

        return new ReadOnlyCollection<ColorReference>(copy);
    }

    private static IReadOnlyList<ColorReference> ToReferences(IReadOnlyList<uint> colors)
    {
        if (colors == null)
        {
            return null;
        }

        var references = new ColorReference[colors.Count];
        for (int index = 0; index < colors.Count; index++)
        {
            references[index] = ColorReference.FromArgb(colors[index]);
        }

        return references;
    }
}
