using Belp.Build.Testing.Resources;
using Belp.Build.Testing;
using FluentAssertions.Execution;
using Microsoft.Build.Execution;
using Xunit;
using FluentAssertions;

namespace Belp.Build.Packinf.UnitTests;

public class AutorhTests
{
    public class Enabled
    {
        [Fact]
        public void Expect_BLP4007_when_authors_is_explicitly_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4007.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <Authors>John Doe</Authors>
                  </PropertyGroup>

                  <ItemGroup>
                    <Author Include="John Doe" />
                  </ItemGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Warn("BLP4007", "Authors is explicitly defined while authors generation is enabled. Remove explicitly defined authors, disable Autorh(set $(DisableAutorh) to true), or uninstall Packinf to manually define authors.", ThisPackage.GetPath("buildMultiTargeting", "Autorh.targets"), ((22, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }

        [Fact]
        public void Expect_CopyrightOwner_to_be_included_in_authors()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "CopyrightOwners.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                  </PropertyGroup>

                  <ItemGroup>
                    <Author Include="Jane Doe" />
                    <CopyrightOwner Include="John Doe" Years="2024" />
                  </ItemGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEmpty();

            result.Items["Author"].Should().BeEquivalentTo([
                MSBuildItem.Create("Jane Doe"),
                MSBuildItem.Create("John Doe", new Dictionary<string, string> {
                    { "Years", "2024"},
                    { "ItemSource", "CopyrightOwner" },
                }),
            ]);
            result.Items["CopyrightOwner"].Should().BeEquivalentTo([
                MSBuildItem.Create("John Doe", new Dictionary<string, string> {
                    { "Years", "2024"},
                }),
            ]);
        }

        [Fact]
        public void Expect_CopyrightOwner_to_not_be_included_in_authors_if_IncludeCopyrightOwnersInAuthors_is_false()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "CopyrightOwnersNotIncluded.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <IncludeCopyrightOwnersInAuthors>false</IncludeCopyrightOwnersInAuthors>
                  </PropertyGroup>

                  <ItemGroup>
                    <Author Include="Jane Doe" />
                    <CopyrightOwner Include="John Doe" Years="2024" />
                  </ItemGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEmpty();

            result.Items["Author"].Should().BeEquivalentTo([
                MSBuildItem.Create("Jane Doe"),
            ]);
            result.Items["CopyrightOwner"].Should().BeEquivalentTo([
                MSBuildItem.Create("John Doe", new Dictionary<string, string> {
                    { "Years", "2024"},
                }),
            ]);
        }

        [Fact]
        public void Expect_BLP4009_when_no_authors_are_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4009.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Failure);

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Error("BLP4009", "No authors defined.", ThisPackage.GetPath("buildMultiTargeting", "Autorh.targets"), ((23, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }

        [Fact]
        public void Expect_BLP4009_when_no_authors_are_defined_while_nonX2Dzero_copyright_owners_are_defined_and_IncludeCopyrightOwnersInAuthors_is_false()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4009.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <IncludeCopyrightOwnersInAuthors>false</IncludeCopyrightOwnersInAuthors>
                  </PropertyGroup>

                  <ItemGroup>
                    <CopyrightOwner Include="John Doe" Years="2024" />
                  </ItemGroup>

                </Project>

                """
            );
            MSBuildResult result = project.Build();

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Failure);

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Error("BLP4009", "No authors defined.", ThisPackage.GetPath("buildMultiTargeting", "Autorh.targets"), ((23, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }
    }

    public class Disabled
    {
        [Fact]
        public void Expect_BLP4008_when_authors_is_not_defined()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4008.csproj",
                """
                <Project>

                  <Import Sdk="Microsoft.NET.Sdk" Project="Sdk.props" />

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <DisableAutorh>true</DisableAutorh>
                  </PropertyGroup>

                  <Import Sdk="Microsoft.NET.Sdk" Project="Sdk.targets" />

                  <PropertyGroup>
                    <Authors></Authors>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Properties["Authors"].Should().BeEmpty();

            result.Diagnostics.Should().BeEquivalentTo([
                Diagnostic.Warn("BLP4008", "Authors is not defined and authors generation is disabled. Enable Autorh(set $(DisableAutorh) to false), explicitly define the authors, or disable required authors(set $(RequireAuthors) to false).", ThisPackage.GetPath("buildMultiTargeting", "Autorh.targets"), ((27, 5), (0, 0)), project.MSBuildProject.FullPath)
            ]);
        }

        [Fact]
        public void Do_not_expect_BLP4008_when_authors_is_not_defined_while_required_authors_is_disabled()
        {
            TestProjectInstance project = MSBuildTest.Load.Project.From.String(
                "BLP4008.csproj",
                """
                <Project>

                  <Import Sdk="Microsoft.NET.Sdk" Project="Sdk.props" />

                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <DisableAutoCopyright>true</DisableAutoCopyright>
                    <RequireCopyrightNotice>false</RequireCopyrightNotice>
                    <DisableAutorh>true</DisableAutorh>
                    <RequireAuthors>false</RequireAuthors>
                  </PropertyGroup>

                  <Import Sdk="Microsoft.NET.Sdk" Project="Sdk.targets" />

                  <PropertyGroup>
                    <Authors></Authors>
                  </PropertyGroup>

                </Project>
                """
            );
            MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

            using var scope = new AssertionScope();

            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Properties["Authors"].Should().BeEmpty();

            result.Diagnostics.Should().BeEmpty();
        }
    }

    [Fact]
    public void Expect_authors_is_generated_in_proper_format()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "BLP4009.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <DisableAutoCopyright>true</DisableAutoCopyright>
                <RequireCopyrightNotice>false</RequireCopyrightNotice>
              </PropertyGroup>

              <ItemGroup>
                <Author Include="John Bloggs" />
                <CopyrightOwner Include="John Doe" Years="2024" />
                <CopyrightOwner Include="Jane Doe" Years="2023, 2024" />
              </ItemGroup>

              <Target Name="ForceEvaluate" AfterTargets="Build">
                <PropertyGroup>
                  <Authors>$(Authors)</Authors>
                  <Company>$(Company)</Company>
                </PropertyGroup>
              </Target>

            </Project>
            """
        );
        MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEmpty();

        result.Properties["AuthorsGenerated"].Should().Be("true");
        result.Properties["Authors"].Should().Be("John Bloggs, John Doe, Jane Doe");
        result.Properties["Company"].Should().Be(result.Properties["Authors"]);

        result.Items["Author"].Should().BeEquivalentTo([
            MSBuildItem.Create("John Bloggs"),
            MSBuildItem.Create("John Doe", new Dictionary<string, string>() {
                { "Years", "2024" },
                { "ItemSource", "CopyrightOwner" },
            }),
            MSBuildItem.Create("Jane Doe", new Dictionary<string, string>() {
                { "Years", "2023, 2024" },
                { "ItemSource", "CopyrightOwner" },
            }),
        ]);
        result.Items["CopyrightOwner"].Should().BeEquivalentTo([
            MSBuildItem.Create("John Doe", new Dictionary<string, string>() {
                { "Years", "2024" },
            }),
            MSBuildItem.Create("Jane Doe", new Dictionary<string, string>() {
                { "Years", "2023, 2024" },
            }),
        ]);
    }
}
