namespace CMS.Core.DTOs
{
    public class TrackingQueuesDto
    {
        public int? FloatingImageID { get; set; }
        public int? LeftAdID { get; set; }
        public Guid guid { get; set; }  
        public int? ImageId { get; set; }
        public Guid ImageGUID { get; set; }
        public string? ImageName { get; set; }
        public string? ImageStatus { get; set; }
        public int? ImageQueueId { get; set; }
        public string? ImageQueueStatus { get; set; }
        public string? ImageUploadPath { get; set; }
        public int? ScriptQueueId { get; set; }
        public string? ScriptQueueStatus { get; set; }
        public string? BannerType { get; set; }
    }
}
