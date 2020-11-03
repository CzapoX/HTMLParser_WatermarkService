using Microsoft.AspNetCore.Http;

namespace Limalima.Backend.Validation
{
    public interface IImageValidator
    {
       bool ValidateFile(IFormFile formFile);
    }
}
