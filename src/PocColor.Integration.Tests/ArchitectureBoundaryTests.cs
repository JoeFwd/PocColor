using System;
using System.Reflection;
using PocColor.Domain.Appearance;
using Xunit;

namespace PocColor.Integration.Tests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void Domain_has_no_taleworlds_reference()
    {
        Assembly domain = typeof(AppearanceRuleSet).Assembly;

        Assert.DoesNotContain(
            domain.GetReferencedAssemblies(),
            assembly => assembly.Name.StartsWith("TaleWorlds.", StringComparison.Ordinal));
    }

    [Fact]
    public void Integration_preserves_module_assembly_name()
    {
        Assert.Equal("PocColor", typeof(PocColorSubModule).Assembly.GetName().Name);
    }
}
