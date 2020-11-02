using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limalima.Backend.Controllers
{
    public class WatermarkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            string folderPath = Directory.GetCurrentDirectory() + "/Images/Temp";
            if (!System.IO.Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var fileName = Path.GetRandomFileName();
                    var filePath = Path.Combine(folderPath,
                         fileName);

                    using (Stream stream = formFile.OpenReadStream())
                    {
                        using (var image = new MagickImage(stream))
                        {
                            using (var watermark = new MagickImage(@"D:\praktyki\limalima-up2020\Limalima.Backend\Limalima.Backend\Images\logo.png"))
                            { 
                               // Draw the watermark in the bottom right corner
                               image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);
                            }
                            image.Write(filePath);
                        }
                    }
                }
            }
            return Ok(new { count = files.Count, size });
        }
    }
}


