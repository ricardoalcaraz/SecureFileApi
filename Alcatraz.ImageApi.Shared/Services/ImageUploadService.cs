using Blake2Fast;
using Grpc.Core;
using ImageRpc;
using SixLabors.ImageSharp;

namespace Alcatraz.ImageApi.Shared.Services;

public class ImageUploadService : ImageUploader.ImageUploaderBase
{
    public override async Task<ImageUploadResponse> UploadImage(IAsyncStreamReader<ImageUploadRequest> requestStream, ServerCallContext context)
    {
        var ctx = context.CancellationToken;
        var memoryStream = new MemoryStream();
        var hasher = Blake2b.CreateIncrementalHasher(digestLength: 64);
        var loadImageTask = Image.LoadAsync(memoryStream, ctx);
        
        await foreach (var request in requestStream.ReadAllAsync(ctx).WithCancellation(ctx))
        {
            var data = request.DataPacket.Memory;
            hasher.Update(data.Span);
            await memoryStream.WriteAsync(data, ctx);
            if (memoryStream.Length > 1024 * 1024 * 8)
            {
                return new ImageUploadResponse();
            }
        }

        if (await loadImageTask is IImage image)
        {
            return new ImageUploadResponse()
            {
                ImageId = Guid.NewGuid().ToString(),
                Hash = Convert.ToBase64String(hasher.Finish())
            };
        }
        
        return new ImageUploadResponse();
    }
}