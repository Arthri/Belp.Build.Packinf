using Belp.Build.Test.MSBuild;
using Belp.Build.Test.MSBuild.Resources;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Build.Execution;
using Xunit;

namespace Belp.Build.Packinf.UnitTests;

public class AutoCopyrightTests
{
    [Fact]
    public void Expect_BLP4002_when_copyright_is_explicitly_defined_while_copyright_generation_is_enabled()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4002.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutorh>true</DisableAutorh>
                <Copyright>Copyright (C) John Doe 2012.</Copyright>
              </PropertyGroup>

              <ItemGroup>
                <CopyrightOwner Include="John Doe" Years="2012" />
              </ItemGroup>

            </Project>
            """
        );
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Warn("BLP4002", "Copyright notice is explicitly defined while copyright generation is enabled. Remove explicitly defined copyright notice, disable AutoCopyright(set $(DisableAutoCopyright) to true), or uninstall Packinf to manually define copyright.", ThisPackage.GetPath("buildMultiTargeting", "AutoCopyright.targets"), ((13, 5), (0, 0)), project.MSBuildProject.FullPath)
        ]);
    }

    [Fact]
    public void Do_not_expect_BLP4002_when_copyright_is_explicitly_defined_while_copyright_generation_is_disabled()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4002.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutoCopyright>true</DisableAutoCopyright>
                <DisableAutorh>true</DisableAutorh>
                <Copyright>Copyright (C) John Doe 2012.</Copyright>
              </PropertyGroup>

            </Project>
            """
        );
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEquivalentTo((IEnumerable<Diagnostic>)[]);
    }

    [Fact]
    public void Expect_BLP4003_when_copyright_is_not_defined_and_copyright_generation_is_disabled()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4003.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutoCopyright>true</DisableAutoCopyright>
                <DisableAutorh>true</DisableAutorh>
              </PropertyGroup>

            </Project>
            """
        );
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Warn("BLP4003", "Copyright notice is not defined and copyright generation is disabled. Enable AutoCopyright(set $(DisableAutoCopyright) to false), explicitly define the copyright notice, or disable required copyright(set $(RequireCopyrightNotice) to false).", ThisPackage.GetPath("buildMultiTargeting", "AutoCopyright.targets"), ((22, 5), (0, 0)), project.MSBuildProject.FullPath)
        ]);
    }

    [Fact]
    public void Do_not_expect_BLP4003_when_copyright_is_not_defined_and_copyright_generation_is_disabled_while_required_copyright_notice_is_disabled()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4003.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutoCopyright>true</DisableAutoCopyright>
                <DisableAutorh>true</DisableAutorh>
                <RequireCopyrightNotice>false</RequireCopyrightNotice>
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
