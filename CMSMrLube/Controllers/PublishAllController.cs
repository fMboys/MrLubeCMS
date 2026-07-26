using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MrLubeCMS.CustomHandler;
using MrLubeCMS.ViewModels;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Policy;
using System.Xml.Linq;
using System.Web;
using System.Text;

namespace MrLubeCMS.Controllers
{
    //[Authorize(Roles = "admin")]
    [Authorize]
    public class PublishAllController : Controller
    {
        private readonly ILogger<PublishAllController> _logger;
        private readonly CMSDbContext _context;
        private readonly IPublishService _repo;
        private readonly AzureService _repoPublish;
        private readonly ILeftAdRepository _leftAdRepo;
        public readonly IConfiguration _configuration;

        public PublishAllController(ILogger<PublishAllController> logger, CMSDbContext context, IPublishService repo, IConfiguration configuration,
            AzureService repoPublish, ILeftAdRepository leftAdRepo)
        {
            _logger = logger;
            _context = context;
            _repo = repo;
            _configuration = configuration;
            _repoPublish = repoPublish;
            _leftAdRepo = leftAdRepo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Publish()
        {
            //var tblque = _repo.GetAllPublish();
            ViewBag.tblque = "this is Publish";
            return View();
        }

        public ActionResult PublishList(int? id, string? view, string? lang, string? Status, string? image)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                bool Edit = true;
                bool View = true;

                //if (User.IsInRole("admin"))
                //{
                //    Edit = true;
                //    View = true;
                //}
                //if (User.IsInRole("admin"))
                //{
                //    View = true;
                //}
                int start = Convert.ToInt32(Request.Form["start"]);
                int length = Convert.ToInt32(Request.Form["length"]);
                string searchValue = Request.Form["search[value]"];
                string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"];
                string sortDirection = Request.Form["order[0][dir]"];
                int totalrows = 0;
                int totalrowsafterfiltering = 0;
                List<trackQueModel> publishList = new List<trackQueModel>();
                var publishingList = _repo.GetAllPublish();
                publishList = (List<trackQueModel>)publishingList;
                publishList = publishList.Where(x => x.status == "pending").ToList();

                totalrows = publishList.Count;


                totalrowsafterfiltering = publishList.Count;

                //sorting
                publishList = publishList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();

                //paging
                publishList = publishList.Skip(start).Take(length).ToList();


                var jsonData = new
                {
                    data = publishList,
                    draw = Request.Form["draw"],
                    recordsTotal = totalrows,
                    recordsFiltered = totalrowsafterfiltering
                };
                return Json(jsonData);

            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Publishing All: {ex.ToString()}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }

        }

