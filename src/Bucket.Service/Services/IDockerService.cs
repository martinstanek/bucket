using System.Threading;
using System.Threading.Tasks;

namespace Bucket.Service.Services;

public interface IDockerService
{
    Task<bool> IsDockerRunningAsync(CancellationToken cancellationToken);
    
    Task<string> GetVersionAsync(CancellationToken cancellationToken);
    
    Task<string> GetDockerStatsAsync(CancellationToken cancellationToken);
    
    Task<string> PullImageAsync(string fullImageName, CancellationToken cancellationToken);
    
    Task ExportImageAsync(string fullImageName, string outputFile, CancellationToken cancellationToken);
    
    Task SaveImageAsync(string fullImageName, string outputFile, CancellationToken cancellationToken);
    
    Task ImportImageAsync(string fullImageName, string inputFile, CancellationToken cancellationToken);
    
    Task LoadImageAsync(string inputFile, CancellationToken cancellationToken);
    
    Task UpStackAsync(string composeFilePath, CancellationToken cancellationToken);
}