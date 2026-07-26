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
    [Table("promo_images")]
    public class PromoImages : CMSBaseEntity
    {
        public PromoImages()
        {
            List<PromoImages> promoImageslst = new List<PromoImages>();
        }
        [Key]
        public int promo_image_id { get; set; }
        [NotMapped]
        public string? page { get; set; }
        public string? url_key { get; set; }
        public string? promo_hyperlink { get; set; }
        [NotMapped]
        public string? ad_hyperlink { get; set; }

    }
    public class PromoImage
    { 
        public IFormFile promoImage { get; set; }
    }
}
