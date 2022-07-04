using System.Buffers;
using Alcatraz.ImageApi.Shared.Models;
using Blake2Fast;
using ImageApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace ImageApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private readonly ImageDbContext _dbContext;
    private readonly ILogger<ImagesController> _logger;
    private readonly ImageApiOptions _imageApiOptions;
    private readonly Uri _fileSaveLocation;

    public ImagesController(IOptionsMonitor<ImageApiOptions> optionsMonitor, ImageDbContext dbContext, ILogger<ImagesController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _imageApiOptions = optionsMonitor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _fileSaveLocation = optionsMonitor.CurrentValue.SaveLocation!;
    }

    [HttpPost("single")]
    public async Task<IActionResult> UploadImages([FromForm]FileUploadRequest fileUpload, CancellationToken ctx)
    {
        _logger.LogInformation("{FileUpload} - Starting", fileUpload);
        
        var userId = Guid.Empty;
        
        _logger.LogDebug("Creating in memory array of {Length} size", fileUpload.File.Length);
        
        await using var readStream = fileUpload.File.OpenReadStream();

        var (image, format) = await Image.LoadWithFormatAsync(readStream, ctx);

        if (image is null)
        {
            return BadRequest("Uploaded file is not a supported image type");
        }
        _logger.LogInformation("{FileUpload} - Supported Image {ImageFormat}", fileUpload, format);

        var memoryStream = new MemoryStream();
        
        image.SaveAsWebp(memoryStream);
        var imageBuffer = memoryStream.GetBuffer();
        
        var digestLength = 64;
        var hash = Blake2b.ComputeHash(digestLength, imageBuffer.AsSpan());

        var imageId = Guid.NewGuid();
        var savePath = new Uri($"{_fileSaveLocation}/{imageId}.webp");
        var imageInfo = new ImageInfo
        {
            FileLocation = savePath,
            Id = imageId,
            UploaderId = userId,
            Hash = hash
        };

        await using var fileStream = new FileStream(imageInfo.FileLocation.AbsolutePath, FileMode.CreateNew);
        await fileStream.WriteAsync(imageBuffer, ctx);
        
        _dbContext.Images.Add(imageInfo);
        await _dbContext.SaveChangesAsync(ctx);
        var fileUploadResponse = new FileUploadResponse(imageInfo.Id, Convert.ToBase64String(imageInfo.Hash));
        
        return CreatedAtAction(nameof(GetImageById), new { id = imageId }, fileUploadResponse);
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm]IFormFile image, CancellationToken ctx)
    {
        var userId = Guid.Empty;
        var imageId = Guid.NewGuid();
        
        //to do map user claim to database
        var filePath = new Uri($"{_imageApiOptions.SaveLocation}/{userId}/{imageId}.webp");
        await using var readStream = image.OpenReadStream();
        var uploadedImage = await Image.LoadAsync(readStream, ctx);

        if (uploadedImage is null)
        {
            return BadRequest("Please upload an image");
        }
        
        var imageInfo = new ImageInfo()
        {
            FileLocation = filePath,
            Id = imageId,
            UploaderId = userId
        };
        _dbContext.Images.Add(imageInfo);
        
        await using var fileStream = new FileStream(filePath.ToString(), FileMode.CreateNew);
        await uploadedImage.SaveAsWebpAsync(fileStream, ctx);
        await _dbContext.SaveChangesAsync(ctx);
        return CreatedAtAction(nameof(GetImageById), new { id = imageId }, new { id = imageId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImageById(Guid id)
    {
        //read file path from db and return a file stream for desired file
        var filePath = await _dbContext.Images.SingleAsync(i => i.Id == id);
        //upload to redis if not available
        return File(filePath.FileLocation.ToString(), "image/webp");
    }
}