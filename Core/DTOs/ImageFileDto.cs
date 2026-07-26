using Microsoft.AspNetCore.Http;

namespace CMS.Core.DTOs
{
    public class ImageFileDto
    {
        public IFormFile Image { get; set; }
    }
}
