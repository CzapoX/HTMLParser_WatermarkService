using HtmlAgilityPack;
using Limalima.Backend.Components.ParsingClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Limalima.Backend.Components
{
    public class PakameraParsingClient : BaseParsingClient
    {
        //https://www.pakamera.pl/bambaki przykladowy profil uzytkownika        

        public PakameraParsingClient(ILogger<PakameraParsingClient> logger, IWatermarkService watermarkService) : base(logger, watermarkService)
        {
        }

        public override async Task<IList<string>> GetProductsLinks(string profileUrl)
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

        public override string GetProductMaterials(HtmlDocument productHtml)
        {
            return "";
        }

        public override string GetProductCategories(HtmlDocument productHtml)
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

        public override List<string> GetProductPhotosUrl(HtmlDocument productHtml)
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

        public override string GetProductName(HtmlDocument productHtml)
        {
            var name = GetProductChoosenElementText(productHtml, "h1", "", "");

            return name;
        }

        public override Decimal GetProductPrice(HtmlDocument productHtml)
        {
            var priceNodeText = GetProductChoosenElementText(productHtml, "span", "class", "ppp bnvalue");
            var priceAsString = Regex.Replace(priceNodeText, "[^0-9.,]", "");
            priceAsString = priceAsString.Remove(0, priceAsString.Length / 2);

            var result = Decimal.Parse(priceAsString);

            return result;
        }

        public override string GetProductDescription(HtmlDocument productHtml)
        {
            var description = GetProductChoosenElementText(productHtml, "span", "class", "ades");

            return Regex.Replace(description, @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>", "$1");
        }
    }
}