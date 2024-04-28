using System;
using System.Collections.Generic;

namespace WatchStore.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; } = null;

   
    public int? BrandId { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<LikedProduct> LikedProducts { get; set; } = new List<LikedProduct>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
