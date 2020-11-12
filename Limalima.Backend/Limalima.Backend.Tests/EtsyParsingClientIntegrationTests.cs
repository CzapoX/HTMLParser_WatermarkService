using Limalima.Backend.Azure;
using Limalima.Backend.Components;
using Limalima.Backend.Components.ParsingClient;
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
    public class EtsyParsingClientIntegrationTests
    {

        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> configurationSettings = new Dictionary<string, string>
        {
            {"FileSizeLimit", "5097152"},          
        };

        private readonly EtsyParsingClient sut;
        private readonly string sampleEtsyUserUrl = "https://www.etsy.com/shop/RedVesselDesigns";

        private readonly string singleEtsyProductUrl = "https://www.etsy.com/pl/listing/873965045/custom-initial-necklace-script-initial";

        private readonly List<string> sampleEtsyProductsUrl = new List<string>
        {
            "https://www.etsy.com/listing/898773161/custom-handmade-vintage-style-coffee",
            "https://www.etsy.com/listing/897583583/ceramic-bending-vase-minimalist-curve",
            "https://www.etsy.com/pl/listing/746855885/dainty-name-choker-necklace-custom-name"
        };

        public EtsyParsingClientIntegrationTests()
        {
            configuration = new ConfigurationBuilder()
              .AddInMemoryCollection(configurationSettings)
              .Build();
            Mock<ImageValidator> imageValidator = new Mock<ImageValidator>(configuration);
           
            Mock<ILogger<EtsyParsingClient>> loggerEtsy = new Mock<ILogger<EtsyParsingClient>>();


            Mock<IAzureImageUploadComponent> azureUploadComponent = new Mock<IAzureImageUploadComponent>();
            Mock<IWatermarkService> watermarkService = new Mock<IWatermarkService>();

            watermarkService.Setup(e=>e.WatermarkImageAndUploadToAzure(It.IsAny<string>())).ReturnsAsync("Url");
            watermarkService.Setup(e => e.GetFiles(It.IsAny<AnnouceViewModel>())).Returns(new string[] { "Directory" });

            sut = new EtsyParsingClient(loggerEtsy.Object, watermarkService.Object);
        }

        [Fact]
        public async Task ShouldFetchProductUrlsForProfileUrl()
        {
            var results = await sut.GetProductsLinks(sampleEtsyUserUrl);

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ShouldFetchProductHtmlForProductUrl()
        {
            var results = await sut.GetProductsHtml(sampleEtsyProductsUrl);

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ShouldFetchArtList()
        {
            //arrage
            var itemHtmlList = await sut.GetProductsHtml(sampleEtsyProductsUrl);

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
            var results = await sut.GetArtsFromUser(sampleEtsyUserUrl);

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ShouldFetchProductMaterials()
        {
            var itemHtml = await sut.GetPageHtml(singleEtsyProductUrl);

            var results = sut.GetProductMaterials(itemHtml);

            Assert.Equal("Różowe złoto; Srebro; Złoto", results);
        }

        [Fact]
        public async Task ShouldFetchProductCategories()
        {
            var itemHtml = await sut.GetPageHtml(singleEtsyProductUrl);

            var results = sut.GetProductCategories(itemHtml);

            Assert.Contains("Biżuteria", results);
        }

        [Fact]
        public async Task ShouldFetchPhotosUrl()
        {
            var itemHtml = await sut.GetPageHtml(singleEtsyProductUrl);

            var results = sut.GetProductPhotosUrl(itemHtml);

            Assert.NotEmpty(results);
        }
    }
}
