using System;
using System.Collections.Generic;
using System.IO;

namespace PocColor.Infrastructure.LegacyConfig;

public static class ModuleDirectoryResolver
{
    public static IEnumerable<string> GetCandidates(string moduleDirectory)
    {
        if (string.IsNullOrWhiteSpace(moduleDirectory))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        DirectoryInfo current = new DirectoryInfo(Path.GetFullPath(moduleDirectory));
        while (current != null)
        {
            if (string.Equals(current.Name, "Modules", StringComparison.OrdinalIgnoreCase))
            {
                seen.Add(current.FullName);
            }

            if (current.Parent != null)
            {
                string parentModules = string.Equals(current.Parent.Name, "Modules", StringComparison.OrdinalIgnoreCase)
                    ? current.Parent.FullName
                    : Path.Combine(current.Parent.FullName, "Modules");
                seen.Add(parentModules);
            }

            current = current.Parent;
        }

        foreach (string path in seen)
        {
            yield return path;
        }
    }
}
