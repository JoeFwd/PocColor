using System.Collections.Generic;
using System.Collections.ObjectModel;
using PocColor.Domain.Appearance;

namespace PocColor.Infrastructure.LegacyConfig;

public sealed class LegacyConfigCompileResult
{
    public LegacyConfigCompileResult(
        AppearanceRuleSet rules,
        IReadOnlyList<LegacyConfigDiagnostic> diagnostics)
    {
        Rules = rules ?? new AppearanceRuleSet(null);

        var copy = new LegacyConfigDiagnostic[diagnostics == null ? 0 : diagnostics.Count];
        for (int index = 0; index < copy.Length; index++)
        {
            copy[index] = diagnostics[index];
        }

        Diagnostics = new ReadOnlyCollection<LegacyConfigDiagnostic>(copy);
    }

    public AppearanceRuleSet Rules { get; }

    public IReadOnlyList<LegacyConfigDiagnostic> Diagnostics { get; }
}
