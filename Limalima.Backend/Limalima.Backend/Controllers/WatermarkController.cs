using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using Limalima.Backend.Models;
using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Limalima.Backend.Controllers
{
    public class WatermarkController : Controller
    {
        private readonly IImageValidator imageValidator;

        private static readonly string tempImagesFolder = Path.Combine(Path.GetTempPath(), "FrutilsImagesTemp");
        private static readonly string watermarkedImagesFolderPath = Directory.GetCurrentDirectory() + "/Images/Temp";

        static WatermarkController()
        {
            if (!Directory.Exists(tempImagesFolder))
            {
                Directory.CreateDirectory(tempImagesFolder);
            }
            if (!Directory.Exists(watermarkedImagesFolderPath))
            {
                Directory.CreateDirectory(watermarkedImagesFolderPath);
            }
        }

        public WatermarkController(IImageValidator imageValidator)
        {
            this.imageValidator = imageValidator;
        }

        public IActionResult Index()
        {
            return View(new AnnouceViewModel { ImageTempId = Guid.NewGuid() });
        }

        [HttpPost]
        public IActionResult AddNew(AnnouceViewModel model)
        {
            var files = Directory.GetFiles(tempImagesFolder, model.ImageTempId + "*");

            foreach (var fileDirectory in files)
            {
                using (var image = new MagickImage(fileDirectory))
                {
                    var fileName = Guid.NewGuid() + "." + Path.GetExtension(fileDirectory);
                    var filePath = Path.Combine(watermarkedImagesFolderPath,
                         fileName);

                    using (var watermark = new MagickImage(Directory.GetCurrentDirectory() + "/Images/logo.png"))
                    {
                        // Draw the watermark in the bottom right corner
                        image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);
                    }
                    image.Write(filePath);
                }
            }
            return Redirect("/");
        }

        //wywolywane przez jquery ajax
        [HttpPost]
        public async Task<IActionResult> UploadAjax(FileUploadViewModel model)
        {
            if (model.File == null)
                return BadRequest();
            if (!imageValidator.ValidateFile(model.File))
                return BadRequest();

            //zapis w temp pod nazwa pliku model.ImageTempId+"_xxx_+".jpg
            string filename = model.ImageTempId + "_" + Guid.NewGuid() + "." + Path.GetExtension(model.File.FileName);
            string tempFilename = Path.Combine(tempImagesFolder, filename);

            using (var stream = System.IO.File.Create(tempFilename))
            {
                await model.File.CopyToAsync(stream);
            }
            return Ok();
        }
    }
}