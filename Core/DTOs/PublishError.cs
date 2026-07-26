using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.DTOs
{
    public class PublishError
    {
        public int banner_id { get; set; }
        public Guid guid { get; set; }
        public string? imageName { get; set; }
        public string? bannerType { get; set; }
    }
}
