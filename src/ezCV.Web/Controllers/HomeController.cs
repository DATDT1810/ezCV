using ezCV.Web.Models;
using ezCV.Web.Models.CvTemplate;
using ezCV.Web.Services.CvTemplate;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ezCV.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICvTemplateService _cvTemplateService;

        public HomeController(ICvTemplateService cvTemplateService)
        {
            _cvTemplateService = cvTemplateService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var templates = await _cvTemplateService.ListAllAsync();

                return View(templates); 
            }
            catch (Exception ex)
            {
              
                ViewBag.ErrorMessage = "Không thể tải danh sách mẫu CV.";
                return View(new List<CvTemplateResponse>());
            }
        }

    }
}
