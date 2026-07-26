using Microsoft.AspNetCore.Http;

namespace CMS.Core.DTOs
{
    public class BlobURIDto
    {
        public string? MethodName { get; set; }
        public string? ContainerName { get; set; }
        public string? FolderPath { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public IFormFile? FormFile { get; set; }
        public byte[]? FileBytes { get; set; }
    }
}
