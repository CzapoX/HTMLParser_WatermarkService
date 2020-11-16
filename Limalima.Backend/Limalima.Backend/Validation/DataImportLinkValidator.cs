using System;

namespace Limalima.Backend.Validation
{

    public interface IDataImportLinkValidator
    {
        bool ValidateLink(string link, string importSource);
    }


    public class DataImportLinkValidator : IDataImportLinkValidator
    {
        public bool ValidateLink(string url, string importSource)
        {
            var result = false;

            if (importSource == "etsy")
                result = ValidateEtsyLink(url);
            if (importSource == "pakamera")
                result = ValidatePakameraLink(url);

            return result;
        }

        private bool ValidatePakameraLink(string url)
        {
            return url.Contains("pakamera.pl/") && url.Contains("-0_s");
        }

        private bool ValidateEtsyLink(string url)
        {
            return url.Contains("etsy.com/") && url.Contains("shop/");
        }
    }
}
