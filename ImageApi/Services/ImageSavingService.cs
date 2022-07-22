using ImageApi.Data;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace ImageApi.Services;

/// <summary>
/// Manage saving and retrieving of files from the file system
/// </summary>
public class ImageSavingService
{
    private readonly ImageDbContext _dbContext;
    private readonly ILogger<ImageSavingService> _logger;
    private readonly ImageApiOptions _apiOptions;

    public ImageSavingService(ImageDbContext dbContext, 
        IOptions<ImageApiOptions> apiOptions,
        ILogger<ImageSavingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _apiOptions = apiOptions.Value;
    }
    
    public Stream ReadImage(ImageInfo imageInfo, CancellationToken ctx)
    {
        var fileInfo = new FileInfo(imageInfo.FileLocation.ToString());
        if (fileInfo.Exists)
        {
            _logger.LogDebug("Returning file from {Path}", fileInfo.FullName);
            return fileInfo.OpenRead();
        }
        
        _logger.LogWarning("File not found from {Path}", fileInfo.FullName);
        return Stream.Null;
    }

    public async Task<ImageInfo> SaveImage(Image image, Guid uploaderId, CancellationToken ctx)
    {
        var imageId = Guid.NewGuid();
        var fileInfo = new FileInfo($"{_apiOptions.SaveLocation}/{uploaderId}/{imageId}");
        if (fileInfo.Exists)
        {
            _logger.LogInformation("File currently exists, overwriting with new data");
            fileInfo.Delete();
        }
        
        await using var fileStream = fileInfo.Create();
        await image.SaveAsWebpAsync(fileStream, ctx);
        
        _logger.LogDebug("Saved file into {Path}", fileInfo.FullName);

        return new ImageInfo
        {
            Id = imageId,
            FileLocation = new Uri(fileInfo.FullName),
            UploaderId = uploaderId
        };
    }
}