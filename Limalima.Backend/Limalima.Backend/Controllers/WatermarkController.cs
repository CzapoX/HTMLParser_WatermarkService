using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Limalima.Backend.Controllers
{
    public class WatermarkController : Controller
    {
        private readonly IImageValidator imageValidator;

        public WatermarkController(IImageValidator imageValidator)
        {
            this.imageValidator = imageValidator;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {

            long size = 0;
            int count = 0;

            string folderPath = Directory.GetCurrentDirectory() + "/Images/Temp";
            if (!System.IO.Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var formFile in files)
            {
                if (formFile.Length > 0 && imageValidator.ValidateFile(formFile))
                {
                    var fileName = Path.GetRandomFileName();
                    var filePath = Path.Combine(folderPath,
                         fileName);

                    using (Stream stream = formFile.OpenReadStream())
                    {
                        using (var image = new MagickImage(stream))
                        {
                            using (var watermark = new MagickImage(Directory.GetCurrentDirectory() + "/Images/logo.png"))
                            {
                                // Draw the watermark in the bottom right corner
                                image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);
                            }
                            image.Write(filePath);
                        }
                    }
                    count++;
                    size += formFile.Length;
                }
            }
            return Ok(new { count , size });
        }
    }
}


