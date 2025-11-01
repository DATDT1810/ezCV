using ezCV.Application.Repositories;
using ezCV.Domain.Entities;
using ezCV.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Infrastructure.Repositories;
    public class CvTemplateRepository : BaseRepository<CvTemplate>, ICvTemplateRepository
    {
        public CvTemplateRepository(ApplicationDbContext context) : base(context)
        {
        }


    }
