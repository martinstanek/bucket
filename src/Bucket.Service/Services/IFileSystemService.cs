namespace Bucket.Service.Services;

public interface IFileSystemService
{
    void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true);
}