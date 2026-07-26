using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Core.Entities
{
    public class ShopTire : CMSBaseEntity
    {
        public ShopTire()
        {
            List<ShopTire> shopTireList = new List<ShopTire>();
        }
        [Key]
        public int shopTire_id { get; set; }        
        public int store_num { get; set; }

        [NotMapped]
        public string[] SelectedStores { get; set; }
    }

    public class ShopTireImage
    {
        //public List<ShopTire>? shopTires { get; set; }
        public IFormFile shopTireImage { get; set; }
    }
}
