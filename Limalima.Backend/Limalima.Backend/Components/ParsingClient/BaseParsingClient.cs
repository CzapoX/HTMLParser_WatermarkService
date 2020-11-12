using HtmlAgilityPack;
using Limalima.Backend.Data;
using Limalima.Backend.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Limalima.Backend.Components.ParsingClient
{
    public interface IParsingClient
    {
        Task<IList<Art>> GetArtsFromUser(string url);
    }


    abstract public class BaseParsingClient : IParsingClient
    {
        protected readonly ILogger<BaseParsingClient> _logger;
        protected readonly IWatermarkService _watermarkService;

        protected BaseParsingClient(ILogger<BaseParsingClient> logger, IWatermarkService watermarkService)
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

        protected List<HtmlNode> GetNode(HtmlDocument html, string descendantsNodeName, string className)
        {
            var node = new List<HtmlNode>();
            return node = html.DocumentNode.Descendants(descendantsNodeName)
                    .Where(n => n.GetAttributeValue("class", "")
                    .StartsWith(className)).ToList();
        }
        protected List<HtmlNode> GetListFromNode(IList<HtmlNode> productsHtml, string descendantsNodeName, string attributeName, string attributeValue)
        {
            var list = new List<HtmlNode>();

            return list = productsHtml[0].Descendants(descendantsNodeName)
                 .Where(n => n.GetAttributeValue(attributeName, "")
                 .Equals(attributeValue)).ToList();
        }

        protected IList<string> GetProductsLinksToList(IList<HtmlNode> productsList)
        {
            var productsLinkList = new List<string>();
            foreach (var productItem in productsList)
            {
                var productLink = productItem.Descendants("a").FirstOrDefault().GetAttributeValue("href", "");
                productsLinkList.Add(productLink);
            }

            return productsLinkList;
        }
        protected string GetProductChoosenElementText(HtmlDocument productHtml, string nodeName, string attributeName, string attributeValue)
        {
            var productList = productHtml.DocumentNode.Descendants(nodeName).
                Where(n => n.GetAttributeValue(attributeName, "")
                .Equals(attributeValue));
            var product = productList.SingleOrDefault();

            return product.InnerHtml.Trim();
        }

        public abstract Task<IList<string>> GetProductsLinks(string profileUrl);
        public abstract string GetProductMaterials(HtmlDocument productHtml);
        public abstract string GetProductCategories(HtmlDocument productHtml);
        public abstract List<string> GetProductPhotosUrl(HtmlDocument productHtml);
        public abstract string GetProductName(HtmlDocument productHtml);
        public abstract decimal GetProductPrice(HtmlDocument productHtml);
        public abstract string GetProductDescription(HtmlDocument productHtml);

    }
}
