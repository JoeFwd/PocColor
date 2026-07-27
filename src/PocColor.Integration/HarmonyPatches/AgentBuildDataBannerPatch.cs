using System;
using System.Reflection;
using Harmony.DependencyInjection.Patches;
using HarmonyLib;
using PocColor.Integration.Runtime;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PocColor.Integration.HarmonyPatches;

internal sealed class AgentBuildDataBannerPatch : IPatch
{
    private static PocColorRuntimeBridge _bridge;

    public AgentBuildDataBannerPatch(PocColorRuntimeBridge bridge)
    {
        _bridge = bridge;
    }

    public MethodInfo TargetMethod => ResolveTarget();

    public MethodInfo PatchMethod => AccessTools.Method(typeof(AgentBuildDataBannerPatch), nameof(Prefix));

    public PatchType PatchType => PatchType.Prefix;

    private static void Prefix(AgentBuildData __instance, Banner banner)
    {
        try
        {
            _bridge.BeginAgentBuild(__instance, banner);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(exception);
        }
    }

    private static MethodInfo ResolveTarget()
    {
        MethodInfo target = AccessTools.Method(typeof(AgentBuildData), nameof(AgentBuildData.Banner));
        ColorApplicationDiagnostics.WritePatchTarget(nameof(AgentBuildData.Banner), target);
        return target;
    }
}

internal sealed class AgentBuildDataClothingColor1Patch : IPatch
{
    private static PocColorRuntimeBridge _bridge;

    public AgentBuildDataClothingColor1Patch(PocColorRuntimeBridge bridge)
    {
        _bridge = bridge;
    }

    public MethodInfo TargetMethod => ResolveTarget();

    public MethodInfo PatchMethod => AccessTools.Method(typeof(AgentBuildDataClothingColor1Patch), nameof(Prefix));

    public PatchType PatchType => PatchType.Prefix;

    private static void Prefix(AgentBuildData __instance, ref uint color)
    {
        try
        {
            uint? resolvedColor = _bridge.ApplyClothingColor1(__instance);
            if (resolvedColor.HasValue)
            {
                color = resolvedColor.Value;
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(exception);
        }
    }

    private static MethodInfo ResolveTarget()
    {
        MethodInfo target = AccessTools.Method(typeof(AgentBuildData), nameof(AgentBuildData.ClothingColor1));
        ColorApplicationDiagnostics.WritePatchTarget(nameof(AgentBuildData.ClothingColor1), target);
        return target;
    }
}

internal sealed class AgentBuildDataClothingColor2Patch : IPatch
{
    private static PocColorRuntimeBridge _bridge;

    public AgentBuildDataClothingColor2Patch(PocColorRuntimeBridge bridge)
    {
        _bridge = bridge;
    }

    public MethodInfo TargetMethod => ResolveTarget();

    public MethodInfo PatchMethod => AccessTools.Method(typeof(AgentBuildDataClothingColor2Patch), nameof(Prefix));

    public PatchType PatchType => PatchType.Prefix;

    private static void Prefix(AgentBuildData __instance, ref uint color)
    {
        try
        {
            uint? resolvedColor = _bridge.ApplyClothingColor2(__instance);
            if (resolvedColor.HasValue)
            {
                color = resolvedColor.Value;
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(exception);
        }
    }

    private static MethodInfo ResolveTarget()
    {
        MethodInfo target = AccessTools.Method(typeof(AgentBuildData), nameof(AgentBuildData.ClothingColor2));
        ColorApplicationDiagnostics.WritePatchTarget(nameof(AgentBuildData.ClothingColor2), target);
        return target;
    }
}
