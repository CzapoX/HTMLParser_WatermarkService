﻿using ImageMagick;
using Limalima.Backend.Azure;
using Limalima.Backend.Models;
using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Limalima.Backend.Components
{
    public interface IWatermarkService
    {
        Task UploadImageToTempFolder(FileUploadViewModel model);
        Task<string> WatermarkImageAndUploadToAzure(string fileDirectory);
        void ClearTempFolder(string[] files);
        string[] GetFiles(AnnouceViewModel model);
        bool ValidateFile(FileUploadViewModel file);
        Task DownloadImageAsync(AnnouceViewModel tempId, string photoUrl);
    }

    public class WatermarkService : IWatermarkService
    {
        private static readonly string watermarkedImagesFolderPath = Directory.GetCurrentDirectory() + "/Images/Temp";
        private static readonly string tempImagesFolder = Path.Combine(Path.GetTempPath(), "FrutilsImagesTemp");

        private readonly IAzureImageUploadComponent _azureImageUpload;
        private readonly ILogger<WatermarkService> _logger;
        private readonly IImageValidator _imageValidator;
        private readonly IWebHostEnvironment _environment;

        public WatermarkService(IAzureImageUploadComponent azureImageUpload, IImageValidator imageValidator, IWebHostEnvironment environment, ILogger<WatermarkService> logger)
        {
            CreateFoldersIfNeeded();
            _azureImageUpload = azureImageUpload;
            _logger = logger;
            _imageValidator = imageValidator;
            _environment = environment;
        }

        public async Task UploadImageToTempFolder(FileUploadViewModel model)
        {
            try
            {
                string filename = model.ImageTempId + "_" + Guid.NewGuid() + Path.GetExtension(model.File.FileName);
                string tempFilename = Path.Combine(tempImagesFolder, filename);

                using var stream = File.Create(tempFilename);
                await model.File.CopyToAsync(stream);
            }
            catch (Exception er)
            {
                _logger.LogError(er, "UploadImageToTempFolder error");
            }
        }

        public async Task<string> WatermarkImageAndUploadToAzure(string fileDirectory)
        {
            try
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(fileDirectory);
                var image = GetImage(fileDirectory);
                var dominantColourCode = GetDominantColor(image);

                AddWatermark(image);
                ResizeImage(image);

                using var finalImage = new MagickImage(new MagickColor(dominantColourCode), 500, 500);
                finalImage.Composite(image, Gravity.Center, CompositeOperator.Over);

                return await UploadToAzure(fileName, finalImage);
            }
            catch (Exception er)
            {
                _logger.LogError(er, "WatermarkImageAndUploadToAzure error");
                return "";
            }
        }

        private string GetDominantColor(MagickImage image)
        {
            Dictionary<IMagickColor<ushort>, int> dict = image.Histogram();

            const int colorsToCompare = 80;
            int r = 0;
            int g = 0;
            int b = 0;

            var sortedDict = (from entry in dict orderby entry.Value descending select entry)
                .ToDictionary(pair => pair.Key, pair => pair.Value).Take(colorsToCompare);

            foreach (var entry in sortedDict)
            {
                r += entry.Key.R;
                g += entry.Key.G;
                b += entry.Key.B;
            }

            r /= colorsToCompare;
            g /= colorsToCompare;
            b /= colorsToCompare;


            //var keyOfMaxValue = dict.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            //var keyOfSecondValue = dict.Aggregate((x, y) => x.Value > y.Value && x.Key != keyOfMaxValue ? x : y).Key;
            //var keyOfThirdValue = dict.Aggregate((x, y) => x.Value > y.Value && (x.Key != keyOfMaxValue && x.Key != keyOfSecondValue) ? x : y).Key;

            //var r = (keyOfMaxValue.R + keyOfSecondValue.R + keyOfThirdValue.R) / 3;
            //var g = (keyOfMaxValue.G + keyOfSecondValue.G + keyOfThirdValue.G) / 3;
            //var b = (keyOfMaxValue.B + keyOfSecondValue.B + keyOfThirdValue.B) / 3;


            var dominantColourCode = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            return dominantColourCode;
        }

        public async Task DownloadImageAsync(AnnouceViewModel model, string photoUrl)
        {
            try
            {
                string filename = model.ImageTempId + "_" + Guid.NewGuid() + Path.GetExtension(photoUrl);
                using WebClient client = new WebClient();
                await client.DownloadFileTaskAsync(new Uri(photoUrl), Path.Combine(tempImagesFolder, filename));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadImageAsync");
            }
        }

        private void AddWatermark(MagickImage image)
        {
            using (var watermark = new MagickImage(_environment.WebRootPath + "/Images/watermark.png"))
            {
                // Draw the watermark in the bottom right corner
                image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);
            }
        }

        private MagickImage GetImage(string fileDirectory)
        {
            var image = new MagickImage(fileDirectory);
            return image;
        }

        private void ResizeImage(MagickImage image, int width = 500, int height = 500)
        {
            var size = new MagickGeometry(width, height);

            image.Resize(size);
        }


        private async Task<string> UploadToAzure(string fileName, MagickImage image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Format = MagickFormat.Png;
                image.Write(ms);
                ms.Position = 0;

                return await _azureImageUpload.UploadFileToStorage(ms, fileName);
            }
        }

        public void ClearTempFolder(string[] files)
        {
            foreach (var file in files) File.Delete(file);
        }

        public string[] GetFiles(AnnouceViewModel model)
        {
            return Directory.GetFiles(tempImagesFolder, model.ImageTempId + "*");
        }
        public bool ValidateFile(FileUploadViewModel file)
        {
            return file.File == null || !_imageValidator.ValidateFile(file.File);
        }

        private void CreateFoldersIfNeeded()
        {
            if (!Directory.Exists(watermarkedImagesFolderPath))
            {
                Directory.CreateDirectory(watermarkedImagesFolderPath);
            }

            if (!Directory.Exists(tempImagesFolder))
            {
                Directory.CreateDirectory(tempImagesFolder);
            }
        }

    }
}
