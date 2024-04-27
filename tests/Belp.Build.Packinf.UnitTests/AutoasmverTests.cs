using Belp.Build.Test.MSBuild;
using Belp.Build.Test.MSBuild.Resources;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Build.Execution;
using Xunit;

namespace Belp.Build.Packinf.UnitTests;

public class AutoasmverTests
{
    public class Enabled
    {
        [Fact]
        public void Expect_BLP4001_when_assembly_version_is_explicitly_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4001.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <DisableAutorh>true</DisableAutorh>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <AssemblyVersion>2.1.0.0</AssemblyVersion>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Failure);

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Error("BLP4001", "AssemblyVersion is already defined. Uninstall Packinf or disable Autoasmver(by setting $(DisableAutoasmver) to true) to manually define AssemblyVersion.", ThisPackage.GetPath("buildMultiTargeting", "Autoasmver.targets"), ((5, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }
    }

    public class Disabled
    {
        [Fact]
        public void Do_not_expect_BLP4001_when_assembly_version_is_explicitly_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4001.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoasmver>true</DisableAutoasmver>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <DisableAutorh>true</DisableAutorh>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <AssemblyVersion>2.1.0.0</AssemblyVersion>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEquivalentTo((IEnumerable<Diagnostic>)[]);
        }
    }

    [Theory]
    [InlineData("1.54.86.34", "1.0.0.0")]
    [InlineData("3.78.2.8", "3.0.0.0")]
    [InlineData("12.0.1", "12.0.0.0")]
    [InlineData("8.0", "8.0.0.0")]
    [InlineData("9.0.0.0", "9.0.0.0")]
    public void Expect_assembly_version_is_properly_generated(string version, string expectedAssemblyVersion)
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4001.csproj",
            $"""
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutoCopyright>true</DisableAutoCopyright>
                <DisableAutorh>true</DisableAutorh>
                <RequireCopyrightNotice>false</RequireCopyrightNotice>
                <Version>{version}</Version>
              </PropertyGroup>

            </Project>
            """
        );
        MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEquivalentTo((IEnumerable<Diagnostic>)[]);

        result.Properties.Should().ContainKey("AssemblyVersion").WhoseValue.Should().Be(expectedAssemblyVersion);
    }
}
