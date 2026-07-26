namespace CMS.Core.DTOs
{
    public class LeftAdDto : CMSBaseDto
    {
        public int Id { get; set; }
        public string? UrlKey { get; set; }
        public string? selectedPages { get; set; }
    }
}
