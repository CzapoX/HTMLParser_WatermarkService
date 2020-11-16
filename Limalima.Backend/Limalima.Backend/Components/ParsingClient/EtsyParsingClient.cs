using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Limalima.Backend.Components.ParsingClient
{
    public class EtsyParsingClient : BaseParsingClient
    {
        //https://www.etsy.com/pl/shop/GracePersonalized przykladowy profil uzytkownika

        public EtsyParsingClient(IWatermarkService watermarkService) : base(watermarkService) { }

        public override async Task<IList<string>> GetProductsLinks(string profileUrl)
        {
            string url = profileUrl;
            List<IList<string>> productLinksList = new List<IList<string>>();

            for (int index = 1; ; ++index)
            {
                url = profileUrl + "?page=" + index;

                var pageHtml = await GetPageHtml(url);
                var productsHtml = GetNode(pageHtml, "ul", "listing-cards");
                var productsList = GetListFromNode(productsHtml, "li", "data-shop-id", "");
                var productsLink = GetProductsLinksToList(productsList);

                if (productLinksList.Any(o => o.SequenceEqual(productsLink)))
                    break;

                productLinksList.Add(productsLink);
            }

            return productLinksList.SelectMany(l => l).Distinct().ToList();
        }

        public override string GetProductMaterials(HtmlDocument productHtml)
        {
            var detailsNode = GetNode(productHtml, "ul", "wt-text-body-01");

            if (detailsNode.Count == 0)
                return "";

            var detailList = GetListFromNode(detailsNode, "li", "class", "");
            var materials = "";

            foreach (var detailText in detailList)
            {
                var productDetail = detailText.InnerText.Trim();

                if (productDetail.Contains("Materiały:"))
                    materials = productDetail.Replace("Materiały: ", "");
            }

            return materials.Replace(",", ";");
        }

        public override string GetProductCategories(HtmlDocument productHtml)
        {
            var categoriesNode = GetNode(productHtml, "ul", "wt-action-group wt-list-inline wt-mb-xs-2");
            var categorylist = GetListFromNode(categoriesNode, "li", "class", "wt-action-group__item-container");

            string joined = categorylist
                .Select(c => c.InnerText.Trim())
                .Aggregate(String.Empty, (current, next) => current + ";" + next);

            return joined;
        }

        public override List<string> GetProductPhotosUrl(HtmlDocument productHtml)
        {
            var photosNode = GetNode(productHtml, "ul", "wt-list-unstyled wt-overflow-hidden wt-position-relative carousel-pane-list");

            var photoList = GetListFromNode(photosNode, "li", "data-carousel-pane", "");
            var photosUrl = new List<string>();

            foreach (var photo in photoList)
            {
                var photoNode = photo.Descendants("img");
                if (photoNode != null)
                {
                    var photoUrl = photoNode.FirstOrDefault()?.GetAttributeValue("data-src", "");

                    if (string.IsNullOrWhiteSpace(photoUrl))
                        photoUrl = photoNode.FirstOrDefault()?.GetAttributeValue("src", "");

                    if (!string.IsNullOrWhiteSpace(photoUrl))
                        photosUrl.Add(photoUrl);
                }
            }
            return photosUrl;
        }

        protected override string GetProductName(HtmlDocument productHtml)
        {
            var name = GetProductChoosenElementText(productHtml, "h1", "class", "wt-text-body-03 wt-line-height-tight wt-break-word wt-mb-xs-1");

            return name;
        }

        protected override decimal GetProductPrice(HtmlDocument productHtml)
        {
            var priceAsString = GetProductChoosenElementText(productHtml, "p", "class", "wt-text-title-03 wt-mr-xs-2");
            priceAsString = Regex.Replace(priceAsString, "[^0-9,]", "");
            var result = decimal.Parse(priceAsString);

            return result;
        }

        protected override string GetProductDescription(HtmlDocument productHtml)
        {
            var description = GetProductChoosenElementText(productHtml, "p", "class", "wt-text-body-01 wt-break-word");

            return Regex.Replace(description, @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>", "$1");
        }

        protected override string GetProductTags(HtmlDocument productHtml)
        {
            return "";
        }
    }
}

