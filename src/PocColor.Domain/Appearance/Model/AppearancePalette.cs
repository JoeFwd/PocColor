using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PocColor.Domain.Appearance.Model;

public sealed class AppearancePalette
{
    private readonly IReadOnlyDictionary<string, BannerSnapshot> _banners;
    private readonly Func<string, BannerSnapshot> _fallback;

    public static AppearancePalette Empty { get; } = new AppearancePalette(null, (IReadOnlyList<ColorReference>)null);

    public AppearancePalette(
        IReadOnlyDictionary<string, BannerSnapshot> banners,
        IReadOnlyList<uint> defaultColors)
        : this(banners, ToReferences(defaultColors))
    {
    }

    public AppearancePalette(
        IReadOnlyDictionary<string, BannerSnapshot> banners,
        IReadOnlyList<ColorReference> defaultColors)
        : this(banners, defaultColors, null)
    {
    }

    public AppearancePalette(
        IReadOnlyDictionary<string, BannerSnapshot> banners,
        IReadOnlyList<ColorReference> defaultColors,
        Func<string, BannerSnapshot> fallback)
    {
        var bannerCopy = new Dictionary<string, BannerSnapshot>(StringComparer.Ordinal);
        if (banners != null)
        {
            foreach (KeyValuePair<string, BannerSnapshot> entry in banners)
            {
                if (entry.Key == null)
                {
                    throw new ArgumentException("A banner palette key cannot be null.", nameof(banners));
                }

                bannerCopy[entry.Key] = entry.Value;
            }
        }

        _banners = new ReadOnlyDictionary<string, BannerSnapshot>(bannerCopy);
        DefaultColors = Copy(defaultColors);
        _fallback = fallback;
    }

    public IReadOnlyList<ColorReference> DefaultColors { get; }

    public BannerSnapshot GetBanner(string serializedCode)
    {
        if (serializedCode == null)
        {
            return null;
        }

        return _banners.TryGetValue(serializedCode, out BannerSnapshot banner)
            ? banner
            : _fallback?.Invoke(serializedCode);
    }

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
