namespace CMS.Core.DTOs
{
    public class GeneralCMSDto : CMSBaseDto
    {
        public int ID { get; set; }
        public int HomeBannerID { get; set; }
        public int ShopTireID { get; set; }
        public int FloatingImageID { get; set; }
        public int LeftAdID { get; set; }
        public int PromoImageID { get; set; }
        public int PromoPageID { get; set; }
        public int StoreNumber { get; set; }
        public string? Language { get; set; }
        public string? UrlKey { get; set; }
        public string? SelectedPages { get; set; }
        public int ItemID { get; set; }
        public DateTime DateExpired { get; set; }
        public string? FrenchTitle { get; set; }
        public string? Status { get; set; }
        public string? BannerType { get; set; }
    }
}
