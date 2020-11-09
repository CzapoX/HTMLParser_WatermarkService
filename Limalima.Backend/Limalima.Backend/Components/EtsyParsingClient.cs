using HtmlAgilityPack;
using Limalima.Backend.Data;
using Limalima.Backend.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Limalima.Backend.Components
{
    public interface IParsingClient
    {
        Task<IList<Art>> GetArtsFromUser(string url);
    }

    public class EtsyParsingClient : IParsingClient
    {

        //https://www.etsy.com/pl/shop/GracePersonalized przykladowy profil uzytkownika

        private readonly ILogger<EtsyParsingClient> _logger;
        private readonly IWatermarkService _watermarkService;

        public EtsyParsingClient(ILogger<EtsyParsingClient> logger, IWatermarkService watermarkService)
        {
            _logger = logger;
            _watermarkService = watermarkService;
        }

        public async Task<IList<Art>> GetArtsFromUser(string url)
        {
            try
            {
                var productLinksList = await GetProductsLinks(url);
                var productsHtmlList = await GetProductsHtml(productLinksList);

                return await CreateArtListAsync(productsHtmlList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetArtsFromUser error");

                return new List<Art>();
            }
        }

        public async Task<IList<string>> GetProductsLinks(string profileUrl)
        {
            var pageHtml = await GetPageHtml(profileUrl);
            var productsHtml = GetNode(pageHtml, "listing-cards");
            var productsList = FetchListFromNode(productsHtml, "data-shop-id");

            return GetProductsLinksToList(productsList);
        }

        public async Task<IList<HtmlDocument>> GetProductsHtml(IList<string> itemsLinksList)
        {
            try
            {
                var productsHtml = new List<HtmlDocument>();
                foreach (var url in itemsLinksList)
                {
                    var productHtml = await GetPageHtml(url);
                    productsHtml.Add(productHtml);
                }

                return productsHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductsHtml error");

                return new List<HtmlDocument>();
            }
        }

        public async Task<IList<Art>> CreateArtListAsync(IList<HtmlDocument> productsHtmlList)
        {
            var artList = new List<Art>();
            var ownerId = Guid.NewGuid();

            foreach (var productHtml in productsHtmlList)
            {
                var art = new Art
                {
                    OwnerId = ownerId,
                    Name = GetProductName(productHtml),
                    Price = GetProductPrice(productHtml),
                    Description = GetProductDescription(productHtml),
                    Status = ArtStatus.Imported,
                    CategoriesImported = GetProductCategories(productHtml),
                    MaterialsImported = GetProductMaterials(productHtml)
                };

                art.ArtPhotos = await ImportImagesToAzure(productHtml, art.ArtId);
                art.MainPhotoUrl = art.ArtPhotos[0].Url;

                artList.Add(art);
            }

            return artList;
        }
        public string GetProductMaterials(HtmlDocument productHtml)
        {
            var detailsNode = GetNode(productHtml, "wt-text-body-01");

            if (detailsNode.Count == 0)
                return "";

            var detailList = FetchListFromNode(detailsNode, "class");
            var materials = "";

            foreach (var detailText in detailList)
            {
                var productDetail = detailText.InnerText.Trim();

                if (productDetail.Contains("Materiały:"))
                    materials = productDetail.Replace("Materiały: ", "");
            }

            return materials.Replace(",", ";");
        }

        public string GetProductCategories(HtmlDocument productHtml)
        {
            var categoriesNode = GetNode(productHtml, "wt-action-group wt-list-inline wt-mb-xs-2");

            var categorylist = FetchListFromNode(categoriesNode, "class", "wt-action-group__item-container");
            var categoriesImported = new List<string>();

            foreach (var category in categorylist)
            {
                var categoryText = category.InnerText.Trim();

                categoriesImported.Add(categoryText);
            }
            string joined = String.Join(';', categoriesImported);

            return joined;
        }

        public List<string> GetProductPhotosUrl(HtmlDocument productHtml)
        {
            var photosNode = GetNode(productHtml, "wt-list-unstyled wt-overflow-hidden wt-position-relative carousel-pane-list");

            var photoList = FetchListFromNode(photosNode, "data-carousel-pane", "");
            var photosUrl = new List<string>();

            foreach (var photo in photoList)
            {
                var photoNode = photo.Descendants("img");
                if (photoNode != null)
                {
                    var photoUrl = photoNode.FirstOrDefault()?.GetAttributeValue("data-src", "");

                    if (String.IsNullOrWhiteSpace(photoUrl))
                        photoUrl = photoNode.FirstOrDefault()?.GetAttributeValue("src", "");

                    if (!String.IsNullOrWhiteSpace(photoUrl))
                        photosUrl.Add(photoUrl);
                }

            }

            return photosUrl;
        }

        public async Task<List<ArtPhoto>> ImportImagesToAzure(HtmlDocument productHtml, Guid artId)
        {
            try
            {
                AnnouceViewModel model = new AnnouceViewModel { ImageTempId = Guid.NewGuid() };
                var photosUrl = GetProductPhotosUrl(productHtml);
                var artPhotoList = new List<ArtPhoto>();

                foreach (var photoUrl in photosUrl)
                {
                    await _watermarkService.DownloadImageAsync(model, photoUrl);
                }

                string[] files = _watermarkService.GetFiles(model);

                foreach (var fileDirectory in files)
                {
                    var url = await _watermarkService.WatermarkImageAndUploadToAzure(fileDirectory);
                    var artPhoto = new ArtPhoto { Url = url, ArtId = artId };
                    artPhotoList.Add(artPhoto);
                }

                _watermarkService.ClearTempFolder(files);

                return artPhotoList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportImagesToAzure error");

                return new List<ArtPhoto>();
            }
        }

        private async Task<HtmlDocument> GetPageHtml(string url)
        {
            try
            {
                var webGet = new HtmlWeb();
                var html = await webGet.LoadFromWebAsync(url);

                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPageHtml error");

                return new HtmlDocument();
            }
        }

        private List<HtmlNode> GetNode(HtmlDocument html, string className)
        {
            var node = new List<HtmlNode>();
            return node = html.DocumentNode.Descendants("ul")
                    .Where(n => n.GetAttributeValue("class", "")
                    .StartsWith(className)).ToList();
        }

        private List<HtmlNode> FetchListFromNode(IList<HtmlNode> productsHtml, string attributeName, string attributeValue = "")
        {
            var list = new List<HtmlNode>();

            return list = productsHtml[0].Descendants("li")
                 .Where(n => n.GetAttributeValue(attributeName, "")
                 .Equals(attributeValue)).ToList();
        }

        private IList<string> GetProductsLinksToList(IList<HtmlNode> productsList)
        {
            var productsLinkList = new List<string>();
            foreach (var productItem in productsList)
            {
                var productLink = productItem.Descendants("a").FirstOrDefault().GetAttributeValue("href", "");
                productsLinkList.Add(productLink);
            }

            return productsLinkList;
        }

        private string GetProductChoosenElemenText(HtmlDocument productHtml, string nodeName, string attributeName, string attributeValue)
        {
            var productList = productHtml.DocumentNode.Descendants(nodeName).
                Where(n => n.GetAttributeValue(attributeName, "")
                .Equals(attributeValue));
            var product = productList.SingleOrDefault();

            return product.InnerHtml.Trim();
        }

        private string GetProductName(HtmlDocument productHtml)
        {
            var name = GetProductChoosenElemenText(productHtml, "h1", "class", "wt-text-body-03 wt-line-height-tight wt-break-word wt-mb-xs-1");

            return name;
        }

        private Decimal GetProductPrice(HtmlDocument productHtml)
        {
            var priceAsString = GetProductChoosenElemenText(productHtml, "p", "class", "wt-text-title-03 wt-mr-xs-2");
            priceAsString = Regex.Replace(priceAsString, "[^0-9,]", "");
            var result = Decimal.Parse(priceAsString);

            return result;
        }

        private string GetProductDescription(HtmlDocument productHtml)
        {
            var description = GetProductChoosenElemenText(productHtml, "p", "class", "wt-text-body-01 wt-break-word");

            return Regex.Replace(description, @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>", "$1");
        }
    }
}

