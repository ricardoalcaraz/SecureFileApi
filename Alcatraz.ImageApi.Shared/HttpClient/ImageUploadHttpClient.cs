using System.Net.Http.Json;
using ImageRpcApi;
using Microsoft.Extensions.Logging;

namespace Alcatraz.ImageApi.Shared.HttpClient;

/// <summary>
/// Typed http client for image upload. Normally these would be generated from openapi but image uploading
/// requires us to get dirty with the stream to make the transfer as efficient as possible
/// </summary>
public class ImageUploadHttpClient
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly ILogger<ImageUploadHttpClient> _logger;

    public ImageUploadHttpClient(System.Net.Http.HttpClient httpClient, ILogger<ImageUploadHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // public async Task<ImageUploadResponse?> UploadImage(ImageUploadRequest uploadRequest, CancellationToken ctx)
    // {
    //     _logger.LogDebug("UploadRequest: {Request}", uploadRequest);
    //     _logger.LogInformation("{ImageId} - image upload starting-----------", uploadRequest.ImageId);
    //     
    //     var multipartContent = new MultipartFormDataContent();
    //     multipartContent.Add(new StreamContent(uploadRequest.File!));
    //     multipartContent.Add(new StringContent(uploadRequest.Description!));
    //     
    //     var uploadResponse = await _httpClient.PostAsync("images/single", multipartContent, ctx);
    //     
    //     uploadResponse.EnsureSuccessStatusCode();
    //     var imageUploadResponse = await uploadResponse.Content.ReadFromJsonAsync<ImageUploadResponse>(cancellationToken: ctx);
    //     if (imageUploadResponse is null)
    //     {
    //         _logger.LogWarning("Received null for request ");
    //     }
    //     else
    //     {
    //         _logger.LogInformation("{ImageId} - finished--------------------", uploadRequest.ImageId);
    //     }
    //     return imageUploadResponse;
    // }

    public async Task<ImageDetailResponse?> GetImageDetails(ImageDetailRequest imageDetailRequest, CancellationToken ctx)
    {
        var requestUri = $"images/{imageDetailRequest.imageId}";

        _logger.LogDebug("ImageDetailRequest: {Request}", imageDetailRequest);
        var response = await _httpClient.GetFromJsonAsync<ImageDetailResponse>(requestUri, cancellationToken: ctx);
        
        _logger.LogDebug("ImageDetailResponse: {Response}", response);
        
        return response;
    }
}

