using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PocColor.Domain.Appearance;
using PocColor.Infrastructure.LegacyConfig;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.DI;

public static class PocColorInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPocColorInfrastructure(
        this IServiceCollection services,
        string moduleDirectory)
    {
        services.AddSingleton(new LegacyConfigLoader(moduleDirectory));
        services.AddSingleton<LegacyConfigCompiler>();
        services.AddSingleton(provider => provider.GetRequiredService<LegacyConfigLoader>().Load());
        services.AddSingleton<LegacyConfigCompileResult>(CompileConfiguration);
        services.AddSingleton(provider => provider.GetRequiredService<LegacyConfigCompileResult>().Rules);
        return services;
    }

    private static LegacyConfigCompileResult CompileConfiguration(IServiceProvider provider)
    {
        LegacyConfigLoader loader = provider.GetRequiredService<LegacyConfigLoader>();
        PocColorModConfig configuration = provider.GetRequiredService<PocColorModConfig>();
        WriteDiagnostics(loader.Diagnostics);

        LegacyConfigCompileResult compiled = provider.GetRequiredService<LegacyConfigCompiler>().Compile(configuration);
        WriteDiagnostics(compiled.Diagnostics);
        return compiled;
    }

    private static void WriteDiagnostics(System.Collections.Generic.IReadOnlyList<LegacyConfigDiagnostic> diagnostics)
    {
        foreach (LegacyConfigDiagnostic diagnostic in diagnostics)
        {
            Trace.WriteLine(diagnostic.Code + ": " + diagnostic.Message);
        }
    }
}
