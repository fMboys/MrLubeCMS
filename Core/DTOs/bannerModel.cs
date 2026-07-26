using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CMS.Core.DTOs
{
    public class bannerModel
    {
        public Guid guid{ get; set; }
        public int banner_id { get; set; }
        public int language_id { get; set; }
        [Required(ErrorMessage = "Please enter title")]
        [StringLength(100)]
        public string? title { get; set; }

        [Required(ErrorMessage = "Please upload image")]
        [Display(Name = "Image")]
        public string? image { get; set; }
        public string? page { get; set; }
        public string? view { get; set; }
        public string? status { get; set; }
        public string? last_user { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_updated { get; set; }
        public string? ad_hyperlink { get; set; }
        [TempData]
        public int imgId { get; set; }
    }
}