        public ActionResult PublishAll()
        {
            bool imguploaded = false;
            var error = new PublishError();
            List<PublishError> ListError = new List<PublishError>();
            List<trackQueModel> publishList = new List<trackQueModel>();
            BlobURIDto blobURIDto = null;
            var publishingList = _repo.PublishOnProd(publishList);
            publishList = (List<trackQueModel>)publishingList;
            publishList = publishList.Where(x => x.status == "pending").AsQueryable().OrderBy(x => x.tblquecms_id).ToList();
            var publishId = "";
            string bannerType = "";
            if(publishList.Count() == 0)
            {
                var jsonData = new
                {
                    data = new { message = bannerType + " No Data for Publish.", isSuccessfull = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                " | UserError: " + jsonData.data.message + " | PublishId: " + publishId);
                return Json(jsonData);
            }
            //uploading images on Prod FTP
            foreach (var item in publishList)
            {
                publishId = Convert.ToString(item.tblquecms_id.ToString());
                bannerType = item.banner_type;

                if (!string.IsNullOrEmpty(item.img_name) && (!string.IsNullOrEmpty(item.banner_type)))
                    if (item.action_done == "Delete")
                    {
                        //Delete image from staging
                        blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("ProdContainerRoot").Value,
                            FolderPath = GetBlobFolderPathByBanner(bannerType),// add path for banner type
                            FileName = item.img_name
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                        string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;


                        //if (!string.IsNullOrEmpty(result))
                        imguploaded = true;

                        //imguploaded = _repoPublish.FTPDeleteFileProd(item.img_name, item.banner_type);
                        bannerType = item.banner_type.ToString();
                    }
                    else if (item.action_done == "Upload")
                    {
                        blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = GetBlobFolderPathByBanner(bannerType),
                            FileName = item.img_name
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Download", blobURIDto);
                        BlobDownloadResponseDto downloadDto = BlobStorageAPIService.BlobFileDownloader(endPoint).Result;

                        FormFile fileForm = new FormFile(downloadDto.Content, 0, downloadDto.Content.Length, downloadDto.Name, downloadDto.FileName);

                        if (downloadDto != null)
                        {
                            blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("ProdContainerRoot").Value,
                                FolderPath = GetBlobFolderPathByBanner(bannerType),
                                FormFile = fileForm,
                                FileName = item.img_name
                            };
                            endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);

                            BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                            if (uploadResponseDto.status != null)
                                imguploaded = true;
                        }

                        //imguploaded = _repoPublish.uploadImagetoFTPServer(item.img_name, item.banner_type);
                        bannerType = item.banner_type;
                    }
                    else if (item.action_done == "UploadPage" || item.action_done == "DeletePage")
                    {
                        imguploaded = true;
                        bannerType = item.banner_type.ToString();
                    }

                if (imguploaded == true)
                {
                    using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                    {

                        if (!string.IsNullOrEmpty(item.que_script) && (bannerType == "Homebanner") || (bannerType == "ShopTireAll"))
                        {
                            try
                            {
                                con.Open();
                                
                                MySqlCommand cmd = new MySqlCommand(item.que_script, con);

                                cmd.ExecuteNonQuery();
                                con.Close();

                                imguploaded = _repo.Updatetbltracking(item.tblquecms_id, item.tblquecmsimage_id);

                            }
                            catch (Exception ex)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Publish Id: " + publishId + ", " + bannerType + " Publishing Error. " + ex.Message, isSuccessfull = false }
                                };
                                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                    " | UserError: " + jsonData.data + " | StackTrace: " + ex.StackTrace);
                                return Json(jsonData);
                            }
                        }

                        else if ((bannerType == "LeftAd") || (bannerType == "FloatingImage") || (bannerType == "ShopTire"))
                        {
                            List<tblquecmsModel> LeftAdQue = new List<tblquecmsModel>();
                            LeftAdQue = _repo.GetLeftAdQue(item.guid);
                            if(item.action_done == "Upload")
                            {
                                RemoveAllFloatingImageProdByGUID(item.guid, bannerType);
                            }
                            foreach (var que in LeftAdQue)
                            {
                                try
                                {
                                    //if (item.que_script.Contains("'"))
                                    //{
                                    //    item.que_script = item.que_script.Replace("'", "''");
                                    //}
                                    con.Open();
                                    //var encode = Convert.ToBase64String(Encoding.UTF8.GetBytes(que.que_script));
                                    MySqlCommand cmd = new MySqlCommand(que.que_script, con);

                                    cmd.ExecuteNonQuery();
                                    con.Close();

                                    _repo.Updatetblcmsque(que.que_id);

                                }
                                catch (Exception ex)
                                {
                                    var jsonData = new
                                    {
                                        data = new { message = "Publish Id: " + publishId + ", " + bannerType + " Publishing Error. " + ex.Message, isSuccessfull = false }
                                    };
                                    _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                    " | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                                    return Json(jsonData);
                                }
                            }
                            imguploaded = _repo.Updatetbltracking(item.tblquecms_id, item.tblquecmsimage_id);
                        }

