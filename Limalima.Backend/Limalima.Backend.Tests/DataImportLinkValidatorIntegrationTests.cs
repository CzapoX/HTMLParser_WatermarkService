using Limalima.Backend.Validation;
using Xunit;

namespace Limalima.Backend.Tests
{
    public class DataImportLinkValidatorIntegrationTests
    {
        private readonly IDataImportLinkValidator sut;
        
        private readonly string EtsyLink = "https://www.etsy.com/shop/BusyPuzzle";
        private readonly string PakameraLink = "https://www.pakamera.pl/pookys-world-0_s12297558.htm";
        private readonly string PakameraProductLink = "https://www.pakamera.pl/do-ciala-inne-the-bikini-body-formula-nr2320329.htm";
        private readonly string EtsyProductLink = "https://www.etsy.com/listing/512037342/first-birthday-gift-personalized-cupcake?ref=hp_rv-2";
        private readonly string IncorrectLink = "https://limalima.pl/";


        public DataImportLinkValidatorIntegrationTests()
        {
            sut = new DataImportLinkValidator();
        }

        [Fact]
        public void ShouldPassForEtsyLink()
        {
            var result = sut.ValidateLink(EtsyLink, "etsy");

            Assert.True(result);
        }

        [Fact]
        public void ShouldPassForPakameraLink()
        {
            var result = sut.ValidateLink(PakameraLink, "pakamera");
            
            Assert.True(result);
        }

        [Fact]
        public void ShouldPassForWrongImportSource()
        {
            var result1 = sut.ValidateLink(PakameraLink, "etsy");
            var result2 = sut.ValidateLink(EtsyLink, "pakamera");

            Assert.False(result1);
            Assert.False(result2);
        }

        [Fact]
        public void ShouldPassForWrongLink()
        {
            var result1 = sut.ValidateLink(PakameraProductLink, "pakamera");
            var result2 = sut.ValidateLink(EtsyProductLink, "etsy");
            var result3 = sut.ValidateLink(IncorrectLink, "pakamera");
            var result4 = sut.ValidateLink(IncorrectLink, "etsy");

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.False(result4);
        }

    }
}
