using CMS.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection;

namespace CMS.Infrastructure.Services
{
    public static class BlobStorageAPIService
    {
        public static IConfiguration _configuration;
        private static IHttpClientFactory _httpCF;

        public static void BlobStorageAPIServiceConfigurations(IConfiguration configuration, IHttpClientFactory httpCF) 
        {
            _configuration = configuration;
            _httpCF = httpCF;
        }

        /// <summary>
        /// Retrieve list of all images from specified Blob container.Note: Parameter 'endPoint' must be a url string that contains
        /// all the information of targeted Blob API i.e. Method name, Parameters and Values of that Blob API.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns>Task<List<ImageFileDto>></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<ImageFileDto>> GetAll(string endPoint)
        {
            try
            {
                List<ImageFileDto> imageFilesDto = new List<ImageFileDto>();

                HttpClient httpClient = _httpCF.CreateClient("blobClient");
                 
                HttpResponseMessage response = await httpClient.GetAsync(endPoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    imageFilesDto = JsonConvert.DeserializeObject<List<ImageFileDto>>(responseBody);

                    if (imageFilesDto.Count > 0 && imageFilesDto != null)
                    {
                        return imageFilesDto;
                    }
                }

                return imageFilesDto;
            }
            catch (Exception ex)
            {
               throw new Exception (ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName);
            }
        }

        /// <summary>
        /// Use to upload a form file on specified Blob container. This method currently only works with FormFiles.
        /// Note: Parameter 'endPoint' must be a url string that contains all the information of targeted Blob API 
        /// i.e. Method name, Parameters and Values of that Blob API.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="blobURIDto"></param>
        /// <returns>Returns response of BlobContainer in BlobUploadResponseDto</returns>
        /// <exception cref="Exception"></exception>
        public static async Task<BlobUploadResponseDto> BlobFileUploader(string endPoint, BlobURIDto blobURIDto)
        {
            BlobUploadResponseDto uploadResponseDto = new BlobUploadResponseDto();
            try
            {
                HttpClient httpClient = _httpCF.CreateClient("blobClient");

                using(HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("POST"), endPoint))
                {
                    //httpClient.DefaultRequestHeaders.Authorization =
                    //               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    request.Headers.TryAddWithoutValidation("accept", "*/*");

                    MultipartFormDataContent multipartContent = new MultipartFormDataContent();

                    //To allow specific file types.
                    var extension = System.IO.Path.GetExtension(blobURIDto.FileName);
                    var imgext = "";
                    if (extension == ".png")
                    { imgext = "image/png"; }
                    else if (extension == ".jpg")
                    {
                        imgext = "image/jpg";
                    }
                    else if (extension == ".jpeg")
                    {
                        imgext = "image/jpeg";
                    }
                    else if (extension == ".gif")
                    {
                        imgext = "image/gif";
                    }
                    else { throw new Exception("Can't send this type of file"); }
                    //var fileType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    //{
                    //    [".png"] = "image/png",
                    //    [".jpg"] = "image/jpg"
                    //    //[".gif"] = "image/gif"
                    //};
                    //if (!fileType.TryGetValue(extension, out string contentType))
                    //{
                    //    throw new Exception("Can't send this type of file");
                    //}

                    //TODO:Use code based on file type?? Check for byte[] and FormFile?
                    //if the file in byte array, use this code. Hint it is better to use form file code.
                    //var file1 = new ByteArrayContent(imageFile);
                    //file1.Headers.Add("Content-Type", "image/jpeg");
                    //multipartContent.Add(file1, "file", "testing2.jpg");

                    StreamContent fileContent = new StreamContent(blobURIDto.FormFile.OpenReadStream());
                    //Can code for specific file types.
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(imgext);
                    multipartContent.Add(fileContent, "file", blobURIDto.FileName);
                    request.Content = multipartContent;

                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        uploadResponseDto = JsonConvert.DeserializeObject<BlobUploadResponseDto>(result);

                        if (uploadResponseDto.status != null)
                        {
                            return uploadResponseDto;
                        }
                    }
                }
                return uploadResponseDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName);
            }
        }

        /// <summary>
        /// This method delete a file from Blob container. Note: Parameter 'endPoint' must be a url string that contains
        /// all the information of targeted Blob API i.e. Method name, Parameters and Values of that Blob API.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns>Status string</returns>
        /// <exception cref="Exception"></exception>
        public static async Task<string> BlobDeleteFile(string endPoint)
        {
            string result = string.Empty;

            try
            {
                HttpClient httpClient = _httpCF.CreateClient("blobClient");
                 
                HttpResponseMessage response = await httpClient.DeleteAsync(endPoint);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                return result;
                //throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName);
            }
        }

        /// <summary>
        /// Download a file from a specified blob container. Note: Parameter 'endPoint' must be a url string that contains
        /// all the information of targeted Blob API i.e. Method name, Parameters and Values of that Blob API.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<BlobDownloadResponseDto> BlobFileDownloader(string endPoint)
        {
            BlobDownloadResponseDto downloadResponseDto = null;
            try
            {
                HttpClient httpClient = _httpCF.CreateClient("blobClient");
                 
                HttpResponseMessage response = await httpClient.GetAsync(endPoint);

                if (response.IsSuccessStatusCode)
                {
                    downloadResponseDto = new BlobDownloadResponseDto()
                    {
                        Name = response.Content.Headers.ContentDisposition.Name,
                        FileName = response.Content.Headers.ContentDisposition.FileName,
                        ContentType = response.Content.Headers.ContentType.MediaType,
                        Content = await response.Content.ReadAsStreamAsync()
                    };

                    if (downloadResponseDto != null)
                        return downloadResponseDto;
                }

                return downloadResponseDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName);
            }
        }
        public static bool isEmptyOrInvalid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }
            var jwtToken = new JwtSecurityToken(token);
            return (jwtToken == null) || (jwtToken.ValidFrom > DateTime.UtcNow) || (jwtToken.ValidTo < DateTime.UtcNow);
        }
    }
}
