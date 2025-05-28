using Bucket.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bucket.Service.Services;
using Xunit;
using System.Threading;
using Shouldly;

namespace Bucket.Tests.Service;

public class CompressorServiceTests
{
    [Fact]
    public async Task PackBundleAsync_CreatesArchiveWithAllFiles_IncludingHidden()
    {
        var extension = ".tar.gz";
        var hiddenFile = ".env";
        var visibleFile = "docker-compose.yml";
        var bundleManifest = BundleManifest.Empty with
        {
            Info = Info.Empty with
            {
                Name = "testBundle"
            }
        };

        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var bundleDir = Path.Combine(tempRoot, "bundle");
        var packedDir = Path.Combine(tempRoot, "packed");
        var unpackedDir = Path.Combine(tempRoot, "unpacked");
        var visibleFilePath = Path.Combine(bundleDir, visibleFile);
        var hiddenFilePath = Path.Combine(bundleDir, hiddenFile);
        var unpackedVisibleFilePath = Path.Combine(unpackedDir, visibleFile);
        var unpackedHiddenFilePath = Path.Combine(unpackedDir, hiddenFile);

        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(bundleDir);
        Directory.CreateDirectory(packedDir);
        Directory.CreateDirectory(unpackedDir);

        await File.WriteAllTextAsync(visibleFilePath, string.Empty);
        await File.WriteAllTextAsync(hiddenFilePath, string.Empty);

        var compressor = new CompressorService();

        var bundlePath = await compressor.PackBundleAsync(bundleManifest, bundleDir, packedDir, extension, cancellationToken: default);

        bundlePath.ShouldNotBeNullOrEmpty();
        File.Exists(bundlePath).ShouldBeTrue();

        await compressor.UnpackBundleAsync(bundlePath, unpackedDir, cancellationToken: default);

        File.Exists(unpackedVisibleFilePath).ShouldBeTrue();
        File.Exists(unpackedHiddenFilePath).ShouldBeTrue();

        Directory.Delete(tempRoot, recursive: true);
    }
}