using Belp.Build.Testing;
using Belp.Build.Testing.Resources;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Build.Execution;
using Xunit;

namespace Belp.Build.Packinf.UnitTests;

public class PackageAssetTests
{
    [Fact]
    public void Expect_BLP4011_when_Assets_contains_an_extensionless_file_named_X5FPackage()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.Samples("BLP4011");
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Failure);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Error("BLP4011", "PackageAsset does not support extension-less files named _Package.", Path.Combine(project.MSBuildProject.DirectoryPath, "Assets", "tools", "_Package"), default, project.MSBuildProject.FullPath)
        ]);
    }

    [Fact]
    public void Expect_BLP4012_when_Assets_contains_an_extensionless_file_with_a_X5FPackage_folder_in_its_path()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.Samples("BLP4012");
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Failure);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Error("BLP4012", "PackageAsset does not support _Package in the paths of package files.", Path.Combine(project.MSBuildProject.DirectoryPath, "Assets", "_Package", "x"), default, project.MSBuildProject.FullPath)
        ]);
    }

    [Fact]
    public void Expect_BLP4013_when_AssetsX2Fcontent_contains_an_extensionless_file()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.Samples("BLP4013");
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Failure);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Error("BLP4013", "Content assets must have an extension.", Path.Combine(project.MSBuildProject.DirectoryPath, "Assets", "content", "x"), default, project.MSBuildProject.FullPath)
        ]);
    }
}
