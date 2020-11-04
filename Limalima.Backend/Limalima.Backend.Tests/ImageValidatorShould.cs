using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IO;
using Xunit;

namespace Limalima.Backend.Tests
{
    public class ImageValidatorShould
    {
    //    Mock<IConfiguration> configMock = new Mock<IConfiguration>();

    //    [Fact]
    //    public void ForGooldFileValidationShouldPass()
    //    {
    //        Mock<IFormFile> fileMock = new Mock<IFormFile>();
    //        var content = "Hello World from a Fake File";
    //        var fileName = "test.pdf";
    //        var ms = new MemoryStream();
    //        var writer = new StreamWriter(ms);
    //        writer.Write(content);
    //        writer.Flush();
    //        ms.Position = 0;
    //        ImageValidator sut = new ImageValidator(configMock.Object);

    //        bool retval = sut.ValidateFile(fileMock.Object);

    //        Assert.True(retval);
    //    }
    }
}
