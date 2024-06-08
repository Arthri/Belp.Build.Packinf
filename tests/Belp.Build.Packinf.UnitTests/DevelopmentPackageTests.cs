using System.Security.Cryptography;

namespace Belp.Build.Packinf.UnitTests;

public class DevelopmentPackageTests
{
    [Fact]
    public void Expect_package_to_be_pushed_and_cache_to_be_cleared()
    {
        TestSampleInstance sample = MSBuildTest.Load.Sample("PushPackage");
        Directory.CreateDirectory(Path.Combine(sample.Directory, "bin", "packages"));

        {
            FileTestProject.Instance project = sample.DefaultProject;
            MSBuildResult result = project.Pack(
                configureProjectInstance: project => project.SetProperty("PushPackageToDevelopmentSource", "true")
            );

            using var scope = new AssertionScope();
            result.OverallResult.Should().Be(BuildResultCode.Success);
            result.Diagnostics.Should().BeEmpty();

            AssertPackagesEqual();
        }

        {
            FileTestProject.Instance project = sample.Projects.First(p => p.TestProject.Name == "PackageUsage.csproj");
            MSBuildResult result = project.Build(BuildRequestDataFlags.ProvideProjectStateAfterBuild);

            using var scope = new AssertionScope();
            result.OverallResult.Should().Be(BuildResultCode.Success);
            result.Diagnostics.Should().BeEmpty();

            string packageCache = result.Properties
                .Should().ContainKey("NuGetPackageRoot")
                .WhoseValue.Should().NotBeNullOrEmpty()
                .And.Subject
                ;
            Directory.Exists(Path.Combine(packageCache, "PushPackage", "1.0.0")).Should().BeTrue("Package should be restored and cached in global package cache");
        }

        {
            FileTestProject.Instance project = sample.DefaultProject;
            MSBuildResult result = project.Pack(
                BuildRequestDataFlags.ProvideProjectStateAfterBuild,
                configureProjectInstance: project => project.SetProperty("PushPackageToDevelopmentSource", "true")
            );

            using var scope = new AssertionScope();
            result.OverallResult.Should().Be(BuildResultCode.Success);
            result.Diagnostics.Should().BeEmpty();

            string packageCache = result.Properties
                .Should().ContainKey("NuGetPackageRoot")
                .WhoseValue.Should().NotBeNullOrEmpty()
                .And.Subject
                ;
            Directory.Exists(Path.Combine(packageCache, "PushPackage", "1.0.0")).Should().BeFalse("Package should be wiped from global package cache");

            AssertPackagesEqual();
        }

        void AssertPackagesEqual()
        {
            Span<byte> hashOriginal = stackalloc byte[64];
            Span<byte> hashNew = stackalloc byte[64];

            GetHash("PushPackage/bin/Debug/PushPackage.1.0.0.nupkg", hashOriginal);
            GetHash("bin/packages/PushPackage.1.0.0.nupkg", hashNew);

            void GetHash(string filePath, scoped Span<byte> output)
            {
                string path = Path.Combine(sample.Directory, filePath);
                File.Exists(path).Should().BeTrue($"File {path} should exist");
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                SHA512.HashData(stream, output);
            }

            hashOriginal.SequenceEqual(hashNew).Should().BeTrue("NuGet package in bin should be equal to pushed NuGet package");
        }
    }
}
