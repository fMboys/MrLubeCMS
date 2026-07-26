using System.ComponentModel.DataAnnotations;

namespace CMS.Core.Entities
{
    public class tblquecmsimage
    {
        [Key]
        public int img_queId { get; set; }
        public int? img_id { get; set; }
        public Guid img_guid { get; set; }
        public int? tblquecms_id { get; set; }
        public string? img_description { get; set; }
        public string? img_name { get; set; }
        public string? img_uploadPath { get; set; }
        public DateTime upload_date { get; set; }
        public string? img_user { get; set; }
        public string? Status { get; set; }
        public string? banner_type { get; set; }
        public string? Action { get; set; }
        public DateTime img_createdDate { get; set; }
        public DateTime img_updatedDate { get; set; }
    }

    
}
