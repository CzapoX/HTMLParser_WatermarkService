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

            if (!_linkValidator.ValidateProfileLink(url, importSource))
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