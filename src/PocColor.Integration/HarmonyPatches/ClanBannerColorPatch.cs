using System;
using System.Reflection;
using Harmony.DependencyInjection.Patches;
using HarmonyLib;
using PocColor.Integration.Runtime;
using TaleWorlds.CampaignSystem;

namespace PocColor.Integration.HarmonyPatches;

internal sealed class ClanBannerColorPatch : IPatch
{
    private static PocColorRuntimeBridge _bridge;

    public ClanBannerColorPatch(PocColorRuntimeBridge bridge)
    {
        _bridge = bridge;
    }

    public MethodInfo TargetMethod => AccessTools.Method(typeof(Clan), "UpdateBannerColorsAccordingToKingdom");

    public MethodInfo PatchMethod => AccessTools.Method(typeof(ClanBannerColorPatch), nameof(Postfix));

    public PatchType PatchType => PatchType.Postfix;

    private static void Postfix(Clan __instance)
    {
        try
        {
            _bridge.ReapplyClanBanner(__instance);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(exception);
        }
    }
}
