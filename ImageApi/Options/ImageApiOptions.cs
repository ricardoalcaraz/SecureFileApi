using System.ComponentModel.DataAnnotations;

namespace ImageApi.Options;

public record ImageApiOptions
{
    [Required] public Uri? SaveLocation { get; init; }
    public const string CONFIG_NAME = "ApiOptions";
}