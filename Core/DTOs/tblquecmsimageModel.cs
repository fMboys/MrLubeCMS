
namespace CMS.Core.DTOs
{
    public class tblquecmsimageModel
    {
        //public tblquecmsimage imgsata;

        //public tblquecmsimageModel(tblquecmsimage imgsata)
        //{
        //    this.imgsata = imgsata;
        //}

        public int img_queId { get; set; }
        public Guid img_guid { get; set; }
        public int img_id { get; set; }
        //public Guid img_guid { get; set; }
        public string img_description { get; set; }
        public string img_name { get; set; }
        public string img_uploadPath { get; set; }
        public DateTime upload_date { get; set; }
        public string img_user { get; set; }

        public string Status { get; set; }
        public string banner_type { get; set; }
        public DateTime img_createdDate { get; set; }
        public DateTime img_updatedDate { get; set; }
    }
}
