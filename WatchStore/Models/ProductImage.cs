using System;
using System.Collections.Generic;

namespace WatchStore.Models;

public partial class ProductImage
{
    public string Id { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int ProductId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
