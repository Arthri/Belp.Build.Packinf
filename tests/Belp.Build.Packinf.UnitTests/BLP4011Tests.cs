using Belp.Build.Testing;
using Belp.Build.Testing.Resources;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Build.Execution;
using Xunit;

namespace Belp.Build.Packinf.UnitTests;

public class BLP4011Tests
{
    [Fact]
    public void Expect_BLP4011_when_Assets_contains_an_extensionless_file()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.Samples("BLP4011");
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Failure);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Error("BLP4011", "Extension-less files are not supported by PackageAsset.", Path.Combine(project.MSBuildProject.DirectoryPath, "Assets", "tools", "x"), default, project.MSBuildProject.FullPath)
        ]);
    }
}
