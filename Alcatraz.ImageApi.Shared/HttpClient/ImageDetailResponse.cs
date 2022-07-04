namespace Alcatraz.ImageApi.Shared.HttpClient;

public record ImageDetailResponse(Guid imageId, Uri Location, string Hash);