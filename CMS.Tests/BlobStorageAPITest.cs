using Castle.Components.DictionaryAdapter.Xml;
using CMS.Core.DTOs;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MrLubeCMS.CustomHandler;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http.Headers;

namespace CMS.Tests
{

    public class BlobStorageAPITest
    {        
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        //static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// This method consume api Get method of AzureBlobStorageAPI project to test its functionality.
        /// </summary>
        /// <returns></returns>
        [Fact]       
        static async Task BlobStorageGetTest()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                
                HttpClient httpClient = new HttpClient();
                List<ImageFileDto> imagesDto = new List<ImageFileDto>();

                //StringContent stringContent = new StringContent(JsonConvert.SerializeObject("dotcom-staging"), System.Text.Encoding.UTF8, "application/json");
                //client.BaseAddress = new Uri("https://localhost:7158/api/Storage?");
                //using HttpResponseMessage response = await client.GetAsync("https://localhost:7158/api/Storage?blobContainerName=dotcom-staging");
                //var response = await client.GetAsync(request);

                //This works fine
                //RestClient client = new RestClient("https://localhost:7158/api/");
                //RestRequest restRequest = new RestRequest("Storage/Get", RestSharp.Method.Get);
                //restRequest.AddParameter("blobContainerName", "dotcom-staging");
                //var response = await client.ExecuteAsync(restRequest);
                string method = "Get?";
                string parameter = "blobContainerName=";
                string value = "dotcom-staging";
                string endpoint = method + parameter + value;

                httpClient.BaseAddress = new Uri("https://localhost:7158/api/Storage/");
                var resp = await httpClient.GetAsync(endpoint);
                //var req = new HttpRequestMessage(HttpMethod.Get, "Storage/Get");
                //req.Headers.Add("Referer", "login.microsoftonline.com");
                //req.Headers.Add("Accept", "application/x-www-form-urlencoded");
                //req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                // This is the important part:
                //req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                //{
                //    { "blobContainerName", "dotcom-staging" }
                //    //{ "client_id", "6e97fc60-xxxxxxxxx-a9bxxxxxb2d" },
                //    //{ "client_secret", "4lSxxxxxxxxxxxmqF4Q" },
                //    //{ "resource", "https://graph.microsoft.com" },
                //    //{ "username", "xxxx@xxxxx.onmicrosoft.com" },
                //    //{ "password", "xxxxxxxxxxxxx" }
                //});

                //HttpResponseMessage resp = await httpClient.SendAsync(req);


                //var query = new Dictionary<string, string>()
                //{
                //    ["blobContainerName"] = "dotcom-staging"
                //};

                //var uri = QueryHelpers.AddQueryString("https://localhost:7158/api/Storage/Get", query);

                //var resp = await httpClient.GetAsync(uri);


                string responseBody = await resp.Content.ReadAsStringAsync();
                
                imagesDto = JsonConvert.DeserializeObject<List<ImageFileDto>>(responseBody);

                //BlobStorageUploadFileTest(imagesDto[0]);

                //response.EnsureSuccessStatusCode();
                //string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                //Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        [Fact]
        static async Task BlobStorageDeleteTest()
        {
            try
            {
                //HttpClient httpClient = new HttpClient();

                //string method = "Delete?";
                //string parameter = "storageContainerName=";
                //string value = "devimages"; 
                //string parameter2 = "&filename=";
                //string value2 = "6.jpg"; 
                //string parameter3 = "&path=";
                //string value3 = string.Empty;
                //string endpoint = method + parameter+ value+ parameter2+ value2+parameter3+value3;

                //httpClient.BaseAddress = new Uri("https://localhost:7158/api/Storage/");
                //httpClient.DefaultRequestHeaders.Accept.Clear();
                //HttpResponseMessage response = await httpClient.DeleteAsync(endpoint);


                RestClient client = new RestClient("https://localhost:7158/api/");
                RestRequest restRequest = new RestRequest("Storage/Delete", RestSharp.Method.Delete);
                restRequest.AddParameter("storageContainerName", "devimages");
                restRequest.AddParameter("filename", "6.jpg");
                restRequest.AddParameter("path", string.Empty);
                var response = await client.ExecuteAsync(restRequest);


            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [Fact]
        static async Task BlobStorageDownloadTest()
        {
            try
            {
                RestClient client = new RestClient("https://localhost:7158/api/");
                RestRequest restRequest = new RestRequest("Storage/Download", RestSharp.Method.Get);
                restRequest.AddParameter("storageContainerName", "devimages");
                restRequest.AddParameter("filename", "shopmob.jpg");
                restRequest.AddParameter("path", string.Empty);
                var response = await client.ExecuteAsync(restRequest);

                //TODO:Try to convert in response dto
                //BlobDownloadResponseDto responseDto = JsonConvert.DeserializeObject<BlobDownloadResponseDto>(response);
                string result = response.Content.ToString();
                
                byte[] imageBytes = response.RawBytes;
                var data = response.ContentType;
                var name = response.GetComponentType();


                BlobStorageUploadFileTest(imageBytes);
               

                //byte[] imageBytes = JsonConvert.DeserializeObject<byte[]>(response.Content);

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        //[Fact]
        static async Task BlobStorageUploadFileTest(byte[] imageFile)
        {
            try
            {
                //HttpClient httpClient = new HttpClient();
                //ImageFileDto file = new ImageFileDto();


                //var stream = new MemoryStream(imageFile);
                //IFormFile formFile = new FormFile(stream, 0, imageFile.Length, "name", "fileName");

                //RestClient client = new RestClient("https://localhost:7158/api/");
                //RestRequest restRequest = new RestRequest("Storage/Upload", RestSharp.Method.Post);
                //restRequest.AddParameter("blobContainerName", "devimages");

                //restRequest.AddFile("file", formFile, "testfile");
                //restRequest.AddParameter("path", string.Empty);
                //var response = await client.ExecuteAsync(restRequest);

                


                //using (var client = new HttpClient())
                //{
                //    using (var content = new MultipartFormDataContent())
                //    {
                //        var values = new[]
                //        {
                //            new KeyValuePair<string, string>("Foo", "Bar"),
                //            new KeyValuePair<string, string>("More", "Less"),
                //        };

                //        foreach (var keyValuePair in values)
                //        {
                //            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                //        }

                //        //var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fileName));

                //        var fileContent = new ByteArrayContent(imageFile);
                //        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                //        {
                //            FileName = "Foo.jpg"
                //        };
                //        content.Add(fileContent);

                //        var requestUri = "https://localhost:7158/api/Storage/Upload";
                //        var result = client.PostAsync(requestUri, content).Result;
                //    }
                //}


                string url = "https://localhost:7158/api/Storage/Upload";
                //string endpoint = url + "?storageContainerName=devimages&path=";
                //const string filePath = @"C:\Path\To\File.png";

                using (var httpClient = new HttpClient())
                {
                    //using (var form = new MultipartFormDataContent())
                    //{
                    //    //using (var fs = File.OpenRead(filePath))
                    //    //{
                    //    using (var streamContent = new StreamContent(stream))
                    //    {
                    //        using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                    //        {
                    //            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                    //            // "file" parameter name should be the same as the server side input parameter name

                    //            form.Add(fileContent, "file", "testfile.jpg");
                    //            var response = await httpClient.PostAsync(url, form);
                    //        }
                    //    }
                    //    //}
                    //}

                    var stream = new MemoryStream(imageFile);
                    //string exe = Path.GetExtension(stream.);
                    IFormFile formFile = new FormFile(stream, 0, imageFile.Length, "Name", "fileName.jpg");

                    //var extension = System.IO.Path.GetExtension(formFile.FileName);

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

                    //Also working...
                    //using (var multipartFormContent = new MultipartFormDataContent())
                    //{
                    //    //Add other fields
                    //    multipartFormContent.Add(new StringContent("devimages"), name: "storageContainerName");
                    //    multipartFormContent.Add(new StringContent(""), name: "path");



                    //    //Add the file

                    //    //var fileStreamContent = new StreamContent(stream);
                    //    //fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                    //    //fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
                    //    //multipartFormContent.Add(fileStreamContent, name: "file", fileName: "testfile.jpg");
                    //    StreamContent fileContent = new StreamContent(formFile.OpenReadStream());
                    //    //ByteArrayContent fileContent = new ByteArrayContent(imageFile);
                    //    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                    //    multipartFormContent.Add(fileContent, name: "file", fileName: "testfiletype4.jpg");

                    //    //using (var streamContent = new StreamContent(stream))
                    //    //{
                    //    //    using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                    //    //    {
                    //    //        //fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                    //    //        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

                    //    //        multipartFormContent.Add(fileContent, name: "file", fileName: "testfiletype.jpg");
                    //    //        // "file" parameter name should be the same as the server side input parameter name
                    //    //        //multipartFormContent.Add(fileContent, "file", "testfile.jpg");

                    //    //        var response = await httpClient.PostAsync(url, multipartFormContent);

                    //    //        //var response = await httpClient.PostAsync(url, form);
                    //    //    }
                    //    //}


                    //    //Send it
                    //    HttpResponseMessage response = await httpClient.PostAsync(url, multipartFormContent);
                    //    response.EnsureSuccessStatusCode();
                    //    //return await response.Content.ReadAsStringAsync();
                    //}

                    string BlobStorageName = "devimages";
                    string BlobStoragePath = "";
                    string urll = "https://localhost:7158/api/" + "Storage/Upload?blobContainerName=" + BlobStorageName +
                                                                                    "&path=" + BlobStoragePath;
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), urll))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "*/*");
                        var multipartContent = new MultipartFormDataContent();

                        //This take image in byte[]
                        //var file1 = new ByteArrayContent(imageFile);
                        //file1.Headers.Add("Content-Type", "image/jpeg");
                        //multipartContent.Add(file1, "file", "testing2.jpg");

                        //This take image in form file. both works fine.
                        StreamContent fileContent = new StreamContent(formFile.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                        multipartContent.Add(fileContent, "file", "testing2.jpg");
                        request.Content = multipartContent;
                        var response = httpClient.SendAsync(request).Result;
                        response.EnsureSuccessStatusCode();
                    }
                    
                }



            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
