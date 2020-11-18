using Limalima.Backend.Validation;
using Xunit;

namespace Limalima.Backend.Tests
{
    public class DataImportLinkValidatorIntegrationTests
    {
        private readonly IDataImportLinkValidator sut;
        
        private const string EtsyLink = "https://www.etsy.com/shop/BusyPuzzle";
        private const string PakameraLink = "https://www.pakamera.pl/pookys-world-0_s12297558.htm";
        private const string PakameraProductLink = "https://www.pakamera.pl/do-ciala-inne-the-bikini-body-formula-nr2320329.htm";
        private const string EtsyProductLink = "https://www.etsy.com/listing/512037342/first-birthday-gift-personalized-cupcake?ref=hp_rv-2";
        private const string IncorrectLink = "https://limalima.pl/";


        public DataImportLinkValidatorIntegrationTests()
        {
            sut = new DataImportLinkValidator();
        }

        [Fact]
        public void ShouldPassForEtsyLink()
        {
            var result = sut.ValidateProfileLink(EtsyLink, "etsy");

            Assert.True(result);
        }

        [Fact]
        public void ShouldPassForPakameraLink()
        {
            var result = sut.ValidateProfileLink(PakameraLink, "pakamera");
            
            Assert.True(result);
        }

        [Fact]
        public void ShouldPassForWrongImportSource()
        {
            var result1 = sut.ValidateProfileLink(PakameraLink, "etsy");
            var result2 = sut.ValidateProfileLink(EtsyLink, "pakamera");

            Assert.False(result1);
            Assert.False(result2);
        }

        [Theory]
        [InlineData(PakameraProductLink, "pakamera")]
        [InlineData(EtsyProductLink, "etsy")]
        public void ShouldFailBecauseOfProvidingProductLinkinsteadOfProfile(string link, string source)
        {
            var result1 = sut.ValidateProfileLink(link, source);

            Assert.False(result1);
        }

        [Fact]
        public void ShouldPassForWrongLink()
        {
            var result1 = sut.ValidateProfileLink(IncorrectLink, "pakamera");
            var result2 = sut.ValidateProfileLink(IncorrectLink, "etsy");

            Assert.False(result1);
            Assert.False(result2);
        }
    }
}
