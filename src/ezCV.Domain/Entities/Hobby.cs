using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Hobby
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? HobbyName { get; set; }

    public virtual User User { get; set; } = null!;
}
