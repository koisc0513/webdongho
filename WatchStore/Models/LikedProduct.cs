using System;
using System.Collections.Generic;

namespace WatchStore.Models;

public partial class LikedProduct
{
    public int LikedProductId { get; set; }

    public int? ProductId { get; set; }

    public int? UserId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
