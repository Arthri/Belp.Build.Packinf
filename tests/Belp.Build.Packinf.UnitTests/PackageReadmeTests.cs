using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace Belp.Build.Packinf.UnitTests;

public class PackageReadmeTests
{
    private static ZipArchive GetNupkgFromBuildResult(MSBuildResult result)
    {
        result.Properties["PackageOutputAbsolutePath"].Should().NotBeNullOrEmpty();
        string outputPackage = result.Items["NuGetPackOutput"].Should().ContainSingle(s => Path.GetExtension(s.Identity) == ".nupkg").Which.Identity;

        var packageStream = new FileStream(outputPackage, FileMode.Open, FileAccess.Read, FileShare.Read);
        var archive = new ZipArchive(packageStream);

        return archive;
    }

    private static void AssertNuspecHasREADME(ZipArchive archive, string readme = "README.md")
    {
        ZipArchiveEntry nuspecFile = archive.Entries.Single(e => Path.GetExtension(e.Name) == ".nuspec");
        using Stream nuspecStream = nuspecFile.Open();
        using var nuspecReader = XmlReader.Create(nuspecStream);
        var nuspec = XDocument.Load(nuspecReader);
        XNamespace defaultNamespace = nuspec.Root!.GetDefaultNamespace();
        XName GetXName(string localName)
        {
            return defaultNamespace + localName;
        }

        nuspec.Should()
            .HaveRoot(GetXName("package"))
            .And.HaveElement(GetXName("metadata"))
            .Which.Should().HaveElement(GetXName("readme"))
            .Which.Should().HaveValue(readme)
            ;
    }

    private static void AssertNupkgHasREADMEWithValue(ZipArchive archive, string expectedREADMEContents, string readmeFilename = "README.md")
    {
        archive.Entries.Should().ContainSingle(e => e.Name == readmeFilename);
        ZipArchiveEntry readmeFile = archive.Entries.Single(e => e.Name == readmeFilename);
        using Stream readmeStream = readmeFile.Open();
        using var readmeReader = new StreamReader(readmeStream);
        string readme = readmeReader.ReadToEnd();
        readme.Should().Be(expectedREADMEContents);
    }

    [Fact]
    public void PackageREADMEX2Emd_beside_project_should_be_considered()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.Samples("PackageREADME");
        MSBuildResult result = project.Pack(
            BuildRequestDataFlags.ProvideProjectStateAfterBuild
        );

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEmpty();

        result.Properties.Should().ContainKey("PackageReadmeFile").WhoseValue.Should().Be("README.md");

        using ZipArchive archive = GetNupkgFromBuildResult(result);

        AssertNuspecHasREADME(archive);

        AssertNupkgHasREADMEWithValue(archive, File.ReadAllText(Path.Combine(project.MSBuildProject.DirectoryPath, "PackageREADME.md")));
    }

    [Fact]
    public void READMEX2Emd_beside_project_should_be_considered()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.Samples("README");
        MSBuildResult result = project.Pack(
            BuildRequestDataFlags.ProvideProjectStateAfterBuild
        );

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEmpty();

        result.Properties.Should().ContainKey("PackageReadmeFile").WhoseValue.Should().Be("README.md");

        using ZipArchive archive = GetNupkgFromBuildResult(result);

        AssertNuspecHasREADME(archive);

        AssertNupkgHasREADMEWithValue(archive, File.ReadAllText(Path.Combine(project.MSBuildProject.DirectoryPath, "README.md")));
    }

    [Fact]
#pragma warning disable IDE1006 // Naming Styles
    public async Task READMEX2Emd_in_source_root_should_be_considered()
#pragma warning restore IDE1006 // Naming Styles
    {
        TestSampleInstance sample = MSBuildTest.Load.Sample("GitREADME");
        FileTestProject.Instance project = sample.DefaultProject;
        Process p_gitInit = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            ArgumentList = { "init" },
            WorkingDirectory = sample.Directory,
        })!;
        await p_gitInit.WaitForExitAsync();
        p_gitInit.ExitCode.Should().Be(0);
        Process p_gitCommit = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            ArgumentList = { "-c", "user.name=\"John Doe\"", "-c", "user.email=\"john.doe@example.com\"", "commit", "--allow-empty", "--only", "-m", "Initial Commit" },
            WorkingDirectory = sample.Directory,
        })!;
        await p_gitCommit.WaitForExitAsync();
        p_gitCommit.ExitCode.Should().Be(0);
        MSBuildResult result = project.Pack(
            BuildRequestDataFlags.ProvideProjectStateAfterBuild
        );

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEmpty();

        result.Properties.Should().ContainKey("PackageReadmeFile").WhoseValue.Should().Be("README.md");

        using ZipArchive archive = GetNupkgFromBuildResult(result);

        AssertNuspecHasREADME(archive);

        AssertNupkgHasREADMEWithValue(archive, File.ReadAllText(Path.Combine(sample.Directory, "README.md")));
    }

    [Fact]
    public void Expect_BLP4006_when_multiple_READMEs_found()
    {
        TestProjectInstance project = MSBuildTest.Load.Project.From.String(
            "TestProject.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

              <ItemGroup>
                <CopyrightOwner Include="John Doe" Years="2021" />
                <PackageReadme Include="A.md" />
                <PackageReadme Include="B.md" />
              </ItemGroup>

            </Project>

            """
        );
        MSBuildResult result = project.Build();

        using var scope = new AssertionScope();

        result.OverallResult.Should().Be(BuildResultCode.Success);

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Warn("BLP4006", "Multiple candidate package READMEs found: A.md, B.md.", ThisPackage.GetPath("buildMultiTargeting", "SetPackageReadme.targets"), ((5, 5), (0, 0)), project.MSBuildProject.FullPath),
        ]);
    }
}
