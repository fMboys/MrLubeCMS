using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.DTOs
{
    public class ShopTireAllDto : CMSBaseDto
    {
        public int? ShopTireId { get; set; }
        //public int LanguageId { get; set; }
        public string? Language { get; set; }
        public int? StoreNumber { get; set; }
        public string? Stores { get; set; }
    }
}
