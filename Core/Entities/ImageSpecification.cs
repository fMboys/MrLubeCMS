
namespace CMS.Core.Entities
{
    public class ImageSpecification
    {
        public int Id { get; set; }
        public string? image_type { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string view_device { get; set; }
        public string banner_type { get; set; }
    }
}
