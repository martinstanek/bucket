using System.Threading.Tasks;

namespace Bucket.Service.Services;

public interface IDockerService
{
    Task<string> GetVersionAsync();
    Task<string> PullImageAsync(string fullImageName);
    Task ExportImageAsync(string fullImageName, string outputFile);
}