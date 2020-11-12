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
    public class PakameraParsingClient : IParsingClient
    {
        //https://www.pakamera.pl/bambaki przykladowy profil uzytkownika

        private readonly ILogger<PakameraParsingClient> _logger;
        private readonly IWatermarkService _watermarkService;

        public PakameraParsingClient(ILogger<PakameraParsingClient> logger, IWatermarkService watermarkService)
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
            string url = profileUrl;
            List<IList<string>> productLinksList = new List<IList<string>>();

            for (int index = 0; ; ++index)
            {
                if (index > 0)
                {
                    var previousPage = index - 1;
                    url = profileUrl.Replace("-" + previousPage, "-" + index);
                }

                var pageHtml = await GetPageHtml(url);
                var productsHtml = GetNode(pageHtml, "ul", "prod  lazy nextba clearfix");
                var productsList = GetListFromNode(productsHtml, "li", "class", "prd");
                var productsLink = GetProductsLinksToList(productsList);

                if (productLinksList.Any(o => o.SequenceEqual(productsLink)))
                    break;

                productLinksList.Add(productsLink);
            }

            var list = productLinksList.SelectMany(l => l).Distinct().ToList();
            string finalLink;
            IList<string> result = new List<string>();

            foreach (var link in list)
            {
                finalLink = link.Insert(0, "https://www.pakamera.pl");
                result.Add(finalLink);
            }

            return result;
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

        public string GetProductCategories(HtmlDocument productHtml)
        {

            var categoriesNode = GetNode(productHtml, "div", "tagcld");

            var categoryList = new List<HtmlNode>();

            categoryList = categoriesNode[0].Descendants("a")
                 .Where(n => n.GetAttributeValue("title", "")
                 .Any()
                ).ToList();


            var categoriesImported = new List<string>();

            foreach (var category in categoryList)
            {
                var categoryText = category.InnerText.Trim();

                categoriesImported.Add(categoryText);
            }
            string joined = String.Join(';', categoriesImported);

            return joined;
        }

        public List<string> GetProductPhotosUrl(HtmlDocument productHtml)
        {
            var photosNode = GetNode(productHtml, "div", "slide-main");

            var photoList = GetListFromNode(photosNode, "a", "class", "");

            var photosUrl = new List<string>();

            foreach (var photo in photoList)
            { 
                    var photoUrl = photo.GetAttributeValue("href", "");

                    if (!String.IsNullOrWhiteSpace(photoUrl))
                        photosUrl.Add(photoUrl);

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

        public async Task<HtmlDocument> GetPageHtml(string url)
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

        private List<HtmlNode> GetNode(HtmlDocument html, string descendantsNodeName, string className)
        {
            var node = new List<HtmlNode>();
            return node = html.DocumentNode.Descendants(descendantsNodeName)
                    .Where(n => n.GetAttributeValue("class", "")
                    .StartsWith(className)).ToList();
        }

        private List<HtmlNode> GetListFromNode(IList<HtmlNode> productsHtml, string descendantsNodeName, string attributeName, string attributeValue)
        {
            var list = new List<HtmlNode>();

            return list = productsHtml[0].Descendants(descendantsNodeName)
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

        private string GetProductChoosenElementText(HtmlDocument productHtml, string nodeName, string attributeName, string attributeValue)
        {
            var productList = productHtml.DocumentNode.Descendants(nodeName).
                Where(n => n.GetAttributeValue(attributeName, "")
                .Equals(attributeValue));
            var product = productList.SingleOrDefault();

            return product.InnerHtml.Trim();
        }

        private string GetProductName(HtmlDocument productHtml)
        {
            var name = GetProductChoosenElementText(productHtml, "h1", "", "");

            return name;
        }

        private Decimal GetProductPrice(HtmlDocument productHtml)
        {
            var priceNodeText = GetProductChoosenElementText(productHtml, "span", "class", "ppp bnvalue");
            var priceAsString = Regex.Replace(priceNodeText, "[^0-9.,]", "");
            priceAsString = priceAsString.Remove(0, priceAsString.Length / 2);

            var result = Decimal.Parse(priceAsString);

            return result;
        }

        private string GetProductDescription(HtmlDocument productHtml)
        {
            var description = GetProductChoosenElementText(productHtml, "span", "class", "ades");

            return Regex.Replace(description, @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>", "$1");
        }

        private string GetProductMaterials(HtmlDocument productHtml)
        {
            return "";
        }
    }
}