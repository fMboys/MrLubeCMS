using System.ComponentModel.DataAnnotations;

namespace CMS.Core.Entities
{
    public class SubMenu
    {
        [Key]
        public int sub_menu_id { get; set; }
        public int language_id { get; set; }
        public int menu_id { get; set; }
        public int item_id { get; set; }
        public string? url_key { get; set; }
        public string? title { get; set; }
        public string? overview { get; set; }
        public string? description { get; set; }
        public string? image { get; set; }
        public int parent_id { get; set; }
        public string last_user { get; set; }
        public string status { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
    }
}
