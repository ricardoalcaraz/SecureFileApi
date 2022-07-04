using System.Buffers;
using Google.Protobuf;
using Grpc.Net.Client;
using ImageRpc;

namespace Alcatraz.ImageApi.Shared.GrpcClient;

public class ImageUploadGrpcClient
{
    public async Task<ImageUploadResponse> UploadImage(Stream imageStream, CancellationToken ctx)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7042");
        var client = new ImageUploader.ImageUploaderClient(channel);
        var streamingCall = client.UploadImage(cancellationToken: ctx);
        
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 32);

        while (imageStream.Position < imageStream.Length)
        {
            var bytesRead = await imageStream.ReadAsync(buffer, ctx);
            
            await streamingCall.RequestStream.WriteAsync(new ImageUploadRequest
            {
                DataPacket = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, bytesRead))
            }, ctx);
        }
        ArrayPool<byte>.Shared.Return(buffer);
        await streamingCall.RequestStream.CompleteAsync();

        return await streamingCall.ResponseAsync;
    }

}