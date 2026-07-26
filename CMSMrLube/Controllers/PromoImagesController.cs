using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MrLubeCMS.CustomHandler;
using MySql.Data.MySqlClient;
using Serilog;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using ClientCredential = Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential;
using System.Runtime.Intrinsics.X86;
using Google.Protobuf.WellKnownTypes;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class PromoImagesController : Controller
    {
        private readonly ILogger<PromoImagesController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly IPromoImagesRepository _promoImageRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;
        public PromoImage promoImage;
        public tblquecmsimageModel imageId;//todo ??

        public PromoImagesController(ILogger<PromoImagesController> logger, CMSDbContext dbContext,
            IPromoImagesRepository promoImageRepo, IApplicationRepository appRepo, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _appRepo = appRepo;
            _promoImageRepo = promoImageRepo;
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
                ViewBag.ddlPromoPages = new List<PromoPages>();
                List<PromoPages> lst = _promoImageRepo.ddlPromoPages();
                if (lst.Count() > 0)
                {
                    ViewBag.ddlPromoPages = lst;
                }
                PromoImages promo = new PromoImages();

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "Promo" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "Promo" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(promo);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Promo Image Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                   " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePromoImage(IFormCollection form)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            string language = string.Empty;
            bool isAdded = false;
            bool isEmpty = false;
            PromoImages promoImages = new PromoImages();
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
                            promoImages.language_id = 1;
                        else
                            promoImages.language_id = 2;
                        promoImages.title = form["title"];
                        promoImages.url_key = form["url_key"];
                        promoImages.view = form["viewDevice"];
                        promoImages.status = form["imageStatus"];
                    }
                    if (promoImages.status == "Active" && _promoImageRepo.IsAlreadyExists(promoImages))
                    {
                        var jsonData = new
                        {
                            data = new { message = "An active image already exists for this title.", isSuccessfull = false }
                        };
                        return Json(jsonData);
                    }

                    promoImages.guid = Guid.NewGuid();
                    string fileName = "";
                    var image = Request.Form?.Files?.GetFile("imageFile");
                    bool isUploaded = false;

                    isEmpty = _appRepo.CheckImageQueue();

                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };
                        string val = HttpContext.Session.GetString("") ?? "";
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);

                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "PromoImages", "Dev");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);
                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }

                    if (isUploaded == true && form != null)
                    {
                        promoImages.image = fileName;
                        //  promoImages.page = "Promo Image"
                        promoImages.promo_hyperlink = Convert.ToString(form["imageHyperLink"]).Replace("'", "");
                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                            promoImages.last_user = HttpContext.User.Identity.Name;
                        }
                        promoImages.last_user = promoImages.last_user ?? "";
                        promoImages.date_created = DateTime.Now;
                        promoImages.date_updated = DateTime.Now;

                        isAdded = _promoImageRepo.Add(promoImages);
                        if (isAdded)
                            isAdded = _promoImageRepo.SaveScriptAndData(formMode, promoImages, ref queuesDto, user);
                    }

                    language = form["language"];
                    TempData["mainImage"] = fileName;
                    TempData["imageId"] = promoImages.guid;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    //TempData["guid"] = promoImages.guid;
                    TempData["formMode"] = "Create";

                    if (isAdded == true)
                    {
                        var jsonData = new
                        {
                            PromoImageData = TempData["mainImage"],
                            data = new { message = "Promo Image uploaded successfully.", isSuccessfull = true, formMode = "Create", isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            PromoImageData = TempData["mainImage"],
                            data = new { message = "There was an error on uploading the Promo Image. Id = " + promoImages.guid, isSuccessfull = false, formMode = "Create" }
                        };
                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"Coupon Image Id: {promoImages.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        PromoImageData = TempData["mainImage"],
                        data = new { message = "There was an error on uploading the Promo Image. Id = " + promoImages.guid, isSuccessfull = false, formMode = "Create" }
                    };
                    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Promo Image Id: {promoImages.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "There was an error on uploading the promo Image. Error = " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                    ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + jsonData.data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the image.");
                return Json(jsonData);
            }
        }

        public async Task<ActionResult> PromoImageList()
        {
            try
            {
                List<PromoImages> PromoImageList = new List<PromoImages>();

                GetData(ref PromoImageList);
                var jsonData = new
                {
                    data = PromoImageList
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "Promo Image List Error: There was an Error on Listing Promo Image." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return base.View("Error");
            }
        }

        public IActionResult Details(Guid id)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                PromoImages PromoImageData = new PromoImages();

                if (id != null)
                {
                    PromoImageData = _promoImageRepo.FindByGuidID(id);
                }
                // Replace with automapper
                PromoImagesDto promoImageDto = new PromoImagesDto()
                {
                    PromoImageId = PromoImageData.promo_image_id,
                    LanguageId = PromoImageData.language_id,
                    url_key = PromoImageData.url_key,
                    Title = PromoImageData.title,
                    ImageName = PromoImageData.image,
                    ImageStatus = PromoImageData.status,
                    ViewDevice = PromoImageData.view,
                    LastUser = PromoImageData.last_user,
                    ViewPage = PromoImageData.page,
                    Hyperlink = PromoImageData.promo_hyperlink,
                    CreatedDate = PromoImageData.date_created,
                    UpdatedDate = PromoImageData.date_updated
                };
                if (PromoImageData.language_id == 1)
                    promoImageDto.Language = "English";
                else if (PromoImageData.language_id == 2)
                    promoImageDto.Language = "French";

                return View(promoImageDto);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Promo Image Detail Issue Id: {id}", isSuccessfull = false };
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
                ViewBag.ddlPromoPages = new List<PromoPages>();
                List<PromoPages> lst = _promoImageRepo.ddlPromoPages();
                if (lst.Count() > 0)
                {
                    ViewBag.ddlPromoPages = lst;
                }
                PromoImages promoImages = new PromoImages();

                promoImages = _promoImageRepo.FindByGuidID(guid);

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "Promo" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "Promo" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(promoImages);

            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Promo Image Specs Issue Id: {guid}", isSuccessfull = false };
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
            string bannerType = "promo";
            Helper helper = new Helper(_configuration);
            PromoImages promoImageData = new PromoImages();
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            try
            {
                //delete from server
                if (id != null)
                {
                    promoImageData = _promoImageRepo.FindByGuidID(id);
                    List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();

                    if (!string.IsNullOrEmpty(promoImageData.image))
                    {
                        var checkdelFile = _appRepo.isFilependingbanner(id, bannerType, ref tblquecmsimage);
                        bool checkprodFile = _promoImageRepo.CheckFileOnProd(promoImageData);
                        if (tblquecmsimage.Count > 0)
                        {
                            foreach (var item in tblquecmsimage)
                            {
                                _appRepo.RemoveImgQueData(item.img_queId);
                            }

                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                                FileName = promoImageData.image
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            //string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);

                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                            if (isDeleted)
                            {
                                isDeleted = _promoImageRepo.Update(promoImageData, formMode);
                                if (checkprodFile == true && isDeleted == true)
                                {
                                    string user = "";
                                    if (HttpContext.User.Identity != null)
                                    {
                                        user = HttpContext.User.Identity.Name ?? "";
                                    }
                                    isAdded = _promoImageRepo.SaveScriptAndData(formMode, promoImageData, ref queuesDto, user);
                                }

                            }

                            if (isDeleted == true)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Promo Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting Promo Image Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                      " | Error Message: " + jsonData.data.message + $"Promo Id: {id}");
                                return Json(jsonData);
                            }
                        }

                        else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                                FileName = promoImageData.image.ToString()
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
                                isDeleted = _promoImageRepo.Update(promoImageData, formMode);
                                if (checkprodFile == true && isDeleted == true)
                                {
                                    string user = "";
                                    if (HttpContext.User.Identity != null)
                                    {
                                        user = HttpContext.User.Identity.Name ?? "";
                                    }
                                    isAdded = _promoImageRepo.SaveScriptAndData(formMode, promoImageData, ref queuesDto, user);
                                }
                                else
                                {
                                    var jsonData = new
                                    {
                                        data = new { message = "There was an Error on Deleting Promo Image Id: " + id, isSuccessfull = false }

                                    };
                                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                          " | Error Message: " + jsonData.data.message + $"Promo Id: {id}");
                                    return Json(jsonData);
                                }

                            }
                            if (isDeleted)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Promo Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting Promo Image Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                      " | Error Message: " + jsonData.data.message + $"Promo Id: {id}");
                                return Json(jsonData);
                            }
                        }

                        else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                                FileName = promoImageData.image.ToString()
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
                                IsDeleted = _promoImageRepo.Update(promoImageData, formMode);
                            }

                            if (isDeleted)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Promo Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting Promo Image Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                      " | Error Message: " + jsonData.data.message + $"Promo Id: {id}");
                                return Json(jsonData);
                            }

                        }

                        else
                        {
                            var jsonData = new
                            {
                                data = new { message = "There was an Error on Deleting Promo Image Id: " + id, isSuccessfull = false }

                            };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                  " | Error Message: " + jsonData.data.message + $"Promo Id: {id}");
                            return Json(jsonData);
                        }

                        //azureFTPDto = helper.GenerateAzureUri(promoImageData.image.ToString(), "PromoImages", "Dev");
                        //isDeleted = AzureService.FTPDeleteFile(promoImageData.image.ToString(), azureFTPDto);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Promo Image Id: " + id, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                              " | Error Message: " + jsonData.data.message + $"Promo Id: {id}");
                        return Json(jsonData);
                    }

                }

                //if (isDeleted)
                //{
                //    string user = "";
                //    if (HttpContext.User.Identity != null)
                //    {
                //        user = HttpContext.User.Identity.Name ?? "";
                //    }
                //    isAdded = _promoImageRepo.SaveScriptAndData(formMode, promoImageData, ref queuesDto, user);
                //}


                return View();
            }
            catch (Exception ex)
            {
                Log.Information("PromoImagesController");
                Log.Error(ex.ToString());
                var jsonData = new
                {
                    data = new { message = "Error: Promo Image Could not be Deleted. " + ex.Message, isSuccessfull = false }

                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IFormCollection formValue)
        {
            var data = new { message = "", isUpdate = false };
            string formMode = "Edit";
            bool isUpdated = false;
            bool isEmpty;
            string language = string.Empty;
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            PromoImages promoImages = new PromoImages();
            AzureFTPDto azureFTPDto = new AzureFTPDto();
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
                        promoImages.guid = guid;
                        promoImages.promo_image_id = Convert.ToInt32(formValue["promo_image_id"]);
                        if (formValue["language"] == "English")
                            promoImages.language_id = 1;
                        else
                            promoImages.language_id = 2;
                        promoImages.title = formValue["title"];
                        promoImages.url_key = formValue["url_key"];
                        promoImages.view = formValue["viewDevice"];
                        promoImages.status = formValue["imageStatus"];

                        if (promoImages.status == "Active" && _promoImageRepo.IsAlreadyExists(promoImages))
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
                                FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                                FormFile = image,
                                FileName = imageName + image.FileName.ToString()
                            };

                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                            BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                            if (uploadResponseDto.status != null)
                                isUploaded = true;

                            //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "PromoImages", "Dev");
                            //isUploaded = AzureService.FTPUploader(image, azureFTPDto);
                            fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                        }
                        //if (formValue != null)
                        //{
                        //promoImages.image = fileName;
                        
                        if (image != null) { promoImages.image = fileName; }
                        else { promoImages.image = formValue["imageUpload"].ToString(); }
                        promoImages.promo_hyperlink = Convert.ToString(formValue["imageHyperLink"]).Replace("'", "");
                        if (HttpContext.User.Identity != null)
                        {
                            promoImages.last_user = HttpContext.User.Identity.Name;
                        }
                        promoImages.last_user = promoImages.last_user ?? "";
                        promoImages.date_created = DateTime.Now;
                        promoImages.date_updated = DateTime.Now;

                        isUpdated = _promoImageRepo.Update(promoImages, string.Empty);

                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                        }

                        if (isUpdated)
                            isUpdated = _promoImageRepo.SaveScriptAndData(formMode, promoImages, ref queuesDto, user);
                    }
                    //if (formValue != null && isUploaded == true)
                    //{ 
                    //    promoImages.image = fileName;
                    //    promoImages.promo_hyperlink = formValue["imageHyperLink"]; 
                    //    //promoImages.last_user = "admin"; //TODO: add login user
                    //    if (HttpContext.User.Identity != null)
                    //    {
                    //        promoImages.last_user = HttpContext.User.Identity.Name;
                    //    }
                    //    promoImages.last_user = promoImages.last_user ?? "";
                    //    promoImages.date_created = DateTime.Now;
                    //    promoImages.date_updated = DateTime.Now;

                    //    isUpdated = _promoImageRepo.Update(promoImages, string.Empty);

                    //    string user = "";
                    //    if (HttpContext.User.Identity != null)
                    //    {
                    //        user = HttpContext.User.Identity.Name ?? "";
                    //    }

                    //    if (isUpdated)
                    //        isUpdated = _promoImageRepo.SaveScriptAndData(formMode, promoImages, ref queuesDto, user);
                    //}

                    TempData["mainImage"] = fileName;
                    TempData["imageId"] = promoImages.guid;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["formMode"] = "Edit";
                    language = formValue["language"];

                    if (isUpdated == true)
                    {
                        var jsonData = new
                        {
                            updatedImage = TempData["mainImage"],
                            data = new { message = "Promo Image updated successfully.", isUpdate = true, formMode = "Edit", isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            updatedImage = TempData["mainImage"],
                            data = new { message = $"Promo Image Updating Error: There was an Error on Updating Promo Image: {promoImages.guid}", isUpdate = false, formMode = "Edit" }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Promo Id: {promoImages.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        updatedImage = TempData["mainImage"],
                        data = new { message = $"Promo Image Updating Error: There was an Error on Updating Promo Image: {promoImages.guid}", isUpdate = false, formMode = "Edit" }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                         " | Error Message: " + jsonData.data.message + $"Promo Id: {promoImages.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Promo Image Could not be Uploaded." + ex.Message, isUpdate = false, formMode = "Edit" }
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
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            BlobURIDto blobURIDto = null;
            tblquecmsimage imageData = new tblquecmsimage();
            PromoImages promoImages = new PromoImages();
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
                    blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                        FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                        FileName = imageName
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
                            FolderPath = _configuration.GetSection("PromoImagesPath").Value,
                            FormFile = fileForm,
                            FileName = imageName
                        };

                        endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;
                    }

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "PromoImages", "Dev");
                    //imageBytes = AzureService.FTPDownloader(azureFTPDto);

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "PromoImages", "Prod");
                    //isUploaded = AzureService.FTPBytesUploader(imageBytes, azureFTPDto);

                    if (isUploaded)
                    {
                        isUpdated = _appRepo.UpdateImageDetailByID(imageId, qImageId);
                        isUpdated = _appRepo.UpdateScriptQueueDetailByID(imageId, qScriptId);
                    }
                }

                if (isUploaded == true && isUpdated == true)
                {
                    promoImages = _promoImageRepo.FindByGuidID(imageId);

                    if (formMode == "Create")
                    {
                        prodUpdated = _promoImageRepo.InsertPromoImageProd(promoImages);
                    }
                    else if (formMode == "Edit")
                    {
                        prodUpdated = _promoImageRepo.UpdatePromoImageProd(imageId, promoImages);
                    }
                }

                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Promo Image Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing Promo Image - {imageId} ", isSuccessfull = false }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                         " | Error Message: " + jsonData.data.message + $"Promo Image Id: {imageId}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Promo Image failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                 " | Error Message: " + jsonData.data.message + $"Promo Image Id: {imageId} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }

        private void GetData(ref List<PromoImages> PromoImageList)
        {
            try
            {
                List<PromoImages> data = _promoImageRepo.GetPromoImageList();
                PromoImageList = data;

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
                        if (spec.banner_type == "Promo" && viewDevice.ToLower() == "desktop")
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
                        else if (spec.banner_type == "Promo" && viewDevice.ToLower() == "mobile")
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
                        data = new { message = "Promo Image is valid.", isSuccessful = true }
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




    }
}
