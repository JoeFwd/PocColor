using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using Xunit;

namespace PocColor.Domain.Tests.Appearance;

public sealed class ColorReferenceTests
{
    [Fact]
    public void Resolver_preserves_palette_ids_and_equal_bit_argb_values_as_distinct_references()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = new RuleOverride(
                strategy: ColorStrategy.RandomConfigured,
                primaryColorReferences: new[]
                {
                    ColorReference.FromPaletteId(83),
                    ColorReference.FromArgb(0x00000053u)
                },
                secondaryColorReferences: new[]
                {
                    ColorReference.FromPaletteId(83),
                    ColorReference.FromArgb(0x00000053u)
                })
        });

        var resolver = new ResolveAppearanceUseCase();
        ResolvedAppearance paletteResult = resolver.Resolve(
            rules,
            Profile(),
            new BannerSnapshot("current", new[] { ColorReference.FromArgb(0xFF000000u) }),
            ResolutionKind.Normal,
            new SequenceChoiceSource(0, 0));
        ResolvedAppearance argbResult = resolver.Resolve(
            rules,
            Profile(),
            new BannerSnapshot("current", new[] { ColorReference.FromArgb(0xFF000000u) }),
            ResolutionKind.Normal,
            new SequenceChoiceSource(1, 1));

        Assert.Equal(ColorReferenceKind.PaletteId, paletteResult.PrimaryColorReference.Kind);
        Assert.Equal(83, paletteResult.PrimaryColorReference.PaletteId);
        Assert.Equal(ColorReferenceKind.Argb, argbResult.PrimaryColorReference.Kind);
        Assert.Equal(0x00000053u, argbResult.PrimaryColorReference.Argb);
    }

    [Fact]
    public void Default_palette_colours_preserve_their_reference_kind()
    {
        var rules = new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = new RuleOverride(strategy: ColorStrategy.RandomConfigured)
        });
        var palette = new AppearancePalette(
            new Dictionary<string, BannerSnapshot>(),
            new[]
            {
                ColorReference.FromPaletteId(83),
                ColorReference.FromArgb(0x00000053u)
            });

        ResolvedAppearance result = new ResolveAppearanceUseCase().Resolve(
            rules,
            Profile(),
            new BannerSnapshot("current", new[] { ColorReference.FromArgb(0xFF000000u) }),
            ResolutionKind.Normal,
            new SequenceChoiceSource(0, 0),
            palette);

        Assert.Equal(ColorReferenceKind.PaletteId, result.PrimaryColorReference.Kind);
        Assert.Equal(83, result.PrimaryColorReference.PaletteId);
        Assert.Equal(ColorReferenceKind.PaletteId, result.SecondaryColorReference.Kind);
    }

    private static UnitProfile Profile()
    {
        return new UnitProfile(
            null, null, "Trooper", false, false, false, false, false, false,
            false, false, 1, null, null);
    }
}
