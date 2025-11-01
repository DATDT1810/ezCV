using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvTemplate.Models
{
    public class CvTemplateResponse
    {
        public int Id { get; set; }

        public string? TemplateName { get; set; }

        public string? PreviewImageUrl { get; set; }

        public bool? IsActive { get; set; }
        public string ViewPath { get; set; }
    }
}
