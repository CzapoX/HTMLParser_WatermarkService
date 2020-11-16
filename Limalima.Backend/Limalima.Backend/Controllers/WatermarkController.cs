using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Limalima.Backend.Components;
using Limalima.Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Limalima.Backend.Controllers
{
    public class WatermarkController : Controller
    {

        private readonly IWatermarkService _watermarkService;

        public WatermarkController(IWatermarkService watermarkService)
        {
            _watermarkService = watermarkService;
        }

        public IActionResult Index()
        {
            return View(new AnnouceViewModel { ImageTempId = Guid.NewGuid() });
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(AnnouceViewModel model)
        {
            string[] files = _watermarkService.GetFiles(model);

            foreach (var fileDirectory in files)
            {
                await _watermarkService.WatermarkImageAndUploadToAzure(fileDirectory);
            }

            _watermarkService.ClearTempFolder(files);

            return Redirect("/");
        }

        //wywolywane przez jquery ajax
        [HttpPost]
        public async Task<IActionResult> UploadAjax(FileUploadViewModel model)
        {
            if (!_watermarkService.ValidateFile(model))
                return BadRequest();

            await _watermarkService.UploadImageToTempFolder(model);

            return Ok();
        }



        //public IActionResult GetImage(Guid imageTempId, int imageIndex)
        //{
        ////TODO
        ////dla zapisaj oferty wgraj z azure
        ////dla nowej wgraj z dysku
        //}
    }
}