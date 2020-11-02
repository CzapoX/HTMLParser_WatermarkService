using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Limalima.Backend.Validation
{
    public interface IImageValidator
    {
       bool ValidateFile(IFormFile formFile);
    }
}
