using System;
using System.IO;
using System.Reflection;
using PocColor.Domain.Appearance.Model;
using TaleWorlds.Core;

namespace PocColor.Integration.Runtime;

internal static class ColorApplicationDiagnostics
{
    private static readonly object Gate = new();
    private static readonly string LogPath = Path.Combine(
        Path.GetDirectoryName(typeof(PocColorSubModule).Assembly.Location),
        "..",
        "..",
        "color-application.log");

    public static void WritePatchTarget(string name, MethodInfo target)
    {
        Write("patch-target\t" + name + "\t" + (target == null ? "NOT_FOUND" : target.ToString()));
    }

    public static void WriteResolved(BasicCharacterObject character, ResolvedAppearance appearance)
    {
        Write(
            "resolved\t" + CharacterId(character) + "\t" + CharacterName(character) + "\t"
            + (appearance?.Strategy?.ToString() ?? "NO_MATCH") + "\t"
            + Color(appearance?.PrimaryColor) + "\t" + Color(appearance?.SecondaryColor) + "\t"
            + (appearance?.BannerCode ?? string.Empty));
    }

    public static void WriteApplied(BasicCharacterObject character, string property, uint? color)
    {
        Write("applied\t" + CharacterId(character) + "\t" + CharacterName(character) + "\t" + property + "\t" + Color(color));
    }

    private static void Write(string line)
    {
        try
        {
            lock (Gate)
            {
                using var writer = new StreamWriter(new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                writer.WriteLine(line);
            }
        }
        catch
        {
        }
    }

    private static string CharacterId(BasicCharacterObject character) => character?.StringId ?? string.Empty;

    private static string CharacterName(BasicCharacterObject character) => character?.Name?.ToString() ?? string.Empty;

    private static string Color(uint? color) => color.HasValue ? color.Value.ToString("X8") : string.Empty;
}
