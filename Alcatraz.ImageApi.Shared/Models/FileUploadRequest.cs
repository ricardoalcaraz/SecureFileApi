using System.ComponentModel.DataAnnotations;
using Alcatraz.ImageApi.Shared.Validation;
using Microsoft.AspNetCore.Http;

namespace Alcatraz.ImageApi.Shared.Models;

public record FileUploadRequest
{
    [MaxFileSize(8, 4096)]
    [Required]
    public IFormFile File { get; init; } = null!;
    
    [Required] 
    public string? Description { get; init; }
}