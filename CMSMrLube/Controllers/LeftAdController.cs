using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using MrLubeCMS.CustomHandler;
using MrLubeCMS.ViewModels;
using System.Reflection;
using System.Linq.Dynamic.Core;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class LeftAdController : Controller
    {
        private readonly ILogger<LeftAdController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly ILeftAdRepository _leftAdRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;

        public LeftAdController(CMSDbContext dbContext, ILeftAdRepository leftAdRepo, IApplicationRepository appRepo, IConfiguration config, ILogger<LeftAdController> logger)
        {
            _dbContext = dbContext;
            _leftAdRepo = leftAdRepo;
            _appRepo = appRepo;
            _configuration = config;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create(int lang,string view)
        {
            var data = new { message = "", isSuccessfull = false };
            VMLeftAd vmLeftAd = new VMLeftAd();
            try
            {
                LeftAdDto leftAdDto = new LeftAdDto();
                List<SubMenu> subMenus = _appRepo.GetSubMenus(lang,view);

                vmLeftAd.LeftAdDto = leftAdDto;
                vmLeftAd.SubMenus = subMenus;

                List<LeftAd> leftAdPages = _leftAdRepo.GetAllLeftAdPages(lang,view).ToList();

                ViewBag.LeftAdPages = leftAdPages;
                ViewBag.lang = lang;
                ViewBag.views = view;

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "LeftAd" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "LeftAd" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(vmLeftAd);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Left-Ad Image Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                     " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        public IActionResult Details(int id)
        {
            var data = new { message = "", isSuccessfull = false };
            LeftAd leftAdData = new LeftAd();
            LeftAdDto leftAdDto = null;
            VMLeftAd vmLeftAd = new VMLeftAd();
            try
            {
                if (id > 0)
                {
                    leftAdData = _leftAdRepo.FindByID(id);
                }

                if (leftAdData != null)
                {
                    leftAdDto = new LeftAdDto()
                    {
                        Id = leftAdData.Id,
                        guid = leftAdData.guid,
                        LanguageId = leftAdData.language_id,
                        Title = leftAdData.title,
                        ImageName = leftAdData.image,
                        ViewPage = leftAdData.page,
                        UrlKey = leftAdData.url_key,
                        ViewDevice = leftAdData.view,
                        LastUser = leftAdData.last_user,
                        ImageStatus = leftAdData.status,
                        CreatedDate = leftAdData.date_created,
                        UpdatedDate = leftAdData.date_updated,
                        Hyperlink = leftAdData.ad_hyperlink
                    };

                    List<SubMenu> subMenus = _appRepo.GetSubMenus(leftAdDto.LanguageId,leftAdDto.ViewDevice);

                    List<LeftAd> leftAdPages = _leftAdRepo.GetAllLeftAdPages(leftAdDto.LanguageId, leftAdDto.ViewDevice).ToList();
                    leftAdPages = leftAdPages.Where(x => x.guid == leftAdDto.guid).ToList();
                    ViewBag.LeftAdPages = leftAdPages;

                    vmLeftAd.LeftAdDto = leftAdDto;
                    vmLeftAd.SubMenus = subMenus;                    
                }

                return View(vmLeftAd);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Left-Ad Image Detail Issue Id: {leftAdData.guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                   " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        public IActionResult Edit(int id)
        {
            var data = new { message = "", isSuccessfull = false };
            LeftAd leftAdData = new LeftAd();
            LeftAdDto leftAdDto = null;
            VMLeftAd vmLeftAd = new VMLeftAd();
            try
            {
                if (id > 0)
                {
                    leftAdData = _leftAdRepo.FindByID(id);
                    //leftAdData = _leftAdRepo.FindByGuid(guid);
                }

                if (leftAdData != null)
                {
                    leftAdDto = new LeftAdDto()
                    {
                        Id = leftAdData.Id,
                        guid = leftAdData.guid,
                        LanguageId = leftAdData.language_id,
                        Title = leftAdData.title,
                        ImageName = leftAdData.image,
                        ViewPage = leftAdData.page,
                        UrlKey = leftAdData.url_key,
                        ViewDevice = leftAdData.view,
                        LastUser = leftAdData.last_user,
                        ImageStatus = leftAdData.status,
                        CreatedDate = leftAdData.date_created,
                        UpdatedDate = leftAdData.date_updated,
                        Hyperlink = leftAdData.ad_hyperlink
                    };
                }

                List<SubMenu> subMenus = _appRepo.GetSubMenus(leftAdDto.LanguageId,leftAdDto.ViewDevice);

                List<SubMenu> checkedPages = _leftAdRepo.GetLeftAdCheckedPages(leftAdDto.guid);

                List<LeftAd> leftAdPages = _leftAdRepo.GetAllLeftAdPages(leftAdDto.LanguageId, leftAdDto.ViewDevice).ToList();

                ViewBag.LeftAdPages = leftAdPages;

                ViewBag.checkedPages = checkedPages;

                vmLeftAd.LeftAdDto = leftAdDto;
                vmLeftAd.SubMenus = subMenus;

                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "LeftAd" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "LeftAd" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(vmLeftAd);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Left-Ad Image Specs Issue Id: {leftAdData.guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                     " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        public async Task<ActionResult> GetLeftAdsList(string? title, string? imageName, string? viewDevice, int language, string? imageStatus)
        {
            List<LeftAd> leftAdsList = new List<LeftAd>();
            try
            {

                int start = Convert.ToInt32(Request.Form["start"]);
                int length = Convert.ToInt32(Request.Form["length"]);
                string searchValue = Request.Form["search[value]"];
                string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"];
                string sortDirection = Request.Form["order[0][dir]"];
                int totalRows = 0;
                int totalFilteredRows = 0;

                LeftAdDto leftAd = new LeftAdDto()
                {
                    Title = title,
                    ImageName = imageName,
                    ViewDevice = viewDevice,
                    ImageStatus = imageStatus,
                    LanguageId = language
                };

                leftAdsList = GetAllLeftAds(leftAd);

                //totalRows = leftAdsList.Count;

                //if (!string.IsNullOrEmpty(searchValue))
                //{
                //    leftAdsList = leftAdsList.Where(x => x.title.Contains(searchValue.ToLower()) ||
                //        x.image.Contains(searchValue.ToLower()) || x.view.Contains(searchValue.ToLower()) || x.status.Contains(searchValue.ToLower()) ||
                //        x.ad_hyperlink.Contains(searchValue.ToLower())).ToList();
                //}

                //totalFilteredRows = leftAdsList.Count;
                ////Sorting
                //leftAdsList = leftAdsList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();
                ////Paging
                //leftAdsList = leftAdsList.Skip(start).Take(length).ToList();

                var jsonData = new
                {
                    data = leftAdsList,
                    //draw = Request.Form["draw"],
                    //recordsTotal = totalRows,
                    //recordsFiltered = totalFilteredRows
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "Left-Ad Image List Error: There was an Error on Listing Left Ad Image." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                  $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return base.View("Error");
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult SaveLeftAd(IFormCollection form)
        {
            string fileName = "";
            string language = string.Empty;
            bool isEmpty = false, isUploaded = false;
            var data = new { message = "", isSuccessful = false };
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            LeftAd leftAd = new LeftAd();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            Helper timeHelp = new Helper(_configuration);
            var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
            var imageName = timestamp + "_";
            try
            {
                if (ModelState.IsValid)
                {
                    var image = Request.Form?.Files?.GetFile("imageFile");

                    isEmpty = _appRepo.CheckImageQueue();

                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("AdsPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };

                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "LeftAd", "stage");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);

                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }

                    if (isUploaded == true && form != null)
                    {
                        string selectedPages = form["selectedPages"];

                        leftAd.guid = Guid.NewGuid();

                        if (form["language"] == "English")
                            leftAd.language_id = 1;
                        else
                            leftAd.language_id = 2;
                        leftAd.title = form["titleImage"];
                        leftAd.image = fileName;
                        leftAd.ad_hyperlink = Convert.ToString(form["imageHyperLink"]).Replace("'","");
                        leftAd.view = form["viewDevice"];
                        leftAd.status = form["imageStatus"];
                        //leftAd.last_user = "admin"; //TODO: add login user
                        leftAd.date_created = DateTime.Now;
                        leftAd.date_updated = DateTime.Now;

                        leftAd.last_user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            leftAd.last_user = HttpContext.User.Identity.Name ?? "";
                        }

                        queuesDto = _leftAdRepo.Add(selectedPages, leftAd);
                    }

                    language = form["language"];
                    TempData["mainImage"] = fileName;
                    TempData["imageGUID"] = leftAd.guid;
                    TempData["formMode"] = "Create";

                    if (queuesDto != null && isUploaded == true)
                    {
                        var jsonData = new
                        {
                            LeftAdImageData = TempData["mainImage"],
                            data = new { message = "Left Ad Image uploaded successfully.", isSuccessful = true, formMode = "Create", isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            LeftAdImageData = TempData["mainImage"],
                            data = new { message = "There was an error on uploading the Left Ad Id = " + leftAd.guid, isSuccessful = false, formMode = "Create", isEmpty, language }
                        };
                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {leftAd.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        LeftAdImageData = TempData["mainImage"],
                        data = new { message = "There was an error on uploading the Left Ad Id = " + leftAd.guid, isSuccessful = false, formMode = "Create", isEmpty, language }
                    };
                    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {leftAd.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "There was an error on uploading the Left Ad image. Error = " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                    ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + jsonData.data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the image.");
                return Json(jsonData);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Edit(IFormCollection formValue)
        {
            string fileName = "";
            string formMode = "Edit";
            string language = string.Empty;
            bool isEmpty = false, isUploaded = false;
            var data = new { message = "", isUpdate = false };
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            LeftAd leftAd = new LeftAd();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            Helper timeHelp = new Helper(_configuration);
            var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
            var imageName = timestamp + "_";
            try
            {
                if (ModelState.IsValid)
                {
                    var image = Request.Form?.Files?.GetFile("imageFile");

                    isEmpty = _appRepo.CheckImageQueue();

                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("AdsPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };

                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "LeftAd", "stage");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);
                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }

                    if (formValue != null)
                    {
                        string selectedPages = formValue["selectedPages"];

                        leftAd.Id = Convert.ToInt32(formValue["leftAdID"]);
                        leftAd.guid = new Guid(formValue["leftAdGUID"]);

                        if (formValue["language"] == "English")
                            leftAd.language_id = 1;
                        else
                            leftAd.language_id = 2;
                        leftAd.title = formValue["titleImage"];
                        if (image != null) { leftAd.image = fileName; }
                        else { leftAd.image = formValue["imageUpload"].ToString(); }
                        leftAd.ad_hyperlink = Convert.ToString(formValue["imageHyperLink"]).Replace("'","");
                        leftAd.view = formValue["viewDevice"];
                        leftAd.status = formValue["imageStatus"];
                       // leftAd.last_user = "admin"; //TODO: add login user
                                                    //floatingImage.date_created = DateTime.Now;
                                                    //floatingImage.date_updated = DateTime.Now;

                        leftAd.last_user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            leftAd.last_user = HttpContext.User.Identity.Name ?? "";
                        }

                        queuesDto = _leftAdRepo.Update(selectedPages, leftAd);
                    }

                    language = formValue["language"];
                    TempData["mainImage"] = fileName;
                    TempData["imageGUID"] = leftAd.guid;
                    TempData["formMode"] = "Edit";

                    if (queuesDto != null)
                    {
                        var jsonData = new
                        {
                            LeftAdImageData = TempData["mainImage"],
                            data = new { message = "Left Ad Image Updated Successfully.", isUpdate = true, isEmpty, language }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            LeftAdImageData = TempData["mainImage"],
                            data = new { message = $"Left Ad Image Updating Error: There was an Error on Updating Left Ad Image: {leftAd.guid}", isUpdate = false, isEmpty, language = formValue["language"] }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"banner Id: {leftAd.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        LeftAdImageData = TempData["mainImage"],
                        data = new { message = $"Left Ad Image Updating Error: There was an Error on Updating Left Ad Image: {leftAd.guid}", isUpdate = false, isEmpty, language = formValue["language"] }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"banner Id: {leftAd.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Left Ad Image could not be Uploaded. " + ex.Message, isUpdate = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        public IActionResult Delete(int id)
        {
            bool isDeleted = false;
            string formMode = "Delete";
            string imageName, imageStatus;
            Guid guid;
            LeftAd leftAdData = new LeftAd();
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            var data = new { message = "", isSuccessful = false };
            try
            {
                //delete from server
                if (id > 0)
                {
                    leftAdData = _leftAdRepo.FindByID(id);
                    guid = leftAdData.guid;
                    tblquecmsimage queueImageData = _appRepo.GetImageDetailByGUID(guid);

                    bool checkprodFile = _leftAdRepo.CheckFileOnProd(leftAdData);
                    //floatingImageData = _floatingRepo.FindByGuid(guid);
                    if(queueImageData != null) {
                        imageName = queueImageData.img_name;
                        imageStatus = queueImageData.Status;
                    }
                    else
                    {
                        imageName = "";
                        imageStatus = "";
                    }

                    if (imageStatus == "pending" && checkprodFile == false)
                    {
                        if (!string.IsNullOrEmpty(imageName))
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("AdsPath").Value,
                                FileName = imageName
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            if (!string.IsNullOrEmpty(result))
                                isDeleted = true;
                            //NOTE: This code has been replaced by Blob Storage API.
                            //azureFTPDto = helper.GenerateAzureUri(imageName, "LeftAd", "stage");
                            //isDeleted = AzureService.FTPDeleteFile(imageName, azureFTPDto);
                        }

                        if (isDeleted)
                        {
                            isDeleted = _leftAdRepo.UpdateLeftAdByGUID(guid);

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
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("AdsPath").Value,
                                FileName = imageName
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(imageName, "LeftAd", "stage");
                            //isDeleted = AzureService.FTPDeleteFile(imageName, azureFTPDto);
                        }

                        if (isDeleted)
                        {
                            isDeleted = _leftAdRepo.UpdateLeftAdByGUID(guid);

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
                                isDeleted = _leftAdRepo.SaveScriptAndData(formMode, leftAdData, ref queuesDto, user);
                            }
                        }
                    }
                    else if ((imageStatus == "pending" || imageStatus == "") && checkprodFile == true)
                    {
                        if (!string.IsNullOrEmpty(imageName))
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("AdsPath").Value,
                                FileName = imageName
                            };
                            Helper helper = new Helper(_configuration);
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                                isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(imageName, "LeftAd", "stage");
                            //isDeleted = AzureService.FTPDeleteFile(imageName, azureFTPDto);
                        }
                        //_ = isDeleted == true;

                        //if (isDeleted)
                        //{
                            isDeleted = _leftAdRepo.UpdateLeftAdByGUID(guid);

                            if (isDeleted)
                                isDeleted = _appRepo.UpdateImageDetailByGUID(guid);

                            //if (isDeleted)
                            //{
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }

                                isDeleted = _appRepo.RemoveAllQueueScriptsByGuid(guid);
                                isDeleted = _leftAdRepo.SaveScriptAndData(formMode, leftAdData, ref queuesDto, user);
                            //}
                        //}
                    }

                    if (isDeleted == true)
                    {
                        var jsonData = new
                        {
                            data = new { message = "Left Ad is Deleted.", isSuccessful = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Left Ad Image Id: " + leftAdData.guid, isSuccessful = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + data.message + $"banner Id: {leftAdData.guid}");
                        return Json(jsonData);
                    }
                }
                if (isDeleted == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Left Ad is Deleted.", isSuccessful = true, formMode = "Delete" }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "There was an Error on Deleting Left Ad Image Id: " + leftAdData.guid, isSuccessful = false }

                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + data.message + $"banner Id: {leftAdData.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Left Ad Could not be Deleted. " + ex.Message, isSuccessful = false }

                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        public IActionResult Publish(IFormFile formFile)
        {
            bool isUpdated = false, isUploaded = false, prodUpdated = false;
            byte[] imageBytes;
            string imageName = string.Empty;
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            BlobURIDto blobURIDto = null;
            tblquecmsimage imageData = new tblquecmsimage();
            Guid imageGUID = new Guid(TempData["imageGUID"].ToString()); //??null
            //Guid imageGUID = new Guid("c9c590e1-e9dc-4e44-8b49-5b08f913ce97");
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
                        FolderPath = _configuration.GetSection("AdsPath").Value,
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
                            FolderPath = _configuration.GetSection("AdsPath").Value,
                            FormFile = fileForm,
                            FileName = imageName
                        };
                        endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);

                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;
                    }

                    //NOTE: This code has been replaced by Blob Storage API.
                    //azureFTPDto = helper.GenerateAzureUri(imageName, "LeftAd", "stage");
                    //imageBytes = AzureService.FTPDownloader(azureFTPDto);

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "LeftAd", "Prod");
                    //isUploaded = AzureService.FTPBytesUploader(imageBytes, azureFTPDto);

                    if (isUploaded)
                    {
                        isUpdated = _appRepo.UpdateImageDetailByGUID(imageGUID);
                        isUpdated = _appRepo.UpdateScriptQueueDetailByGUID(imageGUID);
                    }
                }

                if (isUploaded == true && isUpdated == true)
                {
                    List<LeftAd> leftAds = new List<LeftAd>();
                    leftAds = _leftAdRepo.GetLeftAdsByGUID(imageGUID).ToList();

                    if (formMode == "Create")
                    {
                        prodUpdated = _leftAdRepo.InsertLeftAdProd(leftAds);
                    }
                    else if (formMode == "Edit")
                    {
                        prodUpdated = _leftAdRepo.UpdateLeftAdsProd(leftAds, imageGUID);
                    }
                }

                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Left Ad Image Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing Left Ad Image - {imageGUID} ", isSuccessfull = false }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                        " | Error Message: " + jsonData.data.message + $"Left Ad Image Id: {imageGUID}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Left Ad Image failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                  " | Error Message: " + jsonData.data.message + $"Left Ad Id: {imageGUID} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
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
                        if (spec.banner_type == "LeftAd" && viewDevice.ToLower() == "desktop")
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
                        else if (spec.banner_type == "LeftAd" && viewDevice.ToLower() == "mobile")
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
                        data = new { message = "Left Ad Image is valid.", isSuccessful = true }
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
                  " | UserError: " + data.message + " | hint: " + hint);
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
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
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

                foreach (var subMenu in subMenus)
                {
                    LeftAd leftAd = _leftAdRepo.GetActiveLeftAdPage(subMenu.url_key, viewDevice, language);

                    if (leftAd != null)
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
                    data = new { message = "Selected page(s) are valid for Left Ad.", isSuccessful = true }
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

        /// <summary>
        /// Use to retrieve all data of Left Ads from database. 
        /// </summary>
        /// <param name="leftAdDto"></param>
        /// <returns>List of leftAdDto<FloatingImage></returns>
        private List<LeftAd> GetAllLeftAds(LeftAdDto leftAdDto)
        {
            try
            {
                LeftAd leftAd = new LeftAd()
                {
                    Id = leftAdDto.Id,
                    guid = leftAdDto.guid,
                    title = leftAdDto.Title,
                    image = leftAdDto.ImageName,
                    page = leftAdDto.ViewPage,
                    url_key = leftAdDto.UrlKey,
                    view = leftAdDto.ViewDevice,
                    last_user = leftAdDto.LastUser,
                    status = leftAdDto.ImageStatus,
                    ad_hyperlink = leftAdDto.Hyperlink
                };

                List<LeftAd> leftAdList = _leftAdRepo.GetAllLeftAds().ToList();

                return leftAdList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


         
    }
}
