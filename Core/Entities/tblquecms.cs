using System.ComponentModel.DataAnnotations;

namespace CMS.Core.Entities
{
    public class tblquecms
    {
        [Key]
        public int que_id { get; set; }
        public Guid img_guid { get; set; }
        public int img_id { get; set; }
        //public Guid img_guid { get; set; }
        public string que_desc { get; set; }
        public string que_script { get; set; }
        public DateTime que_date { get; set; }
        public string que_user { get; set; }
        public string Status { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
    }
}
