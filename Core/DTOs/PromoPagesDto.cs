using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.DTOs
{
    public class PromoPagesDto : CMSBaseDto
    { 
        public int promo_page_id { get; set; }
        public string? Language { get; set; }
        public string? guid { get; set; }
        public int ItemId { get; set; }
        public DateTime date_expired { get; set; }
        public string? frenchTitle { get; set; }
        public string? status { get; set; } 
    }
}
