using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Role : BaseEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
