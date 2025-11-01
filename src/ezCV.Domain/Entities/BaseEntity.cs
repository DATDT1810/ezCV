using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ezCV.Domain.Entities
{
    public class BaseEntity
    {
        [NotMapped]
        public virtual long Id { get; set; }
        
        [NotMapped]
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public DateTime UpdatedAt { get; set; }
    }
}