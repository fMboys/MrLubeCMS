using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrLubeCMS.CustomHandler;
using System.Reflection;
using System.Linq.Dynamic.Core;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class ShopTireAllController : Controller
    {
        private readonly ILogger<ShopTireAllController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly IShopTireAllRepository _shopTireAllRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;
        public ShopTireImage imageShopTire;
        public tblquecmsimageModel imageId;//todo ??
        public ShopTireAllController(ILogger<ShopTireAllController> logger, CMSDbContext dbContext,
            IShopTireAllRepository shopTireAllRepo, IApplicationRepository appRepo, IConfiguration configuration) 
        {
            _logger = logger;
            _dbContext = dbContext;
            _appRepo = appRepo;
            _shopTireAllRepo = shopTireAllRepo;
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

        public ActionResult ShopTireAllList(string? Title, string? ImageName, string?  ViewDevice,int languageId)
        {
            bool Edit = true;
            bool View = true;

            try
            {
                //if (User.IsInRole("admin"))
                //{
                //    Edit = true;
                //    View = true;
                //}

                int start = Convert.ToInt32(Request.Form["start"]);
                int length = Convert.ToInt32(Request.Form["length"]);
                string searchValue = Request.Form["search[value]"].ToString().ToLowerInvariant();
                string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"];
                string sortDirection = Request.Form["order[0][dir]"];
                int totalRows = 0;
                int totalFilteredRows = 0;
                List<ShopTireAllDto> shopTireList = new List<ShopTireAllDto>();
                ShopTireAllDto shopTireDto = new ShopTireAllDto()
                {
                    //Stores = Stores,
                    Title = Title,
                    ImageName = ImageName,
                    //ImageStatus = ImageStatus,
                    ViewDevice = ViewDevice,
                    LanguageId = languageId
                };
                var selected = "";
                GetData(shopTireDto, ref shopTireList);
                //totalRows = shopTireList.Count;
                //BannerList.ForEach(a => a.ad_hyperlink?.Equals(string.IsNullOrEmpty(a.ad_hyperlink) ? a.ad_hyperlink == Convert.ToString(DBNull.Value) : a.ad_hyperlink));
                //BannerList.Select(s => s.ad_hyperlink ?? Convert.ToString(DBNull.Value)).ToList();
                //searchValue = searchValue.Replace(" ", "");
                //string lang = searchValue.ToLower().Contains("english") ? "1" : searchValue.Contains("french") ? "2" : searchValue.ToLowerInvariant();

                //if (!string.IsNullOrEmpty(searchValue))
                //{

                //    shopTireList = shopTireList.Where(a => a.Title.Contains(searchValue.ToLower()) ||
                //    a.ImageName.Contains(searchValue.ToLower()) || a.ViewDevice.Contains(searchValue.ToLower()) ||
                //    a.LanguageId.ToString().Contains(lang)).ToList();
                //}
                //shopTireList = shopTireList.Where(x => x.ImageStatus != "delete").ToList();
                //totalFilteredRows = shopTireList.Count;

                //sorting
                //shopTireList = shopTireList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();

                //paging
                //shopTireList = shopTireList.Skip(start).Take(length).ToList();


                var jsonData = new
                {
                    data = shopTireList,
                    //draw = Request.Form["draw"],
                    //recordsTotal = totalRows,
                    //recordsFiltered = totalFilteredRows
                };
                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "ShopTire banner List Error: There was an Error on Listing ShopTire Banner." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return base.View("Error");
            }
        }

        public void GetData(ShopTireAllDto shopTireDto, ref List<ShopTireAllDto> shopTireList)
        {
            try
            {
                ShopTireAllDto shopTire = new ShopTireAllDto
                {
                    //Stores = shopTireDto.Stores,
                    Title = shopTireDto.Title,
                    ImageName = shopTireDto.ImageName,
                    ImageStatus = shopTireDto.ImageStatus,
                    ViewDevice = shopTireDto.ViewDevice
                    //Hyperlink = shopTireDto.Hyperlink
                };

                List<ShopTireAllDto> data = _shopTireAllRepo.GetShopTireAllList(shopTire);

                shopTireList = data;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public ActionResult Edit(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                var model = _shopTireAllRepo.FindById(guid);
                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "ShopTires" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "ShopTires" && a.view_device == "mobile").FirstOrDefault();
                var mwidth = mBannerSize.width;
                var mheight = mBannerSize.height;
                var dwidth = dBannerSize.width;
                var dheight = dBannerSize.height;
                ViewBag.mwidth = mwidth;
                ViewBag.mheight = mheight;
                ViewBag.dwidth = dwidth;
                ViewBag.dheight = dheight;

                return View(model);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Home Page Banner Image Specs Issue Id: {guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
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
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            ShopTire shopTire = new ShopTire();
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            Helper timeHelp = new Helper(_configuration);
            var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
            var imageName = timestamp + "_";

            try
            {
                if (ModelState.IsValid)
                {
                    string fileName = "";
                    bool IsStoreExist = false;
                    var image = Request.Form?.Files?.GetFile("imageFile");
                    bool isUploaded = false;
                    Guid guid = new Guid(formValue["img_guid"]);
                    int storeNo = Convert.ToInt32(formValue["store_num"]);
                    //string selectedStores = formValue["SelectedStores"];

                    if (formValue["language"] == "English")
                        shopTire.language_id = 1;
                    else
                        shopTire.language_id = 2;
                    int lang = shopTire.language_id;
                    var device = formValue["viewDevice"];
                    int shopTire_id = Convert.ToInt32(formValue["shopTireID"]);
                    isEmpty = _appRepo.CheckImageQueue();


                    //if (selectedStores == null || selectedStores == "")
                    //{
                    //    var jsonData = new
                    //    {
                    //        shopTireImageData = TempData["mainImage"],
                    //        data = new { message = "Please select the Store Number.", isSuccessfull = false, formMode = "Create", IsStoreExist = true }
                    //    };

                    //    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                    //        " | Error Message: " + jsonData.data.message + $"StoreNo: {storeNo}");
                    //    return Json(jsonData);
                    //}

                    shopTire.guid = guid;


                    if (image != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("ShopTiresPath").Value,
                            FormFile = image,
                            FileName = imageName + image.FileName.ToString()
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;

                        //azureFTPDto = helper.GenerateAzureUri(image.FileName.ToString(), "ShopTire", "Dev");
                        //isUploaded = AzureService.FTPUploader(image, azureFTPDto);

                        fileName = string.IsNullOrEmpty(imageName + image?.FileName.ToString()) ? "" : imageName + image.FileName.ToString();
                    }

                    if (formValue != null)
                    {
                        //shopTire.SelectedStores = selectedStores.Split(",");
                        shopTire.shopTire_id = Convert.ToInt32(formValue["shopTireID"]);
                        if (formValue["language"] == "English")
                            shopTire.language_id = 1;
                        else
                            shopTire.language_id = 2;
                        //shopTire.store_num = Convert.ToInt32(formValue["store_num"]);
                        shopTire.title = formValue["storeTitle"];

                        if (image != null)
                        {
                            shopTire.image = fileName;
                        }
                        else { shopTire.image = formValue["imageUpload"].ToString(); }

                        shopTire.page = "Shop Tire All Generic Image.";
                        shopTire.ad_hyperlink = Convert.ToString(formValue["imageHyperLink"]).Replace("'", "");
                        shopTire.view = formValue["viewDevice"];
                        //shopTire.status = formValue["imageStatus"];
                        shopTire.status = "active";
                        if (HttpContext.User.Identity != null)
                        {
                            shopTire.last_user = HttpContext.User.Identity.Name ?? "";
                        }
                        shopTire.date_created = DateTime.Now;
                        shopTire.date_updated = DateTime.Now;

                        isUpdated = _shopTireAllRepo.Update(shopTire, formMode);

                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                        }

                        if (isUpdated)
                            isUpdated = _shopTireAllRepo.SaveScriptAndData(formMode,shopTire,ref queuesDto, user);

                    }


                    TempData["mainImage"] = fileName;
                    TempData["imageId"] = shopTire.guid;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["formMode"] = "Edit";

                    if (isUpdated == true)
                    {
                        var jsonData = new
                        {
                            updatedImage = TempData["mainImage"],
                            data = new { message = "ShopTire banner for store: " + shopTire.title + " updated successfully.", isUpdate = true, formMode = "Edit", isEmpty }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            updatedImage = TempData["mainImage"],
                            data = new { message = "There is an Error on Editing ShopTire Banner Id: " + shopTire.guid, isUpdate = false, formMode = "Edit" }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {shopTire.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        updatedImage = TempData["mainImage"],
                        data = new { message = "ShopTire banner's data validation failed. Please enter correct details.", isUpdate = false, formMode = "Edit" }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {shopTire.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "There was an Error in Editing ShopTire Banner. " + ex.Message, isUpdate = false, formMode = "Edit" }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                   $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        public IActionResult Details(Guid id)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                ShopTireDto shopTireDto = null;
                List<ShopTire> shopTireData = new List<ShopTire>();
                List<ShopTireDto> shopTireDataDto = new List<ShopTireDto>();
                //if (id != null)
                //{
                //    shopTireData = _shopTireAllRepo.FindById(id);

                //    foreach (var shopTireData_dto in shopTireData)
                //    {
                //        ShopTireDto shopData = new ShopTireDto();
                //        shopData.ShopTireId = shopTireData_dto.shopTire_id;
                //        shopData.guid = shopTireData_dto.guid;
                //        shopData.LanguageId = shopTireData_dto.language_id;
                //        shopData.Stores = Convert.ToString(shopTireData_dto.store_num);
                //        shopData.Title = shopTireData_dto.title;
                //        shopData.ImageName = shopTireData_dto.image;
                //        shopData.ViewPage = shopTireData_dto.page;
                //        shopData.ViewDevice = shopTireData_dto.view;
                //        shopData.LastUser = shopTireData_dto.last_user;
                //        shopData.ImageStatus = shopTireData_dto.status;
                //        shopData.CreatedDate = shopTireData_dto.date_created;
                //        shopData.UpdatedDate = shopTireData_dto.date_updated;
                //        shopData.Hyperlink = shopTireData_dto.ad_hyperlink;

                //        shopTireDataDto.Add(shopData);
                //    }
                //}

                //if (shopTireDataDto.Count > 1)
                //{
                //    var groupList = shopTireDataDto.GroupBy(a => new { a.guid }).Where(g => g.Count() > 1).SelectMany(a => a).ToList();

                //    string storescombined = "";
                //    List<ShopTire> grouped = new List<ShopTire>();
                //    Guid listGuid = Guid.Empty;
                //    var sameguid = Guid.Empty;
                //    foreach (var group in groupList)
                //    {
                //        List<ShopTireDto> groupedList = new List<ShopTireDto>();
                //        groupedList = groupList.Where(x => x.guid == group.guid).ToList();
                //        int countgrp = groupedList.Count();
                //        List<ShopTireDto> shopTireRemoveList = new List<ShopTireDto>();
                //        var guidcheck = group.guid;

                //        var itemguid = Guid.Empty;
                //        foreach (var itemGrouped in groupedList)
                //        {
                //            itemguid = itemGrouped.guid;
                //            if (sameguid == guidcheck)
                //            {
                //                continue;
                //            }
                //            else
                //            {
                //                storescombined += itemGrouped.Stores + ",";

                //                ShopTireDto shopTireRemove = new ShopTireDto();
                //                shopTireRemove = shopTireDataDto.Where(a => a.guid == itemGrouped.guid && a.Stores == itemGrouped.Stores).FirstOrDefault();
                //                //shopTireList.Remove(a => a.store_num == itemGrouped.store_num && a.guid == itemGrouped.guid);
                //                shopTireRemoveList.Add(shopTireRemove);
                //                shopTireDataDto = shopTireDataDto.Where(a => a.guid != itemGrouped.guid && a.Stores != itemGrouped.Stores).ToList();
                //            }


                //        }
                //        sameguid = itemguid;

                //        //}

                //        shopTireDto = new ShopTireDto();
                //        shopTireDto.ShopTireId = group.ShopTireId;
                //        shopTireDto.guid = group.guid;
                //        shopTireDto.Stores = storescombined;
                //        shopTireDto.Title = group.Title;
                //        shopTireDto.LanguageId = group.LanguageId;
                //        shopTireDto.ImageName = group.ImageName;
                //        shopTireDto.ViewPage = group.ViewPage;
                //        shopTireDto.ViewDevice = group.ViewDevice;
                //        shopTireDto.LastUser = group.LastUser;
                //        shopTireDto.ImageStatus = group.ImageStatus;
                //        shopTireDto.CreatedDate = group.CreatedDate;
                //        shopTireDto.UpdatedDate = group.UpdatedDate;
                //        shopTireDto.Hyperlink = group.Hyperlink;
                //        //obj_shopTireList.Stores = storescombined;

                //    }

                //}
                if(id != null)
                {
                    var group = _shopTireAllRepo.FindById(id);
                    shopTireDto = new ShopTireDto();
                    shopTireDto.ShopTireId = group.shopTire_id;
                    shopTireDto.guid = group.guid;
                    if(Convert.ToString(group.store_num) == "-999")
                    {
                        shopTireDto.Stores = "All Stores";
                    }
                    
                    shopTireDto.Title = group.title;
                    shopTireDto.LanguageId = group.language_id;
                    shopTireDto.ImageName = group.image;
                    shopTireDto.ViewPage = group.page;
                    shopTireDto.ViewDevice = group.view;
                    shopTireDto.LastUser = group.last_user;
                    shopTireDto.ImageStatus = group.status;
                    shopTireDto.CreatedDate = group.date_created;
                    shopTireDto.UpdatedDate = group.date_updated;
                    shopTireDto.Hyperlink = group.ad_hyperlink;
                }



                //shopTireDataDto.Add(shopTireDto);



                // Replace with automapper
                //ShopTireDto shopTireDto = new ShopTireDto()
                //{
                //    ShopTireId = shopTireData.shopTire_id,
                //    LanguageId = shopTireData.language_id,
                //    StoreNumber = shopTireData.store_num,
                //    Title = shopTireData.title,
                //    ImageName = shopTireData.image,
                //    ImageStatus = shopTireData.status,
                //    ViewDevice = shopTireData.view,
                //    LastUser = shopTireData.last_user,
                //    ViewPage = shopTireData.page,
                //    Hyperlink = shopTireData.ad_hyperlink,
                //    CreatedDate = shopTireData.date_created,
                //    UpdatedDate = shopTireData.date_updated
                //};
                //shopTireDto = _mapper.Map<ShopTire, ShopTireDto>(shopTireData);
                if (shopTireDto.LanguageId == 1)
                    shopTireDto.Language = "English";
                else if (shopTireDto.LanguageId == 2)
                    shopTireDto.Language = "French";

                return View(shopTireDto);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: ShopTire-All Banner Detail Issue Id: {id}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
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
                        if (spec.banner_type == "ShopTires" && viewDevice.ToLower() == "desktop")
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
                        else if (spec.banner_type == "ShopTires" && viewDevice.ToLower() == "mobile")
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
                        data = new { message = "ShopTire Banner Image is valid.", isSuccessful = true }
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
                    data = new { message = "ShopTire Banner Image Specs Error: " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        //ShoptireAll Publish
        public IActionResult Publish(IFormFile formFile)
        {
            //bool isUploaded = false;
            bool isUpdated = false, isUploaded = false, prodUpdated = false;
            byte[] imageBytes;
            string imageName = string.Empty;
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            tblquecmsimage imageData = new tblquecmsimage();
            ShopTire shopTire = new ShopTire();
            Helper helper = new Helper(_configuration);
            //int imageId = Convert.ToInt32(TempData["imageId"]);
            Guid imageId = new Guid(TempData["imageId"].ToString() ?? "");
            int qImageId = Convert.ToInt32(TempData["qImageId"]);
            int qScriptId = Convert.ToInt32(TempData["qScriptId"]);
            string formMode = (string)TempData["formMode"] ?? "";
            var data = new { message = "", isSuccessfull = false };
            var storeNo = -999;

            try
            {
                imageData = _appRepo.GetImageDetailByID(imageId, qImageId);
                imageName = imageData.img_name.ToString();

                if (!string.IsNullOrEmpty(imageName))
                {
                    BlobURIDto blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                        FolderPath = _configuration.GetSection("ShopTiresPath").Value,
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
                            FolderPath = _configuration.GetSection("ShopTiresPath").Value,
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
                    shopTire = _shopTireAllRepo.FindByGuidStore(imageId, storeNo);

                    if (formMode == "Edit")
                    {
                        prodUpdated = _shopTireAllRepo.UpdateShopTireAllImageProd(imageId,shopTire);
                    }
                    //else if (formMode == "Edit")
                    //{
                    //    prodUpdated = _couponImageRepo.UpdateCouponImageProd(imageId, shopTire);
                    //}
                }

                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "ShopTire-All Image Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing ShopTire-All Image - {imageId} ", isSuccessfull = false }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                         " | Error Message: " + jsonData.data.message + $"ShopTire-All Image Id: {imageId}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "ShopTire-All Image failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                 " | Error Message: " + jsonData.data.message + $"banner Id: {imageId} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }
    }
}
