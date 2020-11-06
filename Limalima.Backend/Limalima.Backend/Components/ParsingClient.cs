using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Limalima.Backend.Components
{
    public interface IParsingClient
    {
        Task<IList<string>> GetProductsLinks(string profileUrl);
        Task<IList<HtmlDocument>> GetProductsHtml(IList<string> itemsLinksList);
    }

    public class ParsingClient : IParsingClient
    {
        private readonly ILogger<ParsingClient> _logger;


        public ParsingClient(ILogger<ParsingClient> logger)
        {
            _logger = logger;
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


        public async Task<IList<string>> GetProductsLinks(string profileUrl)
        {
            var pageHtml = await GetPageHtml(profileUrl);
            var productsHtml = GetProductsHtml(pageHtml);
            var productsList = GetProductsList(productsHtml);
            return GetProductsLinksToList(productsList);
        }


        private async Task<HtmlDocument> GetPageHtml(string url)
        {
            try
            {
                var webGet = new HtmlWeb();
                var profileHtml = await webGet.LoadFromWebAsync(url);
                return profileHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPageHtml error");
                return new HtmlDocument();
            }
        }

        private List<HtmlNode> GetProductsHtml(HtmlDocument profileHtml)
        {
            var productsHtml = new List<HtmlNode>();
            return productsHtml = profileHtml.DocumentNode.Descendants("ul")
                    .Where(n => n.GetAttributeValue("class", "")
                    .Equals("listing-cards block-grid-xs-2 block-grid-md-3  block-grid-lg-3 block-grid-xl-4 block-grid-no-whitespace mb-xs-3")).ToList();

        }

        private List<HtmlNode> GetProductsList(IList<HtmlNode> productsHtml)
        {
            var productsList = new List<HtmlNode>();
            return productsList = productsHtml[0].Descendants("li")
                 .Where(n => n.GetAttributeValue("data-shop-id", "")
                 .Equals("")).ToList();
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

    }
}

