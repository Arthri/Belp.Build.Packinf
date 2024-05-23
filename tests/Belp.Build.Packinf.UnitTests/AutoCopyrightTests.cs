using Belp.Build.Testing;
using Belp.Build.Testing.Resources;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Build.Execution;
using Xunit;

namespace Belp.Build.Packinf.UnitTests;

public class AutoCopyrightTests
{
    public class Enabled
    {
        [Fact]
        public void Expect_BLP4002_when_copyright_is_explicitly_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4002.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutorh>true</DisableAutorh>
                    <Copyright>Copyright (C) 2012 John Doe.</Copyright>
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
        public void Expect_BLP4004_when_no_copyright_owners_are_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4004.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutorh>true</DisableAutorh>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Failure);

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Error("BLP4004", "No copyright owners defined.", ThisPackage.GetPath("buildMultiTargeting", "AutoCopyright.targets"), ((14, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }

        [Fact]
        public void Expect_BLP4005_when_copyright_owners_do_not_have_years_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4005.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutorh>true</DisableAutorh>
                  </PropertyGroup>

                  <ItemGroup>
                    <CopyrightOwner Include="John Doe" />
                  </ItemGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Failure);

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Error("BLP4005", """No copyrightable years defined for copyright owner "John Doe".""", ThisPackage.GetPath("buildMultiTargeting", "AutoCopyright.targets"), ((18, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }
    }

    public class Disabled
    {
        [Fact]
        public void Do_not_expect_BLP4002_when_copyright_is_explicitly_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4002.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <DisableAutorh>true</DisableAutorh>
                    <Copyright>Copyright (C) 2012 John Doe.</Copyright>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEmpty();
        }

        [Fact]
        public void Expect_BLP4003_when_copyright_is_not_defined()
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
        public void Do_not_expect_BLP4003_when_copyright_is_not_defined_while_required_copyright_notice_is_disabled()
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

            result.Diagnostics.Should().BeEmpty();
        }

        [Fact]
        public void Do_not_expect_BLP4004_when_no_copyright_owners_are_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4004.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <DisableAutorh>true</DisableAutorh>
                    <Copyright>Copyright (C) 2012 John Doe.</Copyright>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEmpty();
        }
    }

    [Fact]
    public void Expect_copyright_notice_is_generated_in_proper_format()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4005.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutorh>true</DisableAutorh>
              </PropertyGroup>

              <ItemGroup>
                <CopyrightOwner Include="John Doe" Years="2012, 2013" />
                <CopyrightOwner Include="Jane Doe" Years="2011, 2013, 2014" />
              </ItemGroup>

              <Target Name="ForceEvaluateCopyright" AfterTargets="Build">
                <PropertyGroup>
                  <Copyright>$(Copyright)</Copyright>
                </PropertyGroup>
              </Target>

            </Project>
            """
        );
        MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEmpty();

        result.Properties.Should().ContainKey("CopyrightGenerated").WhoseValue.Should().Be("true");
        result.Properties.Should().ContainKey("Copyright").WhoseValue.Should().Be("Copyright (C) 2012, 2013 John Doe.\nCopyright (C) 2011, 2013, 2014 Jane Doe.\nAll rights reserved.");
    }
}
