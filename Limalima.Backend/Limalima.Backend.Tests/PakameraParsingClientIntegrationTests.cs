using Limalima.Backend.Azure;
using Limalima.Backend.Components;
using Limalima.Backend.Models;
using Limalima.Backend.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Limalima.Backend.Tests
{
    public class PakameraParsingClientIntegrationTests
    {
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> configurationSettings = new Dictionary<string, string>
        {
            {"FileSizeLimit", "5097152"},
        };

        private readonly PakameraParsingClient sut;
        private readonly string samplePakameraUserUrl = "https://www.pakamera.pl/pookys-world-0_s12297558.htm";

        private readonly string singlePakameraProductUrl = "https://www.pakamera.pl/moda-t-shirty-unisex-t-shirt-black-gold-snb-longer-nr2738670.htm";

        private readonly List<string> samplePakameraProductsUrl = new List<string>
        {
            "https://www.pakamera.pl/przechowywanie-kosze-kosz-metalowy-2-szt-warehouse-nr2740387.htm",
            "https://www.pakamera.pl/zakladki-do-ksiazek-zakladka-zolty-ptaszek-braz-nr2740157.htm",
            "https://www.pakamera.pl/dziecko-plakaty-obrazki-obraz-na-plotnie-100x70cm-kolorowe-kamienice-nr2725025.htm"
        };

        public PakameraParsingClientIntegrationTests()
        {
            configuration = new ConfigurationBuilder()
              .AddInMemoryCollection(configurationSettings)
              .Build();
            Mock<ImageValidator> imageValidator = new Mock<ImageValidator>(configuration);

            Mock<ILogger<PakameraParsingClient>> loggerPakamera = new Mock<ILogger<PakameraParsingClient>>();


            Mock<IAzureImageUploadComponent> azureUploadComponent = new Mock<IAzureImageUploadComponent>();
            Mock<IWatermarkService> watermarkService = new Mock<IWatermarkService>();

            watermarkService.Setup(e => e.WatermarkImageAndUploadToAzure(It.IsAny<string>())).ReturnsAsync("Url");
            watermarkService.Setup(e => e.GetFiles(It.IsAny<AnnouceViewModel>())).Returns(new string[] { "Directory" });

            sut = new PakameraParsingClient(watermarkService.Object);
        }

        [Fact]
        public async Task ShouldFetchProductUrlsForProfileUrl()
        {
            var results = await sut.GetProductsLinks(samplePakameraUserUrl);

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ShouldFetchProductHtmlForProductUrl()
        {
            var results = await sut.GetProductsHtml(samplePakameraProductsUrl);

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ShouldFetchArtList()
        {
            //arrage
            var itemHtmlList = await sut.GetProductsHtml(samplePakameraProductsUrl);

            //act
            var results = await sut.CreateArtListAsync(itemHtmlList);

            //assert
            Assert.NotEmpty(results);

            var sampleArt = results[0];

            Assert.NotEmpty(sampleArt.Name);
            Assert.False(sampleArt.Name.StartsWith(" "));
            Assert.False(sampleArt.Name.StartsWith("\n "));
            Assert.False(sampleArt.Name.EndsWith(" "));
            Assert.False(sampleArt.Name.EndsWith(" \n"));

            Assert.False(sampleArt.Name.Contains("<a", System.StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task ShouldFetchArtListForProfileUrl()
        {
            var results = await sut.GetArtsFromUser(samplePakameraUserUrl);

            Assert.NotEmpty(results);
        }


        [Fact]
        public async Task ShouldFetchProductCategories()
        {
            var itemHtml = await sut.GetPageHtml(singlePakameraProductUrl);

            var results = sut.GetProductCategories(itemHtml);

            Assert.Contains("t-shirty męskie", results);
        }

        [Fact]
        public async Task ShouldFetchPhotosUrl()
        {
            var itemHtml = await sut.GetPageHtml(singlePakameraProductUrl);

            var results = sut.GetProductPhotosUrl(itemHtml);

            Assert.NotEmpty(results);
        }
    }
}