                        else if (!string.IsNullOrEmpty(item.que_script) && (bannerType == "coupon") || (bannerType == "promo") || (bannerType == "couponPage") || (bannerType == "promoPage"))
                        {
                            try
                            {
                                con.Open();
                                MySqlCommand cmd = new MySqlCommand(item.que_script, con);

                                cmd.ExecuteNonQuery();
                                con.Close();

                                imguploaded = _repo.Updatetbltracking(item.tblquecms_id, item.tblquecmsimage_id);

                            }
                            catch (Exception ex)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Publish Id: " + publishId + ", " + bannerType + " Publishing Error. " + ex.Message, isSuccessfull = false }
                                };
                                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                    " | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                                return Json(jsonData);
                            }
                        }
                    }


                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = bannerType + " Publishing Error. Publish Id No: " + publishId, isSuccessfull = false }
                    };
                    _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                    " | UserError: " + jsonData.data.message + " | PublishId: " + publishId);
                    return Json(jsonData);
                    //error.banner_id = (int)item.img_id;
                    //error.guid = item.guid;
                    //error.imageName = item.img_name;
                    //error.bannerType = item.banner_type;
                    //ListError.Add(error);
                    //continue;
                }

            }

            if (imguploaded)
            {
                var jsonData = new
                {
                    data = new { message = "Successfully Published the images/pages on Production.", isSuccessfull = true }
                };
                return Json(jsonData);
            }
            else
            {
                var jsonData = new
                {
                    data = new { message = bannerType + " Publishing Error. Publish Id No: " + publishId, isSuccessfull = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                    " | UserError: " + jsonData.data.message + " | PublishId: " + publishId);
                return Json(jsonData);
            }
            var data = new { message = "Successfully Published the images/pages on Production.", isSuccessfull = true };

            return Json(data);
        }

        //This method is also available in ApplicationRepository.
        /// <summary>
        /// Set the folder path of blob storage container according to provided banner type.
        /// </summary>
        /// <param name="bannerType"></param>
        /// <returns>Folder path of Blob Storage Container</returns>
        private string GetBlobFolderPathByBanner(string bannerType)
        {
            string folderPath = string.Empty;

            try
            {
                if (bannerType == "Homebanner")
                    folderPath = _configuration.GetSection("BannersPath").Value;
                else if (bannerType == "coupon")
                    folderPath = _configuration.GetSection("CouponImagesPath").Value;
                else if (bannerType == "FloatingImage")
                    folderPath = _configuration.GetSection("FloatingPath").Value;
                else if (bannerType == "LeftAd")
                    folderPath = _configuration.GetSection("AdsPath").Value;
                else if (bannerType == "promo")
                    folderPath = _configuration.GetSection("PromoImagesPath").Value;
                else if (bannerType == "ShopTire")
                    folderPath = _configuration.GetSection("ShopTiresPath").Value;
                else if (bannerType == "ShopTireAll")
                    folderPath = _configuration.GetSection("ShopTiresPath").Value;

                return folderPath;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                                    " | UserError: " + ex.StackTrace + " | BannerType: " + bannerType);
                throw;
            }
        }

        public bool RemoveAllFloatingImageProdByGUID(Guid GUID,string bannerType)
        {
            if( bannerType == "FloatingImage")
            {
                bool isRemoved = false;
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                    {
                        try
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("DELETE FROM floating_images WHERE guid = '" + GUID + "'", conn);

                            cmd.ExecuteNonQuery();

                            conn.Close();

                            return true;
                        }
                        catch (Exception)
                        {
                            conn.Close();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            
            else if (bannerType == "LeftAd")
            {
                bool isRemoved = false;
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                    {
                        try
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("DELETE FROM ads_images WHERE guid = '" + GUID + "'", conn);

                            cmd.ExecuteNonQuery();

                            conn.Close();

                            return true;
                        }
                        catch (Exception)
                        {
                            conn.Close();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            else if (bannerType == "ShopTire")
            {
                bool isRemoved = false;
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                    {
                        try
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("DELETE FROM shoptire WHERE guid = '" + GUID + "'", conn);

                            cmd.ExecuteNonQuery();

                            conn.Close();

                            return true;
                        }
                        catch (Exception)
                        {
                            conn.Close();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            else
            {
                return true;
            }
        }


    }

}
