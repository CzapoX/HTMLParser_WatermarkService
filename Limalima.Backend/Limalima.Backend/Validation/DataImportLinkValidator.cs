namespace Limalima.Backend.Validation
{
    public interface IDataImportLinkValidator
    {
        bool ValidateProfileLink(string link, string importSource);
    }

    public class DataImportLinkValidator : IDataImportLinkValidator
    {
        public bool ValidateProfileLink(string url, string importSource)
        {
            switch (importSource)
            {
                case "etsy":
                    return ValidateEtsyLink(url);
                case "pakamera":
                    return ValidatePakameraLink(url);
                default:
                    return false;
            }
        }

        private bool ValidatePakameraLink(string url)
        {
            return url.Contains("pakamera.pl/") && url.Contains("-0_s");
        }

        private bool ValidateEtsyLink(string url)
        {
            return url.Contains("etsy.com/") && url.Contains("/shop/");
        }
    }
}
