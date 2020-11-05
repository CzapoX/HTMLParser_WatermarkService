using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Limalima.Backend.Tests
{
    public class ImageValidatorTest
    {

        private IConfiguration configuration;
        private IImageValidator imageValidator;

        private static readonly string imageMockDirectory =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MockFiles", "MockImage.jpg");
        private static readonly string pdfFile =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MockFiles", "MockPdf.pdf");
        private static readonly string txtFile =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MockFiles", "MockTxt.txt");
        private static readonly string emptyJpgFile =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MockFiles", "MockEmptyJpg.jpg");

        public static readonly List<object[]> wrongFilesDirectory = new List<object[]>
        {
                new object[] {pdfFile},
                new object[] {txtFile},
                new object[] {emptyJpgFile}
        };

        private Dictionary<string, string> configurationSettings = new Dictionary<string, string>
        {
                {"FileSizeLimit", "5097152"},
        };

        public ImageValidatorTest()
        {
            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationSettings)
                .Build();

            imageValidator = new ImageValidator(configuration);
        }


        [Fact]
        public void ForGoodFileValidationShouldPass()
        {
            IFormFile mockFile = GetMockFile(imageMockDirectory);

            bool retval = ValidateFile(mockFile);

            Assert.True(retval);
        }


        [Theory]
        [MemberData(nameof(wrongFilesDirectory))]
        public void ForWrongFileValidationShouldPass(string fileDirectory)
        {
            IFormFile mockFile = GetMockFile(fileDirectory);

            bool retval = ValidateFile(mockFile);

            Assert.False(retval);
        }

        private IFormFile GetMockFile(string fileDirectory)
        {
            var physicalFile = new FileInfo(fileDirectory);
            var fileMock = new Mock<IFormFile>();

            var stream = physicalFile.OpenRead();
            var fileName = physicalFile.Name;

            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(stream.Length);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(stream);

            var file = fileMock.Object;
            return file;
        }

        private bool ValidateFile(IFormFile mockFile)
        {
            return imageValidator.ValidateFile(mockFile);
        }
    }
}
