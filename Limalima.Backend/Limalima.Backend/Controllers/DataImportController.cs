using Limalima.Backend.Components.ParsingClient;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Limalima.Backend.Controllers
{
    public class DataImportController : Controller
    {
        private readonly IParsingClient _parsingClient;

        public DataImportController(IParsingClient parsingClient)
        {
            _parsingClient = parsingClient;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ImportData(string url)
        {

            var artList = await _parsingClient.GetArtsFromUser(url);

            return Ok();
        }
    }
}

//REF 2020-11-07
/*
     <a
        class="display-inline-block listing-link"
        data-listing-id="697558243"
        data-palette-listing-image
        href="https://www.etsy.com/pl/listing/697558243/custom-letter-necklace-vote-necklace?ref=shop_home_feat_1&pro=1"

        target="etsy.697558243"
        title="Custom Letter Necklace - Voxte Necklace - Initial Necklace - Mother&#39;s Day Gift - Personalized Jewelry - Bridesmaid Gift - #LTTRF149"
    >
 */