using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.DTOs
{
    public class trackQueModel
    {
        public int tblquecms_id { get; set; }

        public Guid guid { get; set; }
        public int? img_id { get; set; }

        public int? tblquecmsimage_id { get; set; }
        public string? img_name { get; set; }

        public string? img_uploadPath { get; set; }

        public string? banner_type { get; set; }

        public string? action_done { get; set; }
        public string? que_script { get; set; }

        public string? status { get; set; }
        public DateTime img_updatedDate { get; set; }

    }
}
