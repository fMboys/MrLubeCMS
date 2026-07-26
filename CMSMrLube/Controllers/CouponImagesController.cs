using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MrLubeCMS.CustomHandler;
using MrLubeCMS.ViewModels;
using MySql.Data.MySqlClient;
using Serilog;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
//using System.Web.Mvc;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class CouponImagesController : Controller
    {
        private readonly ILogger<CouponImagesController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly ICouponImagesRepository _couponImageRepo;
        private readonly ICouponsRepository _couponsRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;
        public CouponImage couponImage;
        public tblquecmsimageModel imageId;//todo ??

        public CouponImagesController(ILogger<CouponImagesController> logger, CMSDbContext dbContext,
            ICouponImagesRepository couponImageRepo, IApplicationRepository appRepo, IConfiguration configuration, ICouponsRepository couponsRepo)
        {
            _logger = logger;
            _dbContext = dbContext;
            _appRepo = appRepo;
            _couponImageRepo = couponImageRepo;
            _couponsRepo = couponsRepo;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }

        public IActionResult Create()
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {

                ViewBag.ddlCouponPages = new List<CouponPages>();
                List<CouponPages> lst = _couponImageRepo.ddlCouponPages();
                if (lst.Count() > 0)
                {
                    ViewBag.ddlCouponPages = lst;
                }
                CouponImages coupon = new CouponImages();

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "Coupon" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "Coupon" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(coupon);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Coupon Image Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCouponImage(IFormCollection form)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            string language = string.Empty;
            bool isAdded = false;
            bool isEmpty = false;
            CouponImages couponImages = new CouponImages();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            Helper timeHelp = new Helper(_configuration);
            var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
            var imageName = timestamp + "_";

            try
            {
                if (ModelState.IsValid)
                {

                    if (form != null)
                    {
                        if (form["language"] == "English")
                            couponImages.language_id = 1;
                        else
                            couponImages.language_id = 2;
                        couponImages.title = form["title"];
                        couponImages.url_key = form["url_key"];
                        couponImages.view = form["viewDevice"];
                        couponImages.status = form["imageStatus"];
                    }
                    if (couponImages.status == "Active" && _couponImageRepo.IsAlreadyExists(couponImages))
                    {
                        var jsonData = new
                        {
                            data = new { message = "An active image already exists for this title.", isSuccessfull = false }
                        };
                        return Json(jsonData);
                    }


                    couponImages.guid = Guid.NewGuid();
                    string fileName = "";
                    var image = Request.Form?.Files?.GetFile("imageFile");
                    bool isUploaded = false;

                    isEmpty = _appRepo.CheckImageQueue();

                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "CouponImages", "Dev");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);

                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }

                    if (isUploaded == true && form != null)
                    {
                        couponImages.image = fileName;

                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                            couponImages.last_user = HttpContext.User.Identity.Name;
                        }
                        couponImages.last_user = couponImages.last_user ?? "";
                        couponImages.date_created = DateTime.Now;
                        couponImages.date_updated = DateTime.Now;

                        isAdded = _couponImageRepo.Add(couponImages);
                        if (isAdded)
                            isAdded = _couponImageRepo.SaveScriptAndData(formMode, couponImages, ref queuesDto, user);
                    }

                    language = form["language"];
                    TempData["mainImage"] = fileName;
                    TempData["imageId"] = couponImages.guid;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    //TempData["guid"] = couponImages.guid;
                    TempData["formMode"] = "Create";

                    if (isAdded == true)
                    {
                        var jsonData = new
                        {
                            CouponImageData = TempData["mainImage"],
                            data = new { message = "Coupon Image uploaded successfully.", isSuccessfull = true, formMode = "Create", isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            CouponImageData = TempData["mainImage"],
                            data = new { message = "There was an error on uploading the Coupon Image. Id = " + couponImages.guid, isSuccessfull = false, formMode = "Create" }
                        };
                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"Coupon Image Id: {couponImages.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        CouponImageData = TempData["mainImage"],
                        data = new { message = "There was an error on uploading the Coupon Image. Id = " + couponImages.guid, isSuccessfull = false, formMode = "Create" }
                    };
                    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Coupon Image Id: {couponImages.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "There was an error on uploading the Coupon Image. Error = " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                    ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + jsonData.data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the image.");
                return Json(jsonData);
            }
        }

        public async Task<ActionResult> CouponImageList()
        {
            try
            {
                List<CouponImages> CouponImageList = new List<CouponImages>();

                GetData(ref CouponImageList);
                var jsonData = new
                {
                    data = CouponImageList
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "Coupon Image List Error: There was an Error on Listing Coupon Image." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return base.View("Error");
            }
        }

        //public IActionResult CheckCouponURL(Guid id)
        //{
        //    CouponPages couponPages = new CouponPages();
        //    var result = _couponImageRepo.checkCouponURL(id,ref couponPages);
        //    if (result == true)
        //    {
        //        return Json(new { success = true, message = "Coupon URL already exists.", coupons = couponPages });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Coupon URL does not exist." });
        //    }

        //}
        public IActionResult Details(Guid id)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                CouponImages CouponImageData = new CouponImages();

                if (id != null)
                {
                    CouponImageData = _couponImageRepo.FindByGuidID(id);
                }
                // Replace with automapper
                CouponImagesDto couponImageDto = new CouponImagesDto()
                {
                    CouponImageId = CouponImageData.coupon_image_id,
                    LanguageId = CouponImageData.language_id,
                    url_key = CouponImageData.url_key,
                    Title = CouponImageData.title,
                    ImageName = CouponImageData.image,
                    ImageStatus = CouponImageData.status,
                    ViewDevice = CouponImageData.view,
                    LastUser = CouponImageData.last_user,
                    ViewPage = CouponImageData.page,
                    CreatedDate = CouponImageData.date_created,
                    UpdatedDate = CouponImageData.date_updated
                };
                if (CouponImageData.language_id == 1)
                    couponImageDto.Language = "English";
                else if (CouponImageData.language_id == 2)
                    couponImageDto.Language = "French";

                return View(couponImageDto);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Coupon Image Detail Issue Id: {id}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        public IActionResult Edit(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                ViewBag.ddlCouponPages = new List<CouponPages>();
                List<CouponPages> lst = _couponImageRepo.ddlCouponPages();
                if (lst.Count() > 0)
                {
                    ViewBag.ddlCouponPages = lst;
                }

                CouponImages couponImages = new CouponImages();

                couponImages = _couponImageRepo.FindByGuidID(guid);

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "Coupon" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "Coupon" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(couponImages);

            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Coupon Image Specs Issue Id: {guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }


        public IActionResult Delete(Guid id)
        {
            bool isDeleted = false;
            bool isAdded = false;
            string formMode = "Delete";
            string bannerType = "coupon";
            CouponImages couponImageData = new CouponImages();
            Helper helper = new Helper(_configuration);
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            try
            {
                //delete from server
                if (id != null)
                {
                    couponImageData = _couponImageRepo.FindByGuidID(id);
                    List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();
                    if (!string.IsNullOrEmpty(couponImageData.image))
                    {
                        var checkdelFile = _appRepo.isFilependingbanner(id, bannerType, ref tblquecmsimage);
                        bool checkprodFile = _couponImageRepo.CheckFileOnProd(couponImageData);

                        if (tblquecmsimage.Count > 0)
                        {
                            foreach (var item in tblquecmsimage)
                            {
                                _appRepo.RemoveImgQueData(item.img_queId);
                            }

                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                                FileName = couponImageData.image
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                            if (isDeleted)
                            {
                                isDeleted = _couponImageRepo.Update(couponImageData, formMode);
                                if (checkprodFile == true && isDeleted == true)
                                {
                                    string user = "";
                                    if (HttpContext.User.Identity != null)
                                    {
                                        user = HttpContext.User.Identity.Name ?? "";
                                    }
                                    isAdded = _couponImageRepo.SaveScriptAndData(formMode, couponImageData, ref queuesDto, user);
                                }

                            }

                            //Delete URL for this Image
                            //var DelURL = _cont.Delete();    

                            //azureFTPDto = helper.GenerateAzureUri(couponImageData.image.ToString(), "CouponImages", "Dev");
                            //isDeleted = AzureService.FTPDeleteFile(couponImageData.image.ToString(), azureFTPDto);

                            if (isDeleted == true)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Coupon Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting Coupon Image Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                       " | Error Message: " + jsonData.data.message + $"Coupon Id: {id}");
                                return Json(jsonData);
                            }

                        }
                        else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                                FileName = couponImageData.image.ToString()
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                            isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(model.image.ToString(), "HomeBanner", "Stage");
                            //string Filedel = model.image;
                            //isUpdate = AzureService.FTPDeleteFile(Filedel, azureFTPDto);

                            if (isDeleted)
                            {
                                bool IsDeleted = false;
                                IsDeleted = _couponImageRepo.Update(couponImageData, formMode);
                                if (checkprodFile == true && isDeleted == true)
                                {
                                    string user = "";
                                    if (HttpContext.User.Identity != null)
                                    {
                                        user = HttpContext.User.Identity.Name ?? "";
                                    }
                                    isAdded = _couponImageRepo.SaveScriptAndData(formMode, couponImageData, ref queuesDto, user);
                                }
                                else
                                {
                                    var jsonData = new
                                    {
                                        data = new { message = "There was an Error on Deleting Coupon Image Id: " + id, isSuccessfull = false }

                                    };
                                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                           " | Error Message: " + jsonData.data.message + $"Coupon Id: {id}");
                                    return Json(jsonData);
                                }

                            }
                            if (isDeleted)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Coupon Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting Coupon Image Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                       " | Error Message: " + jsonData.data.message + $"Coupon Id: {id}");
                                return Json(jsonData);
                            }
                        }

                        else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                                FileName = couponImageData.image.ToString()
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                            isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(model.image.ToString(), "HomeBanner", "Stage");
                            //string Filedel = model.image;
                            //isUpdate = AzureService.FTPDeleteFile(Filedel, azureFTPDto);

                            if (isDeleted)
                            {
                                bool IsDeleted = false;
                                IsDeleted = _couponImageRepo.Update(couponImageData, formMode);
                            }

                            if (isDeleted)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Coupon Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting Coupon Image Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                       " | Error Message: " + jsonData.data.message + $"Coupon Id: {id}");
                                return Json(jsonData);
                            }

                        }
                        else
                        {
                            var jsonData = new
                            {
                                data = new { message = "There was an Error on Deleting Coupon Image Id: " + id, isSuccessfull = false }

                            };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                   " | Error Message: " + jsonData.data.message + $"Coupon Id: {id}");
                            return Json(jsonData);
                        }

                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Coupon Image Id: " + id, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Coupon Id: {id}");
                        return Json(jsonData);
                    }


                }
                return View();
            }
            catch (Exception ex)
            {

                var jsonData = new
                {
                    data = new { message = "Error: Coupon Image Could not be Deleted. " + ex.Message, isSuccessfull = false }

                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Edit(IFormCollection formValue)
        {
            var data = new { message = "", isUpdate = false };
            string formMode = "Edit";
            bool isUpdated = false;
            bool isEmpty;
            string language = string.Empty;
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            CouponImages couponImages = new CouponImages();
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            Helper timeHelp = new Helper(_configuration);
            var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
            var imageName = timestamp + "_";
            string fileName = "";

            try
            {
                if (ModelState.IsValid)
                {
                    isEmpty = _appRepo.CheckImageQueue();
                    if (formValue != null)
                    {
                        Guid guid = new Guid(formValue["guid"]);
                        couponImages.guid = guid;
                        couponImages.coupon_image_id = Convert.ToInt32(formValue["coupon_image_id"]);
                        if (formValue["language"] == "English")
                            couponImages.language_id = 1;
                        else
                            couponImages.language_id = 2;
                        couponImages.title = formValue["title"];
                        couponImages.url_key = formValue["url_key"];
                        couponImages.view = formValue["viewDevice"];
                        couponImages.status = formValue["imageStatus"];

                        if (couponImages.status == "Active" && _couponImageRepo.IsAlreadyExists(couponImages))
                        {
                            var jsonData = new
                            {
                                data = new { message = "An active image already exists for this title.", isSuccessfull = false }
                            };
                            return Json(jsonData);
                        }
                        var image = Request.Form?.Files?.GetFile("imageFile");
                        bool isUploaded = false;
                        
                        
                        
                        if (image != null)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                                FormFile = image,
                                FileName = imageName + image.FileName.ToString()
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                            BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                            if (uploadResponseDto.status != null)
                                isUploaded = true;

                            //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "CouponImages", "Dev");
                            //isUploaded = AzureService.FTPUploader(image, azureFTPDto);
                            fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                        }

                        //couponImages.image = fileName;
                        if (image != null) { couponImages.image = fileName; }
                        else { couponImages.image = formValue["imageUpload"].ToString(); }
                        if (HttpContext.User.Identity != null)
                        {
                            couponImages.last_user = HttpContext.User.Identity.Name;
                        }
                        couponImages.last_user = couponImages.last_user ?? "";
                        couponImages.date_created = DateTime.Now;
                        couponImages.date_updated = DateTime.Now;

                        isUpdated = _couponImageRepo.Update(couponImages, string.Empty);

                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                        }

                        if (isUpdated)
                            isUpdated = _couponImageRepo.SaveScriptAndData(formMode, couponImages, ref queuesDto, user);
                    }

                    TempData["mainImage"] = fileName;
                    TempData["imageId"] = couponImages.guid;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["formMode"] = "Edit";
                    language = formValue["language"];

                    if (isUpdated == true)
                    {
                        var jsonData = new
                        {
                            updatedImage = TempData["mainImage"],
                            data = new { message = "Coupon Image updated successfully.", isUpdate = true, formMode = "Edit", isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            updatedImage = TempData["mainImage"],
                            data = new { message = $"Coupon Image Updating Error: There was an Error on Updating Coupon Image: {couponImages.guid}", isUpdate = false, formMode = "Edit" }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Coupon Id: {couponImages.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        updatedImage = TempData["mainImage"],
                        data = new { message = $"Coupon Image Updating Error: There was an Error on Updating Coupon Image: {couponImages.guid}", isUpdate = false, formMode = "Edit" }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Coupon Id: {couponImages.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Coupon Image Could not be Uploaded." + ex.Message, isUpdate = false, formMode = "Edit" }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Publish(IFormFile formFile)
        {
            //bool isUploaded = false;
            bool isUpdated = false, isUploaded = false, prodUpdated = false;
            byte[] imageBytes;
            string imageName = string.Empty;
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            tblquecmsimage imageData = new tblquecmsimage();
            CouponImages couponImages = new CouponImages();
            Helper helper = new Helper(_configuration);
            //int imageId = Convert.ToInt32(TempData["imageId"]);
            Guid imageId = new Guid(TempData["imageId"].ToString() ?? "");
            int qImageId = Convert.ToInt32(TempData["qImageId"]);
            int qScriptId = Convert.ToInt32(TempData["qScriptId"]);
            string formMode = (string)TempData["formMode"] ?? "";
            var data = new { message = "", isSuccessfull = false };

            try
            {
                imageData = _appRepo.GetImageDetailByID(imageId, qImageId);
                imageName = imageData.img_name.ToString();

                if (!string.IsNullOrEmpty(imageName))
                {
                    BlobURIDto blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                        FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                        FileName = imageName
                    };
                    string endPoint = helper.GenerateBlobStorageUri("Download", blobURIDto);
                    BlobDownloadResponseDto downloadDto = BlobStorageAPIService.BlobFileDownloader(endPoint).Result;

                    FormFile fileForm = new FormFile(downloadDto.Content, 0, downloadDto.Content.Length, downloadDto.Name, downloadDto.FileName);

                    if (downloadDto != null)
                    {
                        blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("ProdContainerRoot").Value,
                            FolderPath = _configuration.GetSection("CouponImagesPath").Value,
                            FormFile = fileForm,
                            FileName = imageName
                        };

                        endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;
                    }

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "CouponImages", "Dev");
                    //imageBytes = AzureService.FTPDownloader(azureFTPDto);

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "CouponImages", "Prod");
                    //isUploaded = AzureService.FTPBytesUploader(imageBytes, azureFTPDto);

                    if (isUploaded)
                    {
                        isUpdated = _appRepo.UpdateImageDetailByID(imageId, qImageId);
                        isUpdated = _appRepo.UpdateScriptQueueDetailByID(imageId, qScriptId);
                    }
                }

                if (isUploaded == true && isUpdated == true)
                {
                    couponImages = _couponImageRepo.FindByGuidID(imageId);

                    if (formMode == "Create")
                    {
                        prodUpdated = _couponImageRepo.InsertCouponImageProd(couponImages);
                    }
                    else if (formMode == "Edit")
                    {
                        prodUpdated = _couponImageRepo.UpdateCouponImageProd(imageId, couponImages);
                    }
                }

                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Coupon Image Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing Coupon Image - {imageId} ", isSuccessfull = false }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                         " | Error Message: " + jsonData.data.message + $"Coupon Image Id: {imageId}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Coupon Image failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                 " | Error Message: " + jsonData.data.message + $"banner Id: {imageId} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }

        private void GetData(ref List<CouponImages> CouponImageList)
        {
            try
            {
                List<CouponImages> data = _couponImageRepo.GetCouponImageList();
                CouponImageList = data;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public IActionResult ValidateFileResolution(int width, int height, string viewDevice)
        {
            bool isVerified = false;
            string hint = string.Empty;
            List<ImageSpecification> imageSpecifications = new List<ImageSpecification>();
            var data = new { message = "", isSuccessful = false };

            try
            {
                imageSpecifications = _appRepo.GetAllImagesSpecifications();

                if (imageSpecifications.Count > 0 && imageSpecifications != null)
                {
                    foreach (var spec in imageSpecifications)
                    {
                        if (spec.banner_type == "Coupon" && viewDevice.ToLower() == "desktop")
                        {
                            if (spec.width == width && spec.height == height && spec.view_device == viewDevice.ToLower())
                            {
                                isVerified = true;
                            }
                            else if (spec.view_device == viewDevice.ToLower())
                            {
                                isVerified = false;
                                hint = spec.width + " x " + spec.height + " for desktop.";
                            }
                        }
                        else if (spec.banner_type == "Coupon" && viewDevice.ToLower() == "mobile")
                        {
                            if (spec.width == width && spec.height == height && spec.view_device == viewDevice.ToLower())
                                isVerified = true;
                            else if (spec.view_device == viewDevice.ToLower())
                            {
                                isVerified = false;
                                hint = spec.width + " x " + spec.height + " for mobile.";
                            }
                        }
                    }
                }

                if (isVerified)
                {
                    var jsonData = new
                    {
                        data = new { message = "Coupon Image is valid.", isSuccessful = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "Please upload a valid image of resolution: " + hint, isSuccessful = false }
                    };
                    _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                  " | UserError: " + jsonData.data.message + " | hint: " + hint);
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        //public IActionResult DeleteCouponURL(Guid guid)
        //{
        //    bool isDeleted = false;
        //    bool isAdded = false;
        //    string formMode = "Delete";
        //    string bannerType = "couponPage";
        //    CouponPages couponsData = new CouponPages();
        //    Helper helper = new Helper(_configuration);
        //    AzureFTPDto azureFTPDto = new AzureFTPDto();
        //    TrackingQueuesDto queuesDto = new TrackingQueuesDto();
        //    CouponPages couponPages = new CouponPages();
        //    try
        //    {
        //        couponPages = _couponsRepo.FindByLangGuid(guid);
        //        List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();
        //        var checkdelFile = _appRepo.isFilependingbanner(guid, bannerType, ref tblquecmsimage);
        //        bool checkprodFile = _couponsRepo.CheckFileOnProd(couponPages);
        //        if (tblquecmsimage.Count > 0)
        //        {
        //            foreach (var prod in tblquecmsimage)
        //            {
        //                _appRepo.RemoveImgQueData(prod.img_queId);
        //            }
        //            foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
        //            {
        //                couponsData = _couponsRepo.FindById(item.coupon_page_id);

        //                isDeleted = _couponsRepo.Update(item, formMode);

        //                if (isDeleted)
        //                {
        //                    string user = "";
        //                    if (HttpContext.User.Identity != null)
        //                    {
        //                        user = HttpContext.User.Identity.Name ?? "";
        //                    }
        //                    isAdded = _couponsRepo.SaveScriptAndData(formMode, couponsData, ref queuesDto, user);
        //                }
        //            }

        //            if (isDeleted == true && isAdded == true)
        //            {
        //                var jsonData = new
        //                {
        //                    data = new { message = "Coupon-" + couponsData.title + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
        //                };
        //                return Json(jsonData);
        //            }
        //            else
        //            {
        //                var jsonData = new
        //                {
        //                    data = new { message = "There was an Error on Deleting Coupon Image Id: " + guid, isSuccessfull = false }

        //                };
        //                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
        //                       " | Error Message: " + jsonData.data.message + $"Coupon Id: {guid}");
        //                return Json(jsonData);
        //            }
        //        }
        //        else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
        //        {
        //            foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
        //            {
        //                couponsData = _couponsRepo.FindById(item.coupon_page_id);

        //                isDeleted = _couponsRepo.Update(item, formMode);

        //                if (isDeleted)
        //                {
        //                    string user = "";
        //                    if (HttpContext.User.Identity != null)
        //                    {
        //                        user = HttpContext.User.Identity.Name ?? "";
        //                    }
        //                    isAdded = _couponsRepo.SaveScriptAndData(formMode, couponsData, ref queuesDto, user);
        //                }
        //            }

        //            if (isDeleted == true && isAdded == true)
        //            {
        //                var jsonData = new
        //                {
        //                    data = new { message = "Coupon-" + couponsData.title + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
        //                };
        //                return Json(jsonData);
        //            }
        //            else
        //            {
        //                var jsonData = new
        //                {
        //                    data = new { message = "There was an Error on Deleting Coupon Image Id: " + guid, isSuccessfull = false }

        //                };
        //                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
        //                       " | Error Message: " + jsonData.data.message + $"Coupon Id: {guid}");
        //                return Json(jsonData);
        //            }
        //        }

        //        else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
        //        {
        //            foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
        //            {
        //                couponsData = _couponsRepo.FindById(item.coupon_page_id);

        //                isDeleted = _couponsRepo.Update(item, formMode);

        //            }

        //            if (isDeleted == true)
        //            {
        //                var jsonData = new
        //                {
        //                    data = new { message = "Coupon-" + couponsData.title + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
        //                };
        //                return Json(jsonData);
        //            }
        //            else
        //            {
        //                var jsonData = new
        //                {
        //                    data = new { message = "There was an Error on Deleting Coupon Image Id: " + guid, isSuccessfull = false }

        //                };
        //                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
        //                       " | Error Message: " + jsonData.data.message + $"Coupon Id: {guid}");
        //                return Json(jsonData);
        //            }
        //        }

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {

        //        var jsonData = new
        //        {
        //            data = new { message = "Error: Coupon Page Could not be Deleted. " + ex.Message, isSuccessfull = false }

        //        };
        //        _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
        //           $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
        //        return Json(jsonData);
        //    }
        //}

    }
}
