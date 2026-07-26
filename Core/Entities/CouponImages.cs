using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Entities
{
    [Table("coupon_images")]
    public class CouponImages : CMSBaseEntity
    {
        public CouponImages()
        {
            List<CouponImages> couponImageslst = new List<CouponImages>();
        }
        [Key]
        public int coupon_image_id { get; set; }
        [NotMapped]
        public string? page { get; set; }
        public string? url_key { get; set; } 
        [NotMapped]
        public string? ad_hyperlink { get; set; }

    }
    public class CouponImage
    {
        public IFormFile couponImage { get; set; }
    }
}
