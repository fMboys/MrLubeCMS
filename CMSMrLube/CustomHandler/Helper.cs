using CMS.Core.DTOs;

namespace MrLubeCMS.CustomHandler
{
    public class Helper
    {
        private readonly IConfiguration _configuration;

        public Helper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Create end point Uri for FTP server to upload and download files based on AppSettings values. 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bannerType"></param>
        /// <param name="serverType"></param>
        /// <returns>AzureFTPDto</returns>
        public AzureFTPDto GenerateAzureUri(string fileName, string bannerType, string serverType)
        {
            string endpointFTPS = string.Empty;
            string folderPath = string.Empty;
            string username = string.Empty;
            string password = string.Empty;
            string fullPath = string.Empty;
            string uri = string.Empty;
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            try
            {
                if (bannerType == "ShopTire" && serverType == "Dev")
                {
                    endpointFTPS = _configuration.GetSection("FTP_Server").Value;
                    folderPath = _configuration.GetSection("ShopTiresPath").Value;
                    username = _configuration.GetSection("FTP_Username").Value;
                    password = _configuration.GetSection("FTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "ShopTire" && serverType == "Prod")
                {
                    endpointFTPS = _configuration.GetSection("ProdFTP_Server").Value;
                    folderPath = _configuration.GetSection("ShopTiresPath").Value;
                    username = _configuration.GetSection("ProdFTP_Username").Value;
                    password = _configuration.GetSection("ProdFTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "FloatingImage" && serverType == "stage")
                {
                    endpointFTPS = _configuration.GetSection("FTP_Server").Value;
                    folderPath = _configuration.GetSection("FloatingPath").Value;
                    username = _configuration.GetSection("FTP_Username").Value;
                    password = _configuration.GetSection("FTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "FloatingImage" && serverType == "Prod")
                {
                    endpointFTPS = _configuration.GetSection("ProdFTP_Server").Value;
                    folderPath = _configuration.GetSection("FloatingPath").Value;
                    username = _configuration.GetSection("ProdFTP_Username").Value;
                    password = _configuration.GetSection("ProdFTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "HomeBanner" && serverType == "Stage")
                {
                    endpointFTPS = _configuration.GetSection("FTP_Server").Value;
                    folderPath = _configuration.GetSection("BannersPath").Value;
                    username = _configuration.GetSection("FTP_Username").Value;
                    password = _configuration.GetSection("FTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                } 
                else if (bannerType == "PromoImages" && serverType == "Dev")
                {
                    endpointFTPS = _configuration.GetSection("FTP_Server").Value;
                    folderPath = _configuration.GetSection("PromoImagesPath").Value;
                    username = _configuration.GetSection("FTP_Username").Value;
                    password = _configuration.GetSection("FTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "PromoImages" && serverType == "Prod")
                {
                    endpointFTPS = _configuration.GetSection("ProdFTP_Server").Value;
                    folderPath = _configuration.GetSection("PromoImagesPath").Value;
                    username = _configuration.GetSection("ProdFTP_Username").Value;
                    password = _configuration.GetSection("ProdFTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "LeftAd" && serverType == "stage")
                {
                    endpointFTPS = _configuration.GetSection("FTP_Server").Value;
                    folderPath = _configuration.GetSection("AdsPath").Value;
                    username = _configuration.GetSection("FTP_Username").Value;
                    password = _configuration.GetSection("FTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "LeftAd" && serverType == "Prod")
                {
                    endpointFTPS = _configuration.GetSection("ProdFTP_Server").Value;
                    folderPath = _configuration.GetSection("AdsPath").Value;
                    username = _configuration.GetSection("ProdFTP_Username").Value;
                    password = _configuration.GetSection("ProdFTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "CouponImages" && serverType == "Dev")
                {
                    endpointFTPS = _configuration.GetSection("FTP_Server").Value;
                    folderPath = _configuration.GetSection("CouponImagesPath").Value;
                    username = _configuration.GetSection("FTP_Username").Value;
                    password = _configuration.GetSection("FTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }
                else if (bannerType == "CouponImages" && serverType == "Prod")
                {
                    endpointFTPS = _configuration.GetSection("ProdFTP_Server").Value;
                    folderPath = _configuration.GetSection("CouponImagesPath").Value;
                    username = _configuration.GetSection("ProdFTP_Username").Value;
                    password = _configuration.GetSection("ProdFTP_Password").Value;
                    fullPath = endpointFTPS + folderPath;
                    uri = fullPath + "/" + fileName;

                    azureFTPDto = new AzureFTPDto()
                    {
                        Endpoint = endpointFTPS,
                        FileName = fileName,
                        FolderPath = folderPath,
                        Username = username,
                        Password = password,
                        FullPath = fullPath,
                        Uri = uri
                    };
                }

                return azureFTPDto;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Create string Url endpoint for Blob Storage API method based on spcified condition.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="blobURIDto"></param>
        /// <returns>EndPoint url for a BlobAPI method.</returns>
        public string GenerateBlobStorageUri(string methodName, BlobURIDto blobURIDto)
        {
            string blobMethodName = string.Empty;
            string paramContainer = string.Empty;
            string paramPath = string.Empty;
            string paramFileName = string.Empty;
            string endPoint = string.Empty;

            try
            {
                if (methodName == "Get")
                {
                    //string baseUrl = _configuration.GetSection("BlobStorageAPIUrl").Value;
                    blobMethodName = _configuration.GetSection("BlobStorageGetMethod").Value;
                    paramContainer = _configuration.GetSection("ContainerParamName").Value;

                    endPoint = blobMethodName + paramContainer + blobURIDto.ContainerName;

                    return endPoint;
                }
                else if (methodName == "Upload")
                {
                    blobMethodName = _configuration.GetSection("BlobStorageUploadMethod").Value;
                    paramContainer = _configuration.GetSection("ContainerParamName").Value; 
                    paramPath = _configuration.GetSection("PathParamName").Value;

                    endPoint = blobMethodName + paramContainer + blobURIDto.ContainerName + paramPath + blobURIDto.FolderPath;

                    return endPoint;
                }
                else if (methodName == "Delete")
                {
                    blobMethodName = _configuration.GetSection("BlobStorageDeleteMethod").Value;
                    paramContainer = _configuration.GetSection("ContainerParamName").Value;
                    paramFileName = _configuration.GetSection("ParamFileName").Value;
                    paramPath = _configuration.GetSection("PathParamName").Value;

                    endPoint = blobMethodName + paramContainer + blobURIDto.ContainerName + paramFileName + blobURIDto.FileName + paramPath + blobURIDto.FolderPath;

                    return endPoint;
                }
                else if (methodName == "Download")
                {
                    blobMethodName = _configuration.GetSection("BlobStorageDownloadMethod").Value;
                    paramContainer = _configuration.GetSection("ContainerParamName").Value;
                    paramFileName = _configuration.GetSection("ParamFileName").Value;
                    paramPath = _configuration.GetSection("PathParamName").Value;

                    endPoint = blobMethodName + paramContainer + blobURIDto.ContainerName + paramFileName + blobURIDto.FileName + paramPath + blobURIDto.FolderPath;

                    return endPoint;
                }

                return endPoint;
                
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }
    }
}
