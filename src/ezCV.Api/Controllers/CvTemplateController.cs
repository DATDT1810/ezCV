using ezCV.Application.External.Models;
using ezCV.Application.Features.CvTemplate.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ezCV.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CvTemplateController : ControllerBase
    {
        private readonly ICvTemplateService _cvTemplateService;

        public CvTemplateController(ICvTemplateService cvTemplateService)
        {
            _cvTemplateService = cvTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _cvTemplateService.ListAllAsync();
            return Ok(list);
        }


        [HttpGet("{id}", Name = "GetById")]
        public async Task<IActionResult> GetById(int id)
        {
            var template = await _cvTemplateService.GetByIdIntAsync(id);

            // If the template is not found, throw a 404 Not Found exception
            if (template == null)
            {
               NotFound($"Role with ID not found.");
            }

            return Ok(template);
        }

    }
}
