using live_share_code_demo.Models;
using live_share_code_demo.services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace live_share_code_demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICompilationService compilationService;

        public HomeController(ILogger<HomeController> logger,
                              ICompilationService _compilationService)
        {
            _logger = logger;
            compilationService = _compilationService;
        }
        [HttpPost]
        public IActionResult RunCode(string code)
        {
            var results = new List<string>();
            #region Run The 3 TestCases
            var result1 = compilationService.CompileAndRun(code, "Add", "1,2", "3");
            var result2 = compilationService.CompileAndRun(code, "Add", "4,5", "9");
            var result3 = compilationService.CompileAndRun(code, "Add", "0,-1", "-1");
            #endregion

            results.Add(result1);
            results.Add(result2);
            results.Add(result3);
            return Json(new { result = results });

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
