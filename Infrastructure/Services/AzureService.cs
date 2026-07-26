using CMS.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace CMS.Infrastructure.Services
{
    [Obsolete]
    public class AzureService
    {
        public readonly IConfiguration _configuration;
        public AzureService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Upload a Form file to Azure FTP server. 
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="uploadDto"></param>
        /// <returns>Boolean</returns>
        /// <exception cref="Exception"></exception>
        public static bool FTPUploader(IFormFile formFile, AzureFTPDto uploadDto)
        {
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadDto.Uri);
                request.Method = WebRequestMethods.Ftp.UploadFile.ToLower();

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(uploadDto.Username.ToString(), uploadDto.Password.ToString());
                request.UseBinary = true;
                request.UsePassive = true;
                request.EnableSsl = true;


                using (Stream stream = request.GetRequestStream())
                {
                    //await formFile.CopyToAsync(stream);
                    formFile.CopyTo(stream);
                    //using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    //{
                    //    string result = $"Upload File Complete, status {response.StatusDescription}";
                    //}                    
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        /// <summary>
        /// Download a file from an Azure FTP server's end point.
        /// </summary>
        /// <param name="azureFTPDto"></param>
        /// <returns>byte[]</returns>
        public static byte[] FTPDownloader(AzureFTPDto azureFTPDto)
        {
            try
            {
                FtpWebRequest reqDownload = (FtpWebRequest)WebRequest.Create(azureFTPDto.Uri);
                reqDownload.Method = WebRequestMethods.Ftp.DownloadFile.ToLower();
                reqDownload.Credentials = new NetworkCredential(azureFTPDto.Username, azureFTPDto.Password);
                reqDownload.KeepAlive = true;
                reqDownload.UseBinary = true;
                reqDownload.UsePassive = true;
                reqDownload.EnableSsl = true;

                //FtpWebResponse response = (FtpWebResponse)reqDownload.GetResponse();
                //Stream responseStream = response.GetResponseStream();

                using (FtpWebResponse response = (FtpWebResponse)reqDownload.GetResponse())
                using (MemoryStream responseBody = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(responseBody);
                    return responseBody.ToArray();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        /// <summary>
        /// Use to upload a bytes[] file to Azure FTP server's specified end point.
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <param name="azureFTPDto"></param>
        /// <returns>Boolean</returns>
        public static bool FTPBytesUploader(byte[] fileBytes, AzureFTPDto azureFTPDto)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(azureFTPDto.Uri);
                request.Credentials = new NetworkCredential(azureFTPDto.Username, azureFTPDto.Password);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.EnableSsl = true;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;

                using (Stream stream = request.GetRequestStream())
                {                    
                    stream.Write(fileBytes, 0, fileBytes.Length); 
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }

        //TODO: for generic FTP calls.
        public static byte[] MakeRequest(string method, string uri, string username, string password, byte[] requestBody = null)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = method;
            //Other request settings (e.g. UsePassive, EnableSsl, Timeout set here)

            if (requestBody != null)
            {
                using (MemoryStream requestMemStream = new MemoryStream(requestBody))
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestMemStream.CopyTo(requestStream);
                }
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (MemoryStream responseBody = new MemoryStream())
            {
                response.GetResponseStream().CopyTo(responseBody);
                return responseBody.ToArray();
            }
        }

        /// <summary>
        /// Delete a file from Azure FTP server.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="uploadDto"></param>
        /// <returns>Boolean result</returns>
        /// <exception cref="Exception"></exception>
        public static bool FTPDeleteFile(string formFile, AzureFTPDto uploadDto)
        {
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadDto.Uri);
                request.Method = WebRequestMethods.Ftp.DeleteFile.ToLower();

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(uploadDto.Username.ToString(), uploadDto.Password.ToString());
                request.UseBinary = true;
                request.UsePassive = true;
                request.EnableSsl = true;


                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Console.WriteLine("Delete status: {0}", response.StatusDescription);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                var msg  =ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name;
                return true;
            }
        }


        public bool uploadImagetoFTPServer(string imgName,string bannerType)
        {
            try
            {
                string FilePath = "";
                //Staging FTP server
                string FileDomain = _configuration.GetSection("FTP_Server").Value;
                if(!string.IsNullOrEmpty(bannerType))
                {
                    if (bannerType == "Homebanner") { FilePath = _configuration.GetSection("BannersPath").Value; }
                    if (bannerType == "ShopTire") { FilePath = _configuration.GetSection("ShopTiresPath").Value; }
                    if (bannerType == "FloatingImage") { FilePath = _configuration.GetSection("FloatingPath").Value; }
                    if (bannerType == "LeftAd") { FilePath = _configuration.GetSection("AdsPath").Value; }
                    if (bannerType == "promo") { FilePath = _configuration.GetSection("PromoImagesPath").Value; }
                    if (bannerType == "coupon") { FilePath = _configuration.GetSection("CouponImagesPath").Value; }
                }
                
                string FtpUser = _configuration.GetSection("FTP_Username").Value;
                string FtpPass = _configuration.GetSection("FTP_Password").Value;
                string filefullPath = FileDomain + FilePath;
                string fullimgpath = filefullPath + "/" + imgName;


                FtpWebRequest downloadrequest = (FtpWebRequest)WebRequest.Create(filefullPath + "/" + imgName);
                downloadrequest.Method = WebRequestMethods.Ftp.DownloadFile;
                // This assumes the FTP site uses anonymous logon.
                downloadrequest.Credentials = new NetworkCredential(FtpUser, FtpPass);
                //request.ContentLength = filebytes.Length;
                downloadrequest.KeepAlive = true;
                //downloadrequest.ContentType = null;
                downloadrequest.UseBinary = true;
                downloadrequest.UsePassive = true;
                //downloadrequest.ContentType = "img/jpg,img/png";
                //request.ServicePoint.ConnectionLimit = filebytes.Length;
                downloadrequest.EnableSsl = true;

                FtpWebResponse response = (FtpWebResponse)downloadrequest.GetResponse();
                Stream responseStream = response.GetResponseStream();




                string ProdFileDomain = _configuration.GetSection("ProdFTP_Server").Value;
                //string ProdFilePath = _configuration.GetSection("ProdBannersPath").Value;
                string ProdFtpUser = _configuration.GetSection("ProdFTP_Username").Value;
                string ProdFtpPass = _configuration.GetSection("ProdFTP_Password").Value;
                string ProdfilefullPath = ProdFileDomain + FilePath;
                string Prodfullimgpath = ProdfilefullPath + "/" + imgName;

                //uploading image on Production Server

                Upload(Prodfullimgpath, ToByteArray(responseStream), ProdFtpUser, ProdFtpPass);
                responseStream.Close();

                //FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(ProdfilefullPath + "/" + imgName);
                //uploadRequest.Credentials = new NetworkCredential(FtpUser, FtpPass);
                //uploadRequest.UseBinary = true;
                //uploadRequest.UsePassive = true;
                //uploadRequest.KeepAlive = true;
                ////uploadRequest.ContentLength = 4096;
                //uploadRequest.EnableSsl = true;

                //uploadRequest.Method = WebRequestMethods.Ftp.UploadFile.ToLower();

                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
        }

        public static Byte[] ToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] chunk = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(chunk, 0, chunk.Length)) > 0)
            {
                ms.Write(chunk, 0, bytesRead);
            }

            return ms.ToArray();
        }

        public static bool Upload(string FileName, byte[] Image, string FtpUsername, string FtpPassword)
        {
            try
            {
                FtpWebRequest clsRequest = (FtpWebRequest)WebRequest.Create(FileName);
                clsRequest.Credentials = new NetworkCredential(FtpUsername, FtpPassword);
                clsRequest.Method = WebRequestMethods.Ftp.UploadFile;
                clsRequest.EnableSsl = true;
                clsRequest.UseBinary = true;
                clsRequest.UsePassive = true;
                clsRequest.KeepAlive = true;
                Stream clsStream = clsRequest.GetRequestStream();
                clsStream.Write(Image, 0, Image.Length);

                clsStream.Close();
                clsStream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FTPDeleteFileProd(string imgName, string bannerType)
        {
            try
            {
                string FilePath = "";
                //Staging FTP server
                string FileDomain = _configuration.GetSection("ProdFTP_Server").Value;
                if (!string.IsNullOrEmpty(bannerType))
                {
                    if (bannerType == "Homebanner") { FilePath = _configuration.GetSection("BannersPath").Value; }
                    if (bannerType == "ShopTire") { FilePath = _configuration.GetSection("ShopTiresPath").Value; }
                    if (bannerType == "floating") { FilePath = _configuration.GetSection("FloatingPath").Value; }
                    if (bannerType == "ads") { FilePath = _configuration.GetSection("AdsPath").Value; }
                    if (bannerType == "promo") { FilePath = _configuration.GetSection("PromoImagesPath").Value; }
                    if (bannerType == "coupon") { FilePath = _configuration.GetSection("CouponImagesPath").Value; }
                }

                string FtpUser = _configuration.GetSection("ProdFTP_Username").Value;
                string FtpPass = _configuration.GetSection("ProdFTP_Password").Value;
                string filefullPath = FileDomain + FilePath;
                string fullimgpath = filefullPath + "/" + imgName;
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filefullPath + "/" + imgName);
                request.Method = WebRequestMethods.Ftp.DeleteFile.ToLower();

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(FtpUser, FtpPass);
                request.UseBinary = true;
                request.UsePassive = true;
                request.EnableSsl = true;


                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Console.WriteLine("Delete status: {0}", response.StatusDescription);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.Message + "Method Name: ";
                return true;
            }
        }

    }
}
