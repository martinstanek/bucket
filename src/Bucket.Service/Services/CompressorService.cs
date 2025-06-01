using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Bucket.Service.Model;

namespace Bucket.Service.Services;

public sealed class CompressorService : ICompressorService
{
    public async Task<string> PackBundleAsync(
        BundleManifest bundleDefinition,
        string bundleDirectory,
        string outputDirectory,
        string extension,
        CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(bundleDirectory);
        Guard.Against.NullOrWhiteSpace(outputDirectory);
        Guard.Against.NullOrWhiteSpace(extension);

        if (!Directory.Exists(bundleDirectory))
        {
            throw new DirectoryNotFoundException(bundleDirectory);
        }

        var bundleName = $"{bundleDefinition.Info.Name}{extension}";
        var bundlePath = Path.Combine(outputDirectory, bundleName);

        await using var fs = new FileStream(bundlePath, FileMode.CreateNew, FileAccess.Write);
        await using var gz = new GZipStream(fs, CompressionMode.Compress, leaveOpen: true);
        await using var tar = new TarWriter(gz, leaveOpen: false);

        foreach (var file in Directory.EnumerateFiles(bundleDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(bundleDirectory, file);
            await tar.WriteEntryAsync(file, relativePath, cancellationToken);
        }

        return bundlePath;
    }

    public async Task UnpackBundleAsync(
        string bundlePath,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(bundlePath);
        Guard.Against.NullOrWhiteSpace(outputDirectory);

        if (!File.Exists(bundlePath))
        {
            throw new FileNotFoundException(bundlePath);
        }

        var tempFile = Path.Combine(outputDirectory, Guid.NewGuid().ToString());

        await using var inputStream = File.OpenRead(bundlePath);
        await using (var outputFileStream = File.Create(tempFile))
        {
            await using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);

            await gzipStream.CopyToAsync(outputFileStream, cancellationToken);

            outputFileStream.Seek(0, SeekOrigin.Begin);

            await TarFile.ExtractToDirectoryAsync(
                outputFileStream,
                outputDirectory,
                overwriteFiles: true,
                cancellationToken: cancellationToken
            );
        }

        File.Delete(tempFile);
    }
}