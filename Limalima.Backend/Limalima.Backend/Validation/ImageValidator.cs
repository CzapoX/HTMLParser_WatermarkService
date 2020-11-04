using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Limalima.Backend.Validation
{
    public class ImageValidator : IImageValidator
    {
        private readonly long _fileSizeLimit;

        private static readonly Dictionary<string, List<byte[]>> _fileSignature =
            new Dictionary<string, List<byte[]>>
{
            { ".jpeg", new List<byte[]>
                {
                     new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                     new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                     new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                     new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                 }
             },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                 }
            },
            { ".png", new List<byte[]>
                {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                }
            },
           };

        public ImageValidator(IConfiguration config)
        {
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
        }

        private bool CheckFileExtension(IFormFile formFile)
        {
            string[] permittedExtensions = { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(formFile.FileName).ToLowerInvariant();

            return !(string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext));

        }

        private bool CheckFileSignature(IFormFile formFile)
        {
            using (var reader = new BinaryReader(formFile.OpenReadStream()))
            {
                var ext = Path.GetExtension(formFile.FileName).ToLowerInvariant();
                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return (signatures.Any(signature => headerBytes
                    .Take(signature.Length)
                    .SequenceEqual(signature)));
            }
        }

        private bool CheckMaxFileSize(IFormFile formFile)
        {
            return (formFile.Length <= _fileSizeLimit);
        }

        public bool ValidateFile(IFormFile formFile)
        {
            return (CheckFileExtension(formFile) &&
                CheckMaxFileSize(formFile) &&
                CheckFileSignature(formFile));
        }
    }
}
