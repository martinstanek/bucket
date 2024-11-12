using System.Threading.Tasks;

namespace Bucket.Service.Services;

public interface IDockerService
{
    Task<string> GetVersionAsync();
    
    Task<string> PullImageAsync(string fullImageName);
    
    Task ExportImageAsync(string fullImageName, string outputFile);
    
    Task SaveImageAsync(string fullImageName, string outputFile);
    
    Task ImportImageAsync(string fullImageName, string inputFile);

    Task LoadImageAsync(string inputFile);

    Task UpStackAsync(string composeFilePath);
}