using System.Buffers;
using System.Net.Http.Json;
using Alcatraz.ImageApi.Shared.Models;
using Blake2Fast;
using Google.Protobuf;
using Grpc.Net.Client;
using ImageApi.Data;
using ImageRpc;
using SixLabors.ImageSharp;

namespace ImageApiIntegrationTests;

[TestClass]
public class UnitTest1 : BaseIntegrationTest
{
    [TestMethod]
    public async Task UploadImageThroughFrom()
    {
        var memoryStream = new MemoryStream();
        Image.Load(FakePerson1Picture).SaveAsWebp(memoryStream);
        var hash = Blake2b.ComputeHash(memoryStream.GetBuffer().AsSpan());
        var formContent = new MultipartFormDataContent();
        var streamContent = new StreamContent(FakePerson1Picture);
        formContent.Add(streamContent, "file", "FakePerson1");
        formContent.Add(new StringContent("This is a test"), "Description");
        
        var response = await HttpClient.PostAsync("/Images/single", formContent);
        var uploadResponse = await response.Content.ReadFromJsonAsync<FileUploadResponse>();
        
        Assert.IsNotNull(uploadResponse);
        Assert.AreEqual(Convert.ToBase64String(hash), uploadResponse.Hash);
        Assert.AreNotEqual(default, uploadResponse.ImageId);
    }
    
    [TestMethod]
    public async Task TestMethod2()
    {
        var formContent = new MultipartFormDataContent();
        var file = File.Open(@"/Users/ralcaraz/Pictures/UploadedPeople/0ee48a5a-3a13-4191-a4f3-f5939e8b519d.webp", FileMode.Open);
        var streamContent = new StreamContent(file);
        formContent.Add(streamContent, "image", file.Name);
        
        var response = await HttpClient.PostAsync("/Images", formContent);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
    }
    
    [TestMethod]
    public async Task GetImage()
    {
        var formContent = new MultipartFormDataContent();
        var file = File.Open(@"/Users/ralcaraz/Pictures/UploadedPeople/0ee48a5a-3a13-4191-a4f3-f5939e8b519d.webp", FileMode.Open);
        var streamContent = new StreamContent(file);
        formContent.Add(streamContent, "image", file.Name);
        
        var response = await HttpClient.GetAsync($"Images/{Guid.NewGuid()}");
        
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task GetWeather()
    {
        var forecast = await HttpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>("http://localhost/WeatherForecast");
        
        Assert.IsTrue(forecast.Any());
    }

    [TestMethod]
    public async Task UseGrpc()
    {
        var channel = GrpcChannel.ForAddress(HttpClient.BaseAddress!);
        var uploadCall = new ImageUploader.ImageUploaderClient(channel).UploadImage();
        
        var imageStream = ImageStream(4096, CancellationToken.None);
        await foreach (var image in ImageStream(1024 * 32, CancellationToken.None))
        {
            await uploadCall.RequestStream.WriteAsync(image);
        }

        await uploadCall.RequestStream.CompleteAsync();
        var response = await uploadCall.ResponseAsync;
        
        Assert.IsTrue(response.HasHash);
        Assert.IsTrue(response.HasImageId);
        var responseHash = response.Hash;
        Assert.AreEqual(FakePerson1Hash, responseHash);
    }

    private async IAsyncEnumerable<ImageUploadRequest> ImageStream(int packetSize, CancellationToken ctx)
    {
        var fileStream = File.Open(@"/Users/ralcaraz/Pictures/UploadedPeople/0ee48a5a-3a13-4191-a4f3-f5939e8b519d.webp", FileMode.Open);
        var buffer = ArrayPool<byte>.Shared.Rent(packetSize);
        var readBytes = 0;
        do
        {
            readBytes = await fileStream.ReadAsync(buffer, ctx);
            var data = buffer.AsMemory(0, readBytes);
            yield return new ImageUploadRequest
            {
                DataPacket = ByteString.CopyFrom(data.Span),
                FileSize = (int)fileStream.Length,
                Hash = Blake2b.ComputeHash(data.Span).ToString()
            };
        } while (readBytes > 0);
    }
}