using Belp.Build.Testing;
using Belp.Build.Testing.Resources;
using FluentAssertions;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Xunit;

namespace Belp.Build.Packinf.UnitTests;

public partial class PackEquivalencyTests
{
    [GeneratedRegex("""
                    ^  <Relationship Type="http://schemas\.openxmlformats\.org/package/2006/relationships/metadata/core-properties" Target="/package/services/metadata/core-properties/[0-9a-f]{32}\.psmdcp" Id="[^"]+" />\r?$
                    """, RegexOptions.Multiline)]
    private static partial Regex PSMDCPRelationshipMatcher();

    // Everything in lib is ignored
    private static void AssertArchivesEqual(ZipArchive archive1, ZipArchive archive2, [CallerArgumentExpression(nameof(archive1))] string? archive1Expression = null, [CallerArgumentExpression(nameof(archive2))] string? archive2Expression = null)
    {
        archive1.Entries.Should().HaveCount(archive2.Entries.Count);

        Dictionary<string, ZipArchiveEntry> entriesA = ArchiveToDictionary(archive1);
        Dictionary<string, ZipArchiveEntry> entriesB = ArchiveToDictionary(archive2);

        foreach ((string fullName, ZipArchiveEntry entryA) in entriesA)
        {
            ZipArchiveEntry entryB = entriesB.Should().ContainKey(fullName).WhoseValue;
            HashEntry(entryA).Should().Be(HashEntry(entryB), $"{fullName} in {archive1Expression ?? "archive 1"} and {archive2Expression ?? "archive 2"} should be equal");
        }

        foreach ((string fullName, ZipArchiveEntry entryB) in entriesB)
        {
            entriesA.Should().ContainKey(fullName);
        }

        static string Hash(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[256 / 8];
            SHA256.HashData(stream, buffer);
            return Convert.ToHexString(buffer);
        }

        static string HashEntry(ZipArchiveEntry entry)
        {
            if (entry.FullName == "_rels/.rels")
            {
                using Stream stream = entry.Open();
                using var reader = new StreamReader(stream);
                string contents = PSMDCPRelationshipMatcher().Replace(reader.ReadToEnd(), """
                      <Relationship Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="/package/services/metadata/core-properties/$$$$PSMDCP$$$$" />
                    """);
                Span<byte> buffer = stackalloc byte[256 / 8];
                SHA256.HashData(MemoryMarshal.AsBytes(contents.AsSpan()), buffer);
                return Convert.ToHexString(buffer);
            }
            else
            {
                using Stream stream = entry.Open();
                return Hash(stream);
            }
        }

        static Dictionary<string, ZipArchiveEntry> ArchiveToDictionary(ZipArchive archive)
        {
            return archive.Entries.Where(e => !e.FullName.StartsWith("lib/")).ToDictionary(e => Path.GetExtension(e.Name) == ".psmdcp" ? "$$PSMDCP$$" : e.FullName);
        }
    }

    private static ZipArchive OpenNupkg(TestProjectInstance projectInstance)
    {
        return new ZipArchive(new FileStream(Path.Combine(projectInstance.MSBuildProject.DirectoryPath, "bin", "Debug", $"{Path.GetFileNameWithoutExtension(projectInstance.MSBuildProject.FullPath)}.1.0.0.nupkg"), FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    private static void AssertPacksEqual(string sample1, string sample2)
    {
        TestProjectInstance project1 = MSBuildTest.Load.Project.From.Samples(sample1);
        TestProjectInstance project2 = MSBuildTest.Load.Project.From.Samples(sample2);

        _ = project1.Pack();
        _ = project2.Pack();

        using ZipArchive archive1 = OpenNupkg(project1);
        using ZipArchive archive2 = OpenNupkg(project2);

        AssertArchivesEqual(archive1, archive2);
    }

    [Fact]
    public void Simple_assets_should_result_in_equal_package()
    {
        AssertPacksEqual("SimplePackinfPack", "SimpleNuGetPack");
    }

    [Fact]
    public void Content_assets_should_result_in_equal_package()
    {
        AssertPacksEqual("ContentPackinfPack", "ContentNuGetPack");
    }

    [Fact]
    public void Content_assets_should_result_in_equal_package_in_a_multitargeting_project()
    {
        AssertPacksEqual("ContentPackinfPackMultiTargeting", "ContentNuGetPackMultiTargeting");
    }

    [Fact]
    public void Packinf_should_substitute_Package_ID_and_result_in_equal_package()
    {
        AssertPacksEqual("PackageIdPackinfPack", "PackageIdNuGetPack");
    }

    [Fact]
    public void Packinf_should_substitute_Package_ID_and_result_in_equal_package_when_in_content()
    {
        AssertPacksEqual("ContentPackageIdPackinfPack", "ContentPackageIdNuGetPack");
    }

    [Fact]
    public void Packinf_should_substitute_Package_ID_and_result_in_equal_package_when_in_content_and_when_multitargeting()
    {
        AssertPacksEqual("ContentPackageIdPackinfPackMultiTargeting", "ContentPackageIdNuGetPackMultiTargeting");
    }
}
