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
    private readonly PatchType _patchType;

    public ClanBannerColorPatch(PocColorRuntimeBridge bridge, PatchType patchType)
    {
        _bridge = bridge;
        _patchType = patchType;
    }

    public MethodInfo TargetMethod => AccessTools.Method(typeof(Clan), "UpdateBannerColorsAccordingToKingdom");

    public MethodInfo PatchMethod => AccessTools.Method(
        typeof(ClanBannerColorPatch),
        _patchType == PatchType.Prefix ? nameof(Prefix) : nameof(Postfix));

    public PatchType PatchType => _patchType;

    private static void Prefix(Clan __instance, out string __state)
    {
        try
        {
            __state = _bridge.CaptureClanBannerBeforeKingdomUpdate(__instance);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(exception);
            __state = null;
        }
    }

    private static void Postfix(Clan __instance, string __state)
    {
        try
        {
            _bridge.RestoreClanBannerAfterKingdomUpdate(__instance, __state);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(exception);
        }
    }
}
