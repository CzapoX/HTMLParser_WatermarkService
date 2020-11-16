using Limalima.Backend.Components;
using Limalima.Backend.Components.ParsingClient;
using Limalima.Backend.Data;
using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Limalima.Backend.Controllers
{
    public class DataImportController : Controller
    {
        private readonly IWatermarkService _watermarkService;
        private readonly IDataImportLinkValidator _linkValidator;
        private readonly ArtDbContext _context;

        public DataImportController(IWatermarkService watermarkService, IDataImportLinkValidator linkValidator, ArtDbContext context)
        {
            _watermarkService = watermarkService;
            _linkValidator = linkValidator;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ImportData(string url, string importSource)
        {
            IParsingClient _parsingClient = null;

            if (!_linkValidator.ValidateLink(url, importSource))
                return BadRequest();

            switch (importSource)
            {
                case "etsy":
                    _parsingClient = new EtsyParsingClient(_watermarkService);
                    break;
                case "pakamera":
                    _parsingClient = new PakameraParsingClient(_watermarkService);
                    break;
            }

            //TODO dodac walidacje adresu dla konkretnego importera
            //if (!_parsingClient.IsValidUrl(url))
            //{
            //    return RedirectToAction("Index");
            //}

            if (_parsingClient != null)
            {
                var artList = await _parsingClient.GetArtsFromUser(url);
                foreach (var art in artList)
                    await _context.AddAsync(art);
                
                var success = await _context.SaveChangesAsync() > 0;
            }

            return RedirectToAction("Index");
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