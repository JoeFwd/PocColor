using System.Collections.Generic;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;
using PocColor.Domain.Appearance.Resolution;
using Xunit;

namespace PocColor.Domain.Tests.Appearance;

public sealed class ResolveColorsUtilTests
{
    private static readonly IReadOnlyList<ColorReference> Empty = new ColorReference[0];

    // ----------------------------------------------------------------
    // No strategy
    // ----------------------------------------------------------------

    [Fact]
    public void Null_strategy_returns_null_null()
    {
        var (primary, secondary) = Util().Resolve(null, Empty, Empty, Empty, Empty, Seq());

        Assert.Null(primary);
        Assert.Null(secondary);
    }

    // ----------------------------------------------------------------
    // RandomConfigured
    // ----------------------------------------------------------------

    [Fact]
    public void RandomConfigured_picks_independently_from_primary_and_secondary()
    {
        var primary = Colors(0xFF010101u, 0xFF020202u);
        var secondary = Colors(0xFF030303u, 0xFF040404u);

        var (p, s) = Util().Resolve(
            ColorStrategy.RandomConfigured, primary, secondary, Empty, Empty,
            Seq(1, 0));

        Assert.Equal(0xFF020202u, p.Argb);
        Assert.Equal(0xFF030303u, s.Argb);
    }

    [Fact]
    public void RandomConfigured_falls_back_to_defaults_when_configured_is_empty()
    {
        var defaults = Colors(0xFF010101u, 0xFF020202u);

        var (p, s) = Util().Resolve(
            ColorStrategy.RandomConfigured, Empty, Empty, Empty, defaults,
            Seq(0, 1));

        Assert.Equal(0xFF010101u, p.Argb);
        Assert.Equal(0xFF020202u, s.Argb);
    }

    [Fact]
    public void RandomConfigured_uses_secondary_as_primary_fallback_when_primary_empty()
    {
        var secondary = Colors(0xFF030303u);
        var defaults = Colors(0xFF010101u, 0xFF020202u);

        // primary empty → use secondary list for primary; secondary available
        var (p, s) = Util().Resolve(
            ColorStrategy.RandomConfigured, Empty, secondary, Empty, defaults,
            Seq(1, 0));

        Assert.Equal(0xFF020202u, p.Argb); // secondary empty → default[1]
        Assert.Equal(0xFF030303u, s.Argb);
    }

    // ----------------------------------------------------------------
    // RandomPaired
    // ----------------------------------------------------------------

    [Fact]
    public void RandomPaired_uses_one_index_for_both_channels()
    {
        var primary = Colors(0xFF010101u, 0xFF020202u);
        var secondary = Colors(0xFF030303u, 0xFF040404u);

        var (p, s) = Util().Resolve(
            ColorStrategy.RandomPaired, primary, secondary, Empty, Empty,
            Seq(1));

        Assert.Equal(0xFF020202u, p.Argb);
        Assert.Equal(0xFF040404u, s.Argb);
    }

    [Fact]
    public void RandomPaired_shares_one_default_draw_when_both_channels_empty()
    {
        var choices = new SequenceChoiceSource(1);
        var defaults = Colors(0xFF010101u, 0xFF020202u);

        var (p, s) = Util().Resolve(
            ColorStrategy.RandomPaired, Empty, Empty, Empty, defaults,
            choices);

        Assert.Equal(0xFF020202u, p.Argb);
        Assert.Equal(0xFF020202u, s.Argb);
        Assert.Equal(1, choices.CallCount);
    }

    // ----------------------------------------------------------------
    // FixedPair
    // ----------------------------------------------------------------

    [Fact]
    public void FixedPair_uses_first_two_from_primary_colors()
    {
        var primary = Colors(0xFF010203u, 0xFF040506u, 0xFF070809u);

        var (p, s) = Util().Resolve(
            ColorStrategy.FixedPair, primary, Empty, Empty, Empty, Seq());

        Assert.Equal(0xFF010203u, p.Argb);
        Assert.Equal(0xFF040506u, s.Argb);
    }

