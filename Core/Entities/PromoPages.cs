using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Entities
{
    [Table("promo_pages")] 
    public class PromoPages  
    { 
        [Key]
        public int promo_page_id { get; set; }
        public Guid guid { get; set; }
        public int language_id { get; set; }
        public int itemId { get; set; }
        public string? url_Key { get; set; }
        public string? title { get; set; }
        public DateTime date_expired { get; set; }
        public string? last_user { get; set; }
        public string? status { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_updated { get; set; }
        [NotMapped]
        public string? frenchTitle { get; set; } 
       
    }
}
