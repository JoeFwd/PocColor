using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig;

public sealed class LegacyConfigLoader
{
    private const int MaxModuleConfigSearchDepth = 3;

    private readonly string _moduleDirectory;
    private readonly List<LegacyConfigDiagnostic> _diagnostics = new List<LegacyConfigDiagnostic>();

    public LegacyConfigLoader(string moduleDirectory)
    {
        _moduleDirectory = moduleDirectory;
    }

    public IReadOnlyList<LegacyConfigDiagnostic> Diagnostics =>
        new ReadOnlyCollection<LegacyConfigDiagnostic>(_diagnostics.ToArray());

    public PocColorModConfig Load()
    {
        _diagnostics.Clear();
        var config = new PocColorModConfig();
        foreach (string path in GetConfigPaths())
        {
            PocColorModConfig fragment = ReadConfig(path);
            if (fragment != null)
            {
                LegacyConfigMerger.Merge(config, fragment);
            }
        }

        return config;
    }

    private IEnumerable<string> GetConfigPaths()
    {
        string legacyPath = Path.Combine(_moduleDirectory, "config.json");
        if (File.Exists(legacyPath))
        {
            yield return legacyPath;
        }

        foreach (string modulesDirectory in ModuleDirectoryResolver.GetCandidates(_moduleDirectory)
            .Where(Directory.Exists)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            IEnumerable<string> moduleDirectories;
            try
            {
                moduleDirectories = Directory.GetDirectories(modulesDirectory)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                AddDiagnostic("ConfigScanFailure", "Could not scan module directory '" + modulesDirectory + "': " + exception.Message);
                continue;
            }

            foreach (string configPath in moduleDirectories
                .SelectMany(module => EnumeratePocConfigs(module, 1))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                if (!string.Equals(configPath, legacyPath, StringComparison.OrdinalIgnoreCase))
                {
                    yield return configPath;
                }
            }
        }
    }

    private IEnumerable<string> EnumeratePocConfigs(string directory, int depth)
    {
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(directory).ToArray();
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            AddDiagnostic("ConfigScanFailure", "Could not scan config files in '" + directory + "': " + exception.Message);
            yield break;
        }

        foreach (string file in files)
        {
            if (IsPocConfigFile(file))
            {
                yield return file;
            }
        }

        if (depth >= MaxModuleConfigSearchDepth)
        {
            yield break;
        }

        IEnumerable<string> childDirectories;
        try
        {
            childDirectories = Directory.EnumerateDirectories(directory).ToArray();
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            AddDiagnostic("ConfigScanFailure", "Could not scan config directories in '" + directory + "': " + exception.Message);
            yield break;
        }

        foreach (string childDirectory in childDirectories.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            foreach (string file in EnumeratePocConfigs(childDirectory, depth + 1))
            {
                yield return file;
            }
        }
    }

    private static bool IsPocConfigFile(string path)
    {
        string fileName = Path.GetFileName(path);
        return string.Equals(fileName, "poc.config", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "poc.config.json", StringComparison.OrdinalIgnoreCase);
    }

    private PocColorModConfig ReadConfig(string path)
    {
        try
        {
            return JsonConvert.DeserializeObject<PocColorModConfig>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            AddDiagnostic("ConfigReadFailure", "Could not parse config '" + path + "': " + exception.Message);
            return null;
        }
    }

    private void AddDiagnostic(string code, string message)
    {
        _diagnostics.Add(new LegacyConfigDiagnostic(code, message));
    }
}
