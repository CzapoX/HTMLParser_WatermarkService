using Limalima.Backend.Components;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Limalima.Backend.Validation;
using Limalima.Backend.Azure;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using System.IO;

namespace Limalima.Backend.Tests
{
    public class EtsyParsingClientIntegrationTests
    {

        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> configurationSettings = new Dictionary<string, string>
        {
            {"FileSizeLimit", "5097152"},
            { "AZURE_STORAGE_CONNECTION_STRING", "DefaultEndpointsProtocol=https;AccountName=praktykistorageaccount;AccountKey=stbVFtmTUgT8O0cjl8/sSDfwoP4xbnnxWCDZYPRRez1iX5PxkgZmpqJkWL9sfWs1UEj8zlyrQ3O7qaWjxA0UMA==;EndpointSuffix=core.windows.net" },
            { "AZURE_STORAGE_CONTAINER", "obrazki" }

        };

        private readonly EtsyParsingClient sut;
        private readonly string sampleEtsyUserUrl = "https://www.etsy.com/shop/RedVesselDesigns";
        private readonly List<string> sampleEtsyProductUrlForFetching = new List<string>
        {
            "https://www.etsy.com/pl/listing/873965045/custom-initial-necklace-script-initial"
        };
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
            Mock<ILogger<WatermarkService>> loggerWatermark = new Mock<ILogger<WatermarkService>>();
            Mock<IWebHostEnvironment> environment = new Mock<IWebHostEnvironment>();
            string testsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            environment.Setup(e => e.WebRootPath).Returns(testsPath);

            Mock<ILogger<AzureImageUploadComponent>> loggerAzure = new Mock<ILogger<AzureImageUploadComponent>>();
            Mock<AzureImageUploadComponent> azureUploadComponent = new Mock<AzureImageUploadComponent>(configuration, loggerAzure.Object);
            Mock<WatermarkService> watermarkService = new Mock<WatermarkService>(azureUploadComponent.Object, imageValidator.Object, environment.Object, loggerWatermark.Object);
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
        public void ShouldFetchProductMaterials()
        {
            var itemHtmlList = sut.GetProductsHtml(sampleEtsyProductUrlForFetching).Result;
            var itemHtml = itemHtmlList[0];

            var results = sut.GetProductMaterials(itemHtml);

            Assert.Equal("Różowe złoto, Srebro, Złoto", results);
        }

        [Fact]
        public void ShouldFetchProductCategories()
        {
            var itemHtmlList = sut.GetProductsHtml(sampleEtsyProductUrlForFetching).Result;
            var itemHtml = itemHtmlList[0];

            var results = sut.GetProductCategories(itemHtml);

            Assert.Contains("Biżuteria", results);
        }

        [Fact]
        public void ShouldFetchPhotosUrl()
        {
            var itemHtmlList = sut.GetProductsHtml(sampleEtsyProductUrlForFetching).Result;
            var itemHtml = itemHtmlList[0];

            var results = sut.GetProductPhotosUrl(itemHtml);

            Assert.NotNull(results);
        }
    }
}
