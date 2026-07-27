using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PocColor.Domain.Appearance;
using PocColor.Domain.Appearance.Model;
using PocColor.Domain.Appearance.Port;
using PocColor.Integration.Runtime;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class AgentBuildDataAppearanceTests
{
    [Fact]
    public void Builder_callback_resolves_once_for_both_clothing_colours()
    {
        var choices = new CountingChoiceSource(0);
        var bridge = CreateBridge(CreateRandomConfiguredRules(), choices);

        bridge.BeginAgentBuild(CreateAgentBuildData(), CreateBanner());
        bridge.ApplyClothingColor1();
        bridge.ApplyClothingColor2();

        Assert.Equal(1, choices.CallCount);
        Assert.Equal(bridge.LastResolvedAppearance.PrimaryColor, bridge.LastAppliedColor1);
        Assert.Equal(bridge.LastResolvedAppearance.SecondaryColor, bridge.LastAppliedColor2);
    }

    [Fact]
    public void Battle_banner_random_order_consumes_the_real_colour_choice_after_selecting_a_banner()
    {
        var choices = new SequenceChoiceSource(0, 1);
        var bridge = CreateBridge(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.Default] = new RuleOverride(
                    strategy: ColorStrategy.BannerFirstTwoRandomOrder,
                    battleBannerCodes: new[] { BannerWithPaletteColors(11, 22) })
            }),
            choices);

        bridge.BeginAgentBuild(CreateAgentBuildData(), CreateBanner());

        Assert.Equal(2, choices.CallCount);
        Assert.Equal(22, bridge.LastResolvedAppearance.PrimaryColorReference.PaletteId);
        Assert.Equal(11, bridge.LastResolvedAppearance.SecondaryColorReference.PaletteId);
    }

    [Fact]
    public void Ruler_banner_random_colours_consume_both_real_colour_choices_after_selecting_the_ruler_banner()
    {
        var choices = new SequenceChoiceSource(0, 0, 2, 0);
        var bridge = CreateBridge(
            new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
            {
                [ScopeKey.Default] = new RuleOverride(
                    strategy: ColorStrategy.BannerRandomColors,
                    rulerStrategy: ColorStrategy.BannerRandomColors)
            }),
            choices);
        var rulerProfile = new UnitProfile(
            null, null, "Trooper", false, false, false, false, false, false,
            false, false, 1, "Culture", BannerWithPaletteColors(31, 32, 33));

        ResolvedAppearance appearance = bridge.ResolveForProfile(
            rulerProfile,
            CreateBanner(),
            ResolutionKind.Battle);

        Assert.Equal(4, choices.CallCount);
        Assert.Equal(33, appearance.PrimaryColorReference.PaletteId);
        Assert.Equal(31, appearance.SecondaryColorReference.PaletteId);
    }

    private static PocColorRuntimeBridge CreateBridge(AppearanceRuleSet rules, IChoiceSource choices)
    {
        return new PocColorRuntimeBridge(rules, choices);
    }

    private static AppearanceRuleSet CreateRandomConfiguredRules()
    {
        return new AppearanceRuleSet(new Dictionary<ScopeKey, RuleOverride>
        {
            [ScopeKey.Default] = new RuleOverride(
                strategy: ColorStrategy.RandomPaired,
                primaryColors: new[] { 0xFF102030u, 0xFF405060u },
                secondaryColors: new[] { 0xFF708090u, 0xFFA0B0C0u })
        });
    }

    private static AgentBuildData CreateAgentBuildData()
    {
        return (AgentBuildData)RuntimeHelpers.GetUninitializedObject(typeof(AgentBuildData));
    }

    private static Banner CreateBanner() => new Banner();

    private static string BannerWithPaletteColors(params int[] colors)
    {
        var fields = new List<string>();
        foreach (int color in colors)
        {
            fields.Add("1");
            fields.Add(color.ToString());
            fields.Add("0");
            fields.Add("0");
            fields.Add("0");
            fields.Add("0");
            fields.Add("0");
            fields.Add("0");
            fields.Add("0");
            fields.Add("0");
        }

        return string.Join(".", fields);
    }

    private sealed class CountingChoiceSource : IChoiceSource
    {
        private readonly int _value;

        public CountingChoiceSource(int value)
        {
            _value = value;
        }

        public int CallCount { get; private set; }

        public int NextIndex(int exclusiveUpperBound)
        {
            CallCount++;
            return _value;
        }
    }

    private sealed class SequenceChoiceSource : IChoiceSource
    {
        private readonly Queue<int> _values;

        public SequenceChoiceSource(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public int CallCount { get; private set; }

        public int NextIndex(int exclusiveUpperBound)
        {
            CallCount++;
            return _values.Dequeue();
        }
    }
}
