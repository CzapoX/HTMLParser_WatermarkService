using Limalima.Backend.Validation;
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

        private readonly IConfiguration configuration;
        private readonly IImageValidator imageValidator;

        private readonly Dictionary<string, string> configurationSettings = new Dictionary<string, string>
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
            IFormFile mockFile = GetMockFile(GetPath("MockImage.jpg"));

            bool retval = ValidateFile(mockFile);

            Assert.True(retval);
        }

        [Theory]
        [InlineData("MockPdf.pdf")]
        [InlineData("MockTxt.txt")]
        [InlineData("MockEmptyJpg.jpg")]
        public void ForWrongFileValidationShouldPass(string file)
        {
            IFormFile mockFile = GetMockFile(GetPath(file));

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
        private string GetPath(string file)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MockFiles", file);
        }
    }
}
