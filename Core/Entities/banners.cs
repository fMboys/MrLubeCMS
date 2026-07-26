using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CMS.Core.Entities
{
    public class banners : CMSBaseEntity
    {
        public banners()
        {
            List<banners> BannerList = new List<banners>();
        }
        [Key]
        public int banner_id { get; set; }
        //public int language_id { get; set; }
        //[Required(ErrorMessage = "Please enter title")]
        //[StringLength(100)]
        //public string? title { get; set; }

        //[Required(ErrorMessage = "Please upload image")]
        //[Display(Name = "Image")]
        //public string? image { get; set; }
        //[Required(ErrorMessage = "Please Select the Page")]
        //[Display(Name = "page")]
        //public string? page { get; set; }
        //[Required(ErrorMessage = "Please Select the View-Desktop/Mobile")]
        //[Display(Name = "View")]
        //public string? view { get; set; }
        //public string? status { get; set; }
        //public string? last_user { get; set; }
        //public DateTime? date_created { get; set; }
        //public DateTime? date_updated { get; set; }
        //[Required(ErrorMessage = "Please enter Hyperlink")]
        //public string? ad_hyperlink { get; set; }
    }

    public class bannerData
    {
        //public List<banners>? Banner { get; set; }

        public IFormFile BannerImage { get; set; }
    }
}
