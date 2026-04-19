using Microsoft.AspNetCore.Mvc;
using FileIntegrityMonitor.Models;
using FileIntegrityMonitor.Services;

namespace FileIntegrityMonitor.Controllers
{
    public class ScansController : Controller
    {
        private readonly ScanService _scanService;
        private readonly ComparisonService _comparisonService;

        public ScansController(ScanService scanService, ComparisonService comparisonService)
        {
            _scanService = scanService;
            _comparisonService = comparisonService;
        }

        public IActionResult Index()
        {
            var scans = _scanService.GetAllScans();

            // Build summary statistics for the dashboard/home page.
            var model = new ScanSummaryViewModel
            {
                TotalScans = scans.Count,
                TotalBaselines = scans.Count(s => s.IsBaseline),
                TotalRegularScans = scans.Count(s => !s.IsBaseline),
                RecentScans = scans.Take(10).ToList()
            };

            return View("~/Views/Scans/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult CreateBaseline()
        {
            return View("~/Views/Scans/CreateBaseline.cshtml", new CreateScanViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBaseline(CreateScanViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Scans/CreateBaseline.cshtml", model);

            try
            {
                long scanId = _scanService.RunScan(model.Name, model.RootPath, true);
                TempData["Success"] = "Baseline created successfully.";
                return RedirectToAction(nameof(Details), new { id = scanId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("~/Views/Scans/CreateBaseline.cshtml", model);
            }
        }

        [HttpGet]
        public IActionResult RunScan()
        {
            return View("~/Views/Scans/RunScan.cshtml", new CreateScanViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RunScan(CreateScanViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Scans/RunScan.cshtml", model);

            try
            {
                // A regular scan must have an existing baseline for the same root path
                // so meaningful comparison results can be generated.
                var baseline = _scanService.GetLatestBaselineForPath(model.RootPath);

                if (baseline == null)
                {
                    ModelState.AddModelError(string.Empty, "No baseline exists for this folder path. Create a baseline first.");
                    return View("~/Views/Scans/RunScan.cshtml", model);
                }

                long scanId = _scanService.RunScan(model.Name, model.RootPath, false);

                TempData["Success"] = "Regular scan completed successfully.";
                return RedirectToAction(nameof(Compare), new
                {
                    baselineId = baseline.ScanId,
                    currentId = scanId
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public IActionResult Details(long id)
        {
            var scan = _scanService.GetScanById(id);
            if (scan == null)
                return NotFound();

            var model = new ScanDetailsViewModel
            {
                Scan = scan,
                Files = _scanService.GetFilesByScanId(id)
            };

            if (!scan.IsBaseline)
            {
                // For regular scans, try to load the latest matching baseline so
                // the details page can also show comparison results.
                var baseline = _scanService.GetLatestBaselineForPath(scan.RootPath);

                if (baseline != null && baseline.ScanId != scan.ScanId)
                {
                    var compareModel = _comparisonService.CompareScans(baseline.ScanId, scan.ScanId);
                    model.BaselineScan = baseline;
                    model.ComparisonRows = compareModel.Rows;
                }
            }

            return View("~/Views/Scans/Details.cshtml", model);
        }

        [HttpGet]
        public IActionResult Compare(long baselineId, long currentId)
        {
            try
            {
                var model = _comparisonService.CompareScans(baselineId, currentId);
                return View("~/Views/Scans/Compare.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            var scan = _scanService.GetScanById(id);
            if (scan == null)
                return NotFound();

            return View("~/Views/Scans/Delete.cshtml", scan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            // This handles the POST from the delete confirmation form while
            // still mapping to the "Delete" action name in routing.
            _scanService.DeleteScan(id);
            TempData["Success"] = "Scan deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}