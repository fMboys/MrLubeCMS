using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using CommandLine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MrLubeCMS.CustomHandler;
using MrLubeCMS.ViewModels;
using Serilog;
using System;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class FloatingImageController : Controller
    {
        private readonly ILogger<ShopTireController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly IFloatingImageRepository _floatingRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;

        public FloatingImageController(ILogger<ShopTireController> logger, CMSDbContext dbContext, IFloatingImageRepository floatRepo, IApplicationRepository appRepo, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _floatingRepo = floatRepo;
            _appRepo = appRepo;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            //try
            //{
            //    List<FloatingImage> floatingImages = _floatingRepo.GetAllFloatingImages().ToList();
            //    ViewBag.floatingImages = floatingImages;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            //    return View("Error");
            //}

            return View();
        }

        public IActionResult Create(int lang,string view)
        {
            var data = new { message = "", isSuccessfull = false };
            ViewModelFloatingImage vmFloatingImage = new ViewModelFloatingImage();
            try
            {
                FloatingImageDto floatingImageDto = new FloatingImageDto();
                List<SubMenu> subMenus = _appRepo.GetSubMenus(lang,view);

                vmFloatingImage.FloatingImageDto = floatingImageDto;
                vmFloatingImage.SubMenus = subMenus;

                List<FloatingImage> floatingImagePages = _floatingRepo.GetAllFloatingImagePages(lang,view).ToList();

                ViewBag.FloatingImagePages = floatingImagePages;
                ViewBag.lang = lang;
                ViewBag.views = view;

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "FloatingImage" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "FloatingImage" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(vmFloatingImage);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Floating Image Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return Problem("Error:MrLubeCMS");
            }
        }

        public async Task<ActionResult> GetFloatingImagesList(string? title, string? imageName, string? viewDevice, int language, string? imageStatus)
        {
            bool Edit = true;
            bool View = true;
            List<FloatingImage> floatingImagesList = new List<FloatingImage>();
            try
            {
                //if (User.IsInRole("admin"))
                //{
                //    Edit = true;
                //    View = true;
                //}

                int start = Convert.ToInt32(Request.Form["start"]);
                int length = Convert.ToInt32(Request.Form["length"]);
                string searchValue = Request.Form["search[value]"];
                string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"];
                string sortDirection = Request.Form["order[0][dir]"];
                int totalRows = 0;
                int totalFilteredRows = 0;

                FloatingImageDto floatingImage = new FloatingImageDto()
                {
                    Title = title,
                    ImageName = imageName,
                    ViewDevice = viewDevice,
                    ImageStatus = imageStatus,
                    LanguageId = language
                };

                floatingImagesList = GetAllFloatingImages(floatingImage);

                //totalRows = floatingImagesList.Count;

                //if (!string.IsNullOrEmpty(searchValue))
                //{
                //    floatingImagesList = floatingImagesList.Where(x => x.title.Contains(searchValue.ToLower()) ||
                //        x.image.Contains(searchValue.ToLower()) || x.view.Contains(searchValue.ToLower()) || x.status.Contains(searchValue.ToLower()) ||
                //        x.ad_hyperlink.Contains(searchValue.ToLower())).ToList();
                //}

                //totalFilteredRows = floatingImagesList.Count;
                ////Sorting
                //floatingImagesList = floatingImagesList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();
                ////Paging
                //floatingImagesList = floatingImagesList.Skip(start).Take(length).ToList();

                var jsonData = new
                {
                    data = floatingImagesList,
                    //draw = Request.Form["draw"],
                    //recordsTotal = totalRows,
                    //recordsFiltered = totalFilteredRows
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "Floating Image List Error: There was an Error on Listing Floating Image." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                   $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return base.View("Error");
            }
        }

        public IActionResult Details(int id)
        {
            var data = new { message = "", isSuccessfull = false };
            FloatingImage floatingImageData = new FloatingImage();
            FloatingImageDto floatingDto = null;
            ViewModelFloatingImage vmFloatingImage = new ViewModelFloatingImage();
            try
            {
                if (id > 0)
                {
                    floatingImageData = _floatingRepo.FindByID(id);
                }
                //if (guid != Guid.Empty)
                //{
                //    floatingImageData = _floatingRepo.FindByGuid(guid);
                //}

                if (floatingImageData != null)
                {
                    floatingDto = new FloatingImageDto()
                    {
                        Id = floatingImageData.Id,
                        guid = floatingImageData.guid,
                        LanguageId = floatingImageData.language_id,
                        Title = floatingImageData.title,
                        ImageName = floatingImageData.image,
                        ViewPage = floatingImageData.page,
                        UrlKey = floatingImageData.url_key,
                        ViewDevice = floatingImageData.view,
                        LastUser = floatingImageData.last_user,
                        ImageStatus = floatingImageData.status,
                        CreatedDate = floatingImageData.date_created,
                        UpdatedDate = floatingImageData.date_updated,
                        Hyperlink = floatingImageData.ad_hyperlink
                    };

                    List<SubMenu> subMenus = _appRepo.GetSubMenus(floatingDto.LanguageId,floatingDto.ViewDevice);

                    List<FloatingImage> floatingImagePages = _floatingRepo.GetAllFloatingImagePages(floatingDto.LanguageId, floatingDto.ViewDevice).ToList();
                    floatingImagePages = floatingImagePages.Where(x => x.guid == floatingDto.guid).ToList();
                    ViewBag.FloatingImagePages = floatingImagePages;

                    vmFloatingImage.FloatingImageDto = floatingDto;
                    vmFloatingImage.SubMenus = subMenus;                    
                }

                return View(vmFloatingImage);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Flaoting Image Detail Issue Id: {id}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        public IActionResult Edit(int id)
        {
            var data = new { message = "", isSuccessfull = false };
            FloatingImage floatingImageData = new FloatingImage();
            FloatingImageDto floatingDto = new FloatingImageDto();
            ViewModelFloatingImage vmFloatingImage = new ViewModelFloatingImage();
            try
            {
                if (id > 0)
                {
                    floatingImageData = _floatingRepo.FindByID(id);
                }

                if (floatingImageData != null)
                {
                    floatingDto = new FloatingImageDto()
                    {
                        Id = floatingImageData.Id,
                        guid = floatingImageData.guid,
                        LanguageId = floatingImageData.language_id,
                        Title = floatingImageData.title,
                        ImageName = floatingImageData.image,
                        ViewPage = floatingImageData.page,
                        UrlKey = floatingImageData.url_key,
                        ViewDevice = floatingImageData.view,
                        LastUser = floatingImageData.last_user,
                        ImageStatus = floatingImageData.status,
                        CreatedDate = floatingImageData.date_created,
                        UpdatedDate = floatingImageData.date_updated,
                        Hyperlink = floatingImageData.ad_hyperlink
                    };
                }

                List<SubMenu> subMenus = _appRepo.GetSubMenus(floatingDto.LanguageId,floatingDto.ViewDevice);

                List<SubMenu> checkedPages = _appRepo.GetCheckedPages(floatingDto.guid);

                List<FloatingImage> floatingImagePages = _floatingRepo.GetAllFloatingImagePages(floatingDto.LanguageId, floatingDto.ViewDevice).ToList();

                ViewBag.FloatingImagePages = floatingImagePages;

                ViewBag.checkedPages = checkedPages;

                vmFloatingImage.FloatingImageDto = floatingDto;
                vmFloatingImage.SubMenus = subMenus;

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "FloatingImage" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "FloatingImage" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(vmFloatingImage);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Floating Image Specs Issue Id: {floatingImageData.guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveFloatingImage(IFormCollection form)
        {
            string fileName = "";
            string language = string.Empty;
            bool isEmpty = false, isUploaded = false;
            var data = new { message = "", isSuccessful = false };
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            FloatingImage floatingImage = new FloatingImage();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            try
            {
                if (ModelState.IsValid)
                {
                    Helper timeHelp = new Helper(_configuration);
                    var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
                    var imageName = timestamp + "_";
                    var image = Request.Form?.Files?.GetFile("imageFile");

                    isEmpty = _appRepo.CheckImageQueue();

                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("FloatingPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "FloatingImage", "stage");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);

                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }

                    if (isUploaded == true && form != null)
                    {
                        string selectedPages = form["selectedPages"];

                        floatingImage.guid = Guid.NewGuid();

                        if (form["language"] == "English")
                            floatingImage.language_id = 1;
                        else
                            floatingImage.language_id = 2;
                        floatingImage.title = form["titleImage"];
                        floatingImage.image = fileName;
                        floatingImage.ad_hyperlink = Convert.ToString(form["imageHyperLink"]).Replace("'","");
                        floatingImage.view = form["viewDevice"];
                        floatingImage.status = form["imageStatus"];
                       // floatingImage.last_user = "admin"; //TODO: add login user
                        floatingImage.date_created = DateTime.Now;
                        floatingImage.date_updated = DateTime.Now;
                        floatingImage.last_user = "";
                        if (HttpContext.User.Identity != null)
                        { 
                            floatingImage.last_user = HttpContext.User.Identity.Name ?? "";
                        }

                        queuesDto = _floatingRepo.Add(selectedPages, floatingImage);
                    }

                    language = form["language"];
                    TempData["mainImage"] = fileName;
                    TempData["imageGUID"] = floatingImage.guid;
                    TempData["formMode"] = "Create";

                    if (queuesDto != null && isUploaded == true)
                    {
                        var jsonData = new
                        {
                            floatingImageData = TempData["mainImage"],
                            data = new { message = "Floating Image uploaded successfully.", isSuccessful = true, formMode = "Create", isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            floatingImageData = TempData["mainImage"],
                            data = new { message = "There was an error on uploading the Floating image. Id = " + floatingImage.guid, isSuccessful = false, formMode = "Create", isEmpty, language }
                        };
                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {floatingImage.guid}");
                        return Json(jsonData);
                    } 
                }
                else
                {
                    var jsonData = new
                    {
                        floatingImageData = TempData["mainImage"],
                        data = new { message = "There was an error on uploading the Floating image. Id = " + floatingImage.guid, isSuccessful = false, formMode = "Create", isEmpty, language }
                    };
                    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"banner Id: {floatingImage.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "There was an error on uploading the Floating image. Error = " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                    ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + jsonData.data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the image.");
                return Json(jsonData);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IFormCollection formValue)
        {
            string fileName = "";
            string formMode = "Edit";
            string language = string.Empty;
            bool isEmpty = false, isUploaded = false;
            var data = new { message = "", isUpdate = false };
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            FloatingImage floatingImage = new FloatingImage();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();

            try
            {
                if (ModelState.IsValid)
                {
                    Helper timeHelp = new Helper(_configuration);
                    var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
                    var imageName = timestamp + "_";
                    var image = Request.Form?.Files?.GetFile("imageFile");

                    isEmpty = _appRepo.CheckImageQueue();

                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("FloatingPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "FloatingImage", "stage");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);

                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }
                    
                    if (formValue != null)
                    {
                        string selectedPages = formValue["selectedPages"];

                        floatingImage.Id = Convert.ToInt32(formValue["floatingImageID"]);
                        floatingImage.guid = new Guid(formValue["floatingImageGUID"]);

                        if (formValue["language"] == "English")
                            floatingImage.language_id = 1;
                        else
                            floatingImage.language_id = 2;
                        floatingImage.title = formValue["titleImage"];
                        if(image != null) { floatingImage.image = fileName; }
                        else { floatingImage.image = formValue["imageUpload"].ToString(); }
                        floatingImage.ad_hyperlink = Convert.ToString(formValue["imageHyperLink"]).Replace("'","");
                        floatingImage.view = formValue["viewDevice"];
                        floatingImage.status = formValue["imageStatus"];
                        //floatingImage.last_user = "admin"; //TODO: add login user
                        floatingImage.last_user = "";
                        if (HttpContext.User.Identity != null)
                        { 
                            floatingImage.last_user = HttpContext.User.Identity.Name ?? "";
                        }

                        queuesDto = _floatingRepo.Update(selectedPages, floatingImage);
                    }

                    language = formValue["language"];
                    TempData["mainImage"] = fileName;
                    TempData["imageGUID"] = floatingImage.guid;
                    TempData["formMode"] = "Edit";

                    if (queuesDto != null)
                    {
                        var jsonData = new
                        {
                            floatingImageData = TempData["mainImage"],
                            data = new { message = "Floating Image Updated Successfully", isUpdate = true, isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            floatingImageData = TempData["mainImage"],
                            data = new { message = $"Floating Image Updating Error: There was an Error on Updating Floating Image: {floatingImage.guid}", isUpdate = false, isEmpty, language }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"banner Id: {floatingImage.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        floatingImageData = TempData["mainImage"],
                        data = new { message = $"Floating Image Updating Error: There was an Error on Updating Floating Image: {floatingImage.guid}", isUpdate = false, isEmpty, language }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"banner Id: {floatingImage.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Floating Image could not be Updated. " + ex.Message, isUpdate = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        
        public IActionResult Delete(int id)
        {
            bool isDeleted = false;
            string formMode = "Delete";
            string imageName, imageStatus;
            Guid guid;
            FloatingImage floatingImageData = new FloatingImage();
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            var data = new { message = "", isSuccessfull = false };
            try
            {
                //delete from server
                if (id > 0)
                {
                    floatingImageData = _floatingRepo.FindByID(id);
                    guid = floatingImageData.guid;
                    tblquecmsimage queueImageData = _appRepo.GetImageDetailByGUID(guid);

                    bool checkprodFile = _floatingRepo.CheckFileOnProd(floatingImageData);
                    //floatingImageData = _floatingRepo.FindByGuid(guid);

                    if (queueImageData != null)
                    {
                        imageName = queueImageData.img_name;
                        imageStatus = queueImageData.Status;
                    }
                    else
                    {
                        imageName = "";
                        imageStatus = "";
                    }

                    imageName = queueImageData.img_name;
                    imageStatus = queueImageData.Status;

                    if (imageStatus == "pending" && checkprodFile == false)
                    {
                        if (!string.IsNullOrEmpty(imageName))
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("FloatingPath").Value,
                                FileName = imageName
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                            //NOTE: This code has been replaced by Blob Storage API.
                            //azureFTPDto = helper.GenerateAzureUri(imageName, "FloatingImage", "stage");
                            //isDeleted = AzureService.FTPDeleteFile(imageName, azureFTPDto);
                        }

                        if (isDeleted)
                        {
                            isDeleted = _floatingRepo.UpdateFloatingImageByGUID(guid);

                            if (isDeleted)
                                isDeleted = _appRepo.RemoveQueueImageByGuid(guid);

                            if (isDeleted)
                                isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                        }
                    }
                    else if (imageStatus == "completed" && checkprodFile == true)
                    {
                        if (!string.IsNullOrEmpty(imageName))
                        {
                            //azureFTPDto = helper.GenerateAzureUri(imageName, "FloatingImage", "Prod");
                            //isDeleted = AzureService.FTPDeleteFile(imageName, azureFTPDto);

                            //if (isDeleted)
                            //{
                            //    isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                            //    isDeleted = _floatingRepo.SaveScriptAndData(formMode, floatingImageData, ref queuesDto);
                            //}
                            //NOTE: This code has been replaced by Blob Storage API.
                            //azureFTPDto = helper.GenerateAzureUri(imageName, "FloatingImage", "stage");
                            //isDeleted = AzureService.FTPDeleteFile(imageName, azureFTPDto);

                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("FloatingPath").Value,
                                FileName = imageName
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                        }

                        if (isDeleted)
                        {
                            isDeleted = _floatingRepo.UpdateFloatingImageByGUID(guid);

                            if (isDeleted)
                                isDeleted = _appRepo.UpdateImageDetailByGUID(guid);

                            if (isDeleted)
                            {
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }

                                isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                                isDeleted = _floatingRepo.SaveScriptAndData(formMode, floatingImageData, ref queuesDto, user);
                            }

                            //if (isDeleted)
                            //    isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                        }
                    }

                    else if ((imageStatus == "pending" || imageStatus == "") && checkprodFile == true)
                    {
                        if (!string.IsNullOrEmpty(imageName))
                        {

                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("FloatingPath").Value,
                                FileName = imageName
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                        }

                        if (isDeleted)
                        {
                            isDeleted = _floatingRepo.UpdateFloatingImageByGUID(guid);

                            if (isDeleted)
                                isDeleted = _appRepo.UpdateImageDetailByGUID(guid);

                            if (isDeleted)
                            {
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }

                                isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                                isDeleted = _floatingRepo.SaveScriptAndData(formMode, floatingImageData, ref queuesDto, user);
                            }

                            //if (isDeleted)
                            //    isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                        }
                    }
                }

                if (isDeleted == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Floating Image is Deleted.", isSuccessfull = true, formMode = "Delete" }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "There was an Error on Deleting Floating Image Id: " + floatingImageData.guid, isSuccessfull = false }

                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"Floating image Id: {floatingImageData.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Floating Image Could not be Deleted. " + ex.Message, isSuccessfull = false }

                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        public IActionResult Publish(IFormFile formFile)
        {
            bool isUpdated = false, isUploaded = false, prodUpdated = false;
            string imageName = string.Empty;
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            BlobURIDto blobURIDto = null;
            tblquecmsimage imageData = new tblquecmsimage();
            Guid imageGUID = new Guid(TempData["imageGUID"].ToString());
            //Guid imageGUID = new Guid("cef81ec6-2362-422c-8006-718f4c331544");
            string formMode = (string)TempData["formMode"] ?? "";
            var data = new { message = "", isSuccessfull = false };

            try
            {
                imageData = _appRepo.GetImageDetailByGUID(imageGUID);
                imageName = imageData.img_name.ToString();

                if (!string.IsNullOrEmpty(imageName))
                {
                    blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                        FolderPath = _configuration.GetSection("FloatingPath").Value,
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
                            FolderPath = _configuration.GetSection("FloatingPath").Value,
                            FormFile = fileForm,
                            FileName = imageName,
                            //FileType = downloadDto.ContentType
                        };
                        endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);

                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;
                    }

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "FloatingImage", "stage");
                    //imageBytes = AzureService.FTPDownloader(azureFTPDto);

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "FloatingImage", "Prod");
                    //isUploaded = AzureService.FTPBytesUploader(imageBytes, azureFTPDto);

                    if (isUploaded)
                    {
                        isUpdated = _appRepo.UpdateImageDetailByGUID(imageGUID);
                        isUpdated = _appRepo.UpdateScriptQueueDetailByGUID(imageGUID);
                    }
                }

                if (isUploaded == true && isUpdated == true)
                {
                    List<FloatingImage> floatingImages= new List<FloatingImage>();
                    floatingImages = _floatingRepo.GetFloatingImagesByGUID(imageGUID).ToList();

                    if (formMode == "Create")
                    {
                        prodUpdated = _floatingRepo.InsertFloatingImageProd(floatingImages);
                    }
                    else if (formMode == "Edit")
                    {                        
                        prodUpdated = _floatingRepo.UpdateFloatingImageProd(floatingImages, imageGUID); //update process is not implemented yet. it always creates new records.
                    }
                }

                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Floating Image Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing Floating Image - {imageGUID} ", isSuccessfull = false }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                         " | Error Message: " + jsonData.data.message + $"Floating Image Id: {imageGUID}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Floating Image failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                  " | Error Message: " + jsonData.data.message + $"banner Id: {imageGUID} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }

        public IActionResult ValidateFileResolution(int width, int height, string viewDevice)
        {
            bool isVerified = false;
            string hint = string.Empty;
            List<ImageSpecification> imageSpecifications = new List<ImageSpecification>();
            var data = new { message = "", isSuccessful = false };
            string formMode = "Edit";
            try
            {
                imageSpecifications = _appRepo.GetAllImagesSpecifications();

                if (imageSpecifications.Count > 0 && imageSpecifications != null)
                {
                    foreach (var spec in imageSpecifications)
                    {
                        if (spec.banner_type == "FloatingImage" && viewDevice.ToLower() == "desktop")
                        {
                            if (spec.width == width && spec.height == height && spec.view_device == viewDevice.ToLower())
                            {
                                isVerified = true;
                            }                            
                            else if(spec.view_device == viewDevice.ToLower())
                            {
                                isVerified = false;
                                hint = spec.width + " x " + spec.height + " for desktop.";
                            }
                        }
                        else if (spec.banner_type == "FloatingImage" && viewDevice.ToLower() == "mobile")
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
                        data = new { message = "Floating Image is valid.", isSuccessful = true, formMode }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "Please upload a valid image of resolution: " + hint, isSuccessful = false, formMode }
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

        /// <summary>
        /// Use to retrieve all data of Floating Images from database. 
        /// </summary>
        /// <param name="floatingImageDto"></param>
        /// <returns>List of FloatingImages List<FloatingImage></returns>
        private List<FloatingImage> GetAllFloatingImages(FloatingImageDto floatingImageDto)
        {
            try
            {
                FloatingImage floatingImage = new FloatingImage()
                {
                    Id = floatingImageDto.Id,
                    guid = floatingImageDto.guid,
                    title = floatingImageDto.Title,
                    image = floatingImageDto.ImageName,
                    page = floatingImageDto.ViewPage,
                    url_key = floatingImageDto.UrlKey,
                    view = floatingImageDto.ViewDevice,
                    last_user = floatingImageDto.LastUser,
                    status = floatingImageDto.ImageStatus,
                    ad_hyperlink = floatingImageDto.Hyperlink
                };

                List<FloatingImage> floatingImagesList = _floatingRepo.GetAllFloatingImages().ToList();

                return floatingImagesList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult CheckActiveImagePages(string selectedPages, string viewDevice, string language)
        {
            bool isActive = false;
            var data = new { message = "", isSuccessful = false };
            List<SubMenu> subMenus;

            try
            {
                subMenus = _appRepo.GetSelectedSubMenus(selectedPages);
                
                foreach ( var subMenu in subMenus )
                {
                    FloatingImage floatingImage = _floatingRepo.GetActiveFloatingImagePage(subMenu.url_key, viewDevice, language);

                    if (floatingImage != null)
                    {
                        var jsonData = new
                        {
                            data = new { message = "The page '" + subMenu.title + "' is already have an active image.", isSuccessful = false }
                        };
                        return Json(jsonData);
                    }
                }
                
                var resultJson = new
                {
                    data = new { message = "Selected page(s) are valid for Floating image.", isSuccessful = true }
                };
                return Json(resultJson);

            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: " + ex.Message, isSuccessfull = false }
                };
                return Json(jsonData);
            }
        }

    }
}
