using Bucket.Service.Model;
using Bucket.Service.Services;
using Shouldly;
using Xunit;

namespace Bucket.Tests.Service;

public sealed class CompressorServiceTests
{
    [Fact]
    public async Task PackUnpack_InputIsValid_AllFilesArePresent()
    {
        var compressor = new CompressorService();
        var validManifest = BundleManifest.TryParseFromPath("./Bundle/manifest.json", out var manifest);
        var tempDirectoryPath = $"./{DateTime.Now:yyyyMMddhhss}";
        var archivePath = Path.Combine(tempDirectoryPath, "bucket-test-bundle.tar.gz");

        Directory.CreateDirectory(tempDirectoryPath);

        validManifest.ShouldBeTrue();
        manifest.ShouldNotBeNull();
        Directory.Exists(tempDirectoryPath).ShouldBeTrue();

        await compressor.PackBundleAsync(manifest, "./Bundle", tempDirectoryPath, ".tar.gz", CancellationToken.None);
        await compressor.UnpackBundleAsync(archivePath, tempDirectoryPath, CancellationToken.None);

        File.Exists(Path.Combine(tempDirectoryPath, "backend", "docker-compose.yml")).ShouldBeTrue();
        File.Exists(Path.Combine(tempDirectoryPath, "backend", ".env")).ShouldBeTrue();
        File.Exists(Path.Combine(tempDirectoryPath, "proxy", "docker-compose.yml")).ShouldBeTrue();
        File.Exists(Path.Combine(tempDirectoryPath, "proxy", "config", "api-gateway.conf")).ShouldBeTrue();

        Directory.Delete(tempDirectoryPath, recursive: true);
    }
}