using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Alcatraz.ImageApi.Shared.Validation;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _minSizeInBytes;
    private readonly long _maxSizeInBytes;

    public MaxFileSizeAttribute(int maxSizeInMb, int minSizeInBytes)
    {
        _minSizeInBytes = minSizeInBytes;
        _maxSizeInBytes = 1024 * 1024 * maxSizeInMb;
        ErrorMessage = $"File cannot be larger than {maxSizeInMb}MB";
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxSizeInBytes)
            {
                return file.Length > _maxSizeInBytes ? new ValidationResult(ErrorMessage) : ValidationResult.Success ;
            }
            else if(file.Length < _minSizeInBytes)
            {
                return file.Length > _maxSizeInBytes ? new ValidationResult($"File cannot be smaller than {_minSizeInBytes}B") : ValidationResult.Success ;
            }
        }
        
        return ValidationResult.Success;
    }
}