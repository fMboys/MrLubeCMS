using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.DTOs
{
    public class PromoImagesDto : CMSBaseDto
    {
        public int PromoImageId { get; set; }
        public string Language { get; set; }
        public string url_key { get; set; }
        
    }
}
