using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using WatchStore.Models;
using Microsoft.EntityFrameworkCore;
namespace WatchStore.Models
{
    public class BrandImage
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
    }
}
