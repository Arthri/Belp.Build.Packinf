using System.IO.Compression;

namespace Belp.Build.Packinf.UnitTests;

public class LibraryPlaceholderTests
{
    [Fact]
    public void Expect_X5FX2EX5F_placeholder_to_be_in_package_lib_folder()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "Project.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <AddMetapackagePlaceholderLibrary>true</AddMetapackagePlaceholderLibrary>
                <IncludeBuildOutput>false</IncludeBuildOutput>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

              <ItemGroup>
                <CopyrightOwner Include="John Doe" Years="2021" />
              </ItemGroup>

            </Project>
            
            """);
        MSBuildResult result = project.Pack(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

        using (var scope = new AssertionScope())
        {
            result.OverallResult.Should().Be(BuildResultCode.Success);

            result.Diagnostics.Should().BeEmpty();
        }


        MSBuildItem nupkgItem = result
            .Items.Should().ContainKey("NuGetPackOutput")
            .WhoseValue.Should().Contain(i => i.Identity.EndsWith(".nupkg"))
            .Which
            ;

        using var stream = new FileStream(nupkgItem.Identity, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var archive = new ZipArchive(stream);

        using (var scope = new AssertionScope())
        {
            archive.Entries.Should().Contain(e => e.FullName == "lib/net8.0/_._");
        }
    }
}
