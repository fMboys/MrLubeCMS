namespace CMS.Core.DTOs
{
    public class CMSBaseDto
    {
        public Guid guid { get; set; }
        public int LanguageId { get; set; }
        public string? Title { get; set; }
        public string? ImageName { get; set; }
        public string? ViewPage { get; set; }
        public string? ViewDevice { get; set; }
        public string? LastUser { get; set; }
        public string? ImageStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Hyperlink { get; set; }
    }
}
