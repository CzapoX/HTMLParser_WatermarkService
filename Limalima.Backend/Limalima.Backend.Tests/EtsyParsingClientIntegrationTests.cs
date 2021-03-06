﻿using Limalima.Backend.Azure;
using Limalima.Backend.Components;
using Limalima.Backend.Components.ParsingClient;
using Limalima.Backend.Models;
using Limalima.Backend.Validation;
using Microsoft.Extensions.Configuration;
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
        private string sampleEtsyUserUrlThatNeedTrim = "https://www.etsy.com/shop/BusyPuzzle?ref=simple-shop-header-name&listing_id=862159716";
        private readonly string singleEtsyProductUrl = "https://www.etsy.com/pl/listing/873965045/custom-initial-necklace-script-initial";

        private readonly List<string> sampleEtsyProductsUrl = new List<string>
        {
            "https://www.etsy.com/pl/listing/924734514/black-baseball-cap-bad-to-the-bone?ref=shop_home_active_17",
            "https://www.etsy.com/pl/listing/940398871/pink-sphynx-cat-wall-art-mystical-decor?ref=shop_home_active_1",
            "https://www.etsy.com/pl/listing/825941394/llama-tote-bag-alpaca-shopping-bag-bag?ref=shop_home_active_23&pro=1"
        };

        public EtsyParsingClientIntegrationTests()
        {
            configuration = new ConfigurationBuilder()
              .AddInMemoryCollection(configurationSettings)
              .Build();
            Mock<ImageValidator> imageValidator = new Mock<ImageValidator>(configuration);
            Mock<IAzureImageUploadComponent> azureUploadComponent = new Mock<IAzureImageUploadComponent>();
            Mock<IWatermarkService> watermarkService = new Mock<IWatermarkService>();

            watermarkService.Setup(e => e.WatermarkImageAndUploadToAzure(It.IsAny<string>())).ReturnsAsync("Url");
            watermarkService.Setup(e => e.GetFiles(It.IsAny<AnnouceViewModel>())).Returns(new string[] { "Directory" });

            sut = new EtsyParsingClient(watermarkService.Object);
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
            var itemHtmlList = await sut.GetProductsHtml(sampleEtsyProductsUrl);

            var results = await sut.CreateArtListAsync(itemHtmlList);

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

        [Fact]
        public void ShouldRemoveAllCharactersAfterQuestionMark()
        {
            var url = sut.PrepareEtsyLink(sampleEtsyUserUrlThatNeedTrim);

            Assert.DoesNotContain("?ref", url);
            Assert.NotEmpty(url);
            Assert.Contains("etsy", url);
        }
    }
}