    [Fact]
    public void FixedPair_returns_null_null_when_fewer_than_two_primaries()
    {
        var primary = Colors(0xFF010203u);

        var (p, s) = Util().Resolve(
            ColorStrategy.FixedPair, primary, Empty, Empty, Empty, Seq());

        Assert.Null(p);
        Assert.Null(s);
    }

    // ----------------------------------------------------------------
    // BannerFirstTwo
    // ----------------------------------------------------------------

    [Fact]
    public void BannerFirstTwo_uses_first_two_banner_colors()
    {
        var banner = Colors(0xFF010203u, 0xFF040506u, 0xFF070809u);

        var (p, s) = Util().Resolve(
            ColorStrategy.BannerFirstTwo, Empty, Empty, banner, Empty, Seq());

        Assert.Equal(0xFF010203u, p.Argb);
        Assert.Equal(0xFF040506u, s.Argb);
    }

    [Fact]
    public void BannerFirstTwo_returns_null_null_when_fewer_than_two_banner_colors()
    {
        var banner = Colors(0xFF010203u);

        var (p, s) = Util().Resolve(
            ColorStrategy.BannerFirstTwo, Empty, Empty, banner, Empty, Seq());

        Assert.Null(p);
        Assert.Null(s);
    }

    // ----------------------------------------------------------------
    // BannerFirstTwoRandomOrder
    // ----------------------------------------------------------------

    [Fact]
    public void BannerFirstTwoRandomOrder_returns_in_order_when_choice_is_0()
    {
        var banner = Colors(0xFF010203u, 0xFF040506u);

        var (p, s) = Util().Resolve(
            ColorStrategy.BannerFirstTwoRandomOrder, Empty, Empty, banner, Empty, Seq(0));

        Assert.Equal(0xFF010203u, p.Argb);
        Assert.Equal(0xFF040506u, s.Argb);
    }

    [Fact]
    public void BannerFirstTwoRandomOrder_returns_reversed_when_choice_is_1()
    {
        var banner = Colors(0xFF010203u, 0xFF040506u);

        var (p, s) = Util().Resolve(
            ColorStrategy.BannerFirstTwoRandomOrder, Empty, Empty, banner, Empty, Seq(1));

        Assert.Equal(0xFF040506u, p.Argb);
        Assert.Equal(0xFF010203u, s.Argb);
    }

    [Fact]
    public void BannerFirstTwoRandomOrder_returns_null_null_when_banner_has_fewer_than_two_colors()
    {
        var banner = Colors(0xFF010203u);

        var (p, s) = Util().Resolve(
            ColorStrategy.BannerFirstTwoRandomOrder, Empty, Empty, banner, Empty, Seq(0));

        Assert.Null(p);
        Assert.Null(s);
    }

    // ----------------------------------------------------------------
    // BannerRandomColors
    // ----------------------------------------------------------------

    [Fact]
    public void BannerRandomColors_picks_independently_from_banner_colors()
    {
        var banner = Colors(0xFF010101u, 0xFF020202u, 0xFF030303u);

        var (p, s) = Util().Resolve(
            ColorStrategy.BannerRandomColors, Empty, Empty, banner, Empty, Seq(2, 0));

        Assert.Equal(0xFF030303u, p.Argb);
        Assert.Equal(0xFF010101u, s.Argb);
    }

    // ----------------------------------------------------------------
    // Unresolved / unknown strategy
    // ----------------------------------------------------------------

    [Fact]
    public void Unresolved_strategy_returns_null_null()
    {
        var (p, s) = Util().Resolve(
            ColorStrategy.Unresolved, Empty, Empty, Empty, Empty, Seq());

        Assert.Null(p);
        Assert.Null(s);
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static IResolveColorsUtil Util() => new ResolveColorsUtil();

    private static IReadOnlyList<ColorReference> Colors(params uint[] argbs)
    {
        var refs = new ColorReference[argbs.Length];
        for (int i = 0; i < argbs.Length; i++)
        {
            refs[i] = ColorReference.FromArgb(argbs[i]);
        }

        return refs;
    }

    private static SequenceChoiceSource Seq(params int[] values) => new SequenceChoiceSource(values);
}
