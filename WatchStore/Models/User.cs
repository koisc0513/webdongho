using System;
using System.Collections.Generic;

namespace WatchStore.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public int? IsAdmin { get; set; }

    public virtual ICollection<LikedProduct> LikedProducts { get; set; } = new List<LikedProduct>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
