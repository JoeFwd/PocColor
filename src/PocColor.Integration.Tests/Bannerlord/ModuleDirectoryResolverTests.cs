using System.Linq;
using PocColor.Infrastructure.LegacyConfig;
using Xunit;

namespace PocColor.Integration.Tests.Bannerlord;

public sealed class ModuleDirectoryResolverTests
{
    [Fact]
    public void Discovers_configuration_roots_from_a_staged_module_path()
    {
        const string stagedModule = @"D:\Game\bin\Modules\PocColor";
        string[] roots = ModuleDirectoryResolver.GetCandidates(stagedModule).ToArray();

        Assert.Contains(@"D:\Game\bin\Modules", roots);
        Assert.Contains(@"D:\Game\Modules", roots);
        Assert.DoesNotContain(stagedModule + "\\Modules", roots);
    }
}
