namespace CMS.Core.DTOs
{
    public class BlobUploadResponseDto
    {
        public string status { get; set; }
        public bool error { get; set; }
        public Blob blob { get; set; }

        public class Blob
        {
            public string uri { get; set; }
            public string name { get; set; }
            public object contentType { get; set; }
            public object content { get; set; }
        }

    }
}
