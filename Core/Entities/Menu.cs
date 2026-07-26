using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Entities
{
    public class Menu
    {
        [Key]
        public int id { get; set; }
        public int menu_id { get; set; }
        public int language_id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string? last_user { get; set; }
        public string? status { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
    }
}
