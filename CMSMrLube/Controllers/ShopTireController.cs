using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MrLubeCMS.CustomHandler;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Tls;
using Serilog;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace MrLubeCMS.Controllers
{
    // [Authorize(Roles ="admin")]
    [Authorize]
    public class ShopTireController : Controller
    {
        private readonly ILogger<ShopTireController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly IShopTireRepository _shopTireRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;
        public ShopTireImage imageShopTire;
        public tblquecmsimageModel imageId;//todo ??

        public ShopTireController(ILogger<ShopTireController> logger, CMSDbContext dbContext,
            IShopTireRepository shopTireRepo, IApplicationRepository appRepo, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _appRepo = appRepo;
            _shopTireRepo = shopTireRepo;
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

        public async Task<IActionResult> CreateAsync(int lang, string view)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                ShopTire shopTire = new ShopTire();

                List<SelectListItem> storeNumbersList = (List<SelectListItem>)await _shopTireRepo.GetStoreNumbersList(lang, view);

                ViewBag.StoreNumbersList = storeNumbersList;

                ViewBag.lang = lang;
                ViewBag.views = view;

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

                return View(shopTire);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: ShopTire Banner Image Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return Problem("Error:MrLubeCMS");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveShopTire(IFormCollection form)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            bool isAdded = false;
            bool isEmpty = false;
            ShopTire shopTire = new ShopTire();

            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
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
                    bool isUploaded = true;
                    var guid = Guid.NewGuid();
                    shopTire.guid = guid;
                    int storeNo = Convert.ToInt32(form["store_num"]);
                    if (form["language"] == "English")
                        shopTire.language_id = 1;
                    else
                        shopTire.language_id = 2;
                    int lang = shopTire.language_id;
                    var device = form["viewDevice"];
                    isEmpty = _appRepo.CheckImageQueue();
                    string selectedStores = form["SelectedStores"];
                    string comastores = "";

                    if(selectedStores == null || selectedStores == "")
                    {
                        var jsonData = new
                        {
                            shopTireImageData = TempData["mainImage"],
                            data = new { message = "Please select the Store Number.", isSuccessfull = false, formMode = "Create", IsStoreExist = true }
                        };

                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"StoreNo: {storeNo}");
                        return Json(jsonData);
                    }

                    IsStoreExist = _appRepo.CheckStoreExist(selectedStores, lang, device, 0,ref comastores);
                    if (IsStoreExist == true)
                    {
                        var jsonData = new
                        {
                            shopTireImageData = TempData["mainImage"],
                            data = new { message = "Store No: " + comastores + " banner already exists. Please delete this banner first and Add again for this store.", isSuccessfull = false, formMode = "Create", IsStoreExist = true }
                        };

                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"StoreNo: {comastores}");
                        return Json(jsonData);
                    }

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

                    if (isUploaded == true && form != null)
                    {
                        //string selectedStores = form["SelectedStores"];
                        shopTire.SelectedStores = selectedStores.Split(",");
                        if (form["language"] == "English")
                            shopTire.language_id = 1;
                        else
                            shopTire.language_id = 2;
                        //shopTire.store_num = Convert.ToInt32(form["SelectedStores"]);
                        shopTire.title = form["storeTitle"];
                        shopTire.image = fileName;
                        shopTire.page = "ShopTire store page " + Convert.ToInt32(form["store_num"]);
                        shopTire.ad_hyperlink = Convert.ToString(form["imageHyperLink"]).Replace("'","");
                        shopTire.view = form["viewDevice"];
                        shopTire.status = form["imageStatus"];
                        // shopTire.last_user = "admin"; //TODO: add login user
                        shopTire.last_user = "";
                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                            shopTire.last_user = user;
                        }

                        shopTire.date_created = DateTime.Now;
                        shopTire.date_updated = DateTime.Now;

                        isAdded = _shopTireRepo.Add(shopTire);
                        //if (isAdded)
                        //    isAdded = _shopTireRepo.SaveScriptAndData(formMode, shopTire, ref queuesDto, user);
                    }

                    TempData["mainImage"] = fileName;
                    TempData["imageId"] = shopTire.guid;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["formMode"] = "Create";

                    if (isAdded == true)
                    {
                        //var data = new { shopTireImageData = TempData["mainImage"], model = shopTire, message = "ShopTire uploaded successfully.", isSuccessful = true };
                        var jsonData = new
                        {
                            shopTireImageData = TempData["mainImage"],
                            data = new { message = " ShopTire banner uploaded successfully.", isSuccessfull = true, formMode = "Create", isEmpty }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            shopTireImageData = TempData["mainImage"],
                            data = new { message = "There was an Error on Uploading ShopTire banner.", isSuccessfull = false, formMode = "Create" }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"ShopTire Id: {shopTire.guid}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        shopTireImageData = TempData["mainImage"],
                        data = new { message = $"There was an Error on Uploading ShopTire banner. {shopTire.guid}", isSuccessfull = false, formMode = "Create" }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                        " | Error Message: " + jsonData.data.message + $"ShopTire Id: {shopTire.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                var jsonData = new
                {
                    data = new { message = $"Error: There was an Error on Uploading ShopTire banner. {shopTire.guid} " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                    ControllerContext.ActionDescriptor.ActionName + $" | StackTrace: {ex.StackTrace} | Msg: " + jsonData.data.message );
                _logger.LogWarning("There was an error on uploading the image.");
                return Json(jsonData);
            }
        }

        public async Task<ActionResult> ShopTireList(string? Stores, string? Title, string? ImageName, string? ImageStatus, string? ViewDevice, int LanguageId)
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
                List<ShopTireDto> shopTireList = new List<ShopTireDto>();
                ShopTireDto shopTireDto = new ShopTireDto()
                {
                    Stores = Stores,
                    Title = Title,
                    ImageName = ImageName,
                    ImageStatus = ImageStatus,
                    ViewDevice = ViewDevice,
                    LanguageId = LanguageId
                };
                var selected = "";
                GetData(shopTireDto, ref shopTireList);
                //foreach(var select in shopTireList)
                //{
                //    selected = string.Join(",",Convert.ToString(select.store_num).AsQueryable().GroupBy(Convert.ToString(select.guid)));
                //}
                //var s = string.Join(",", (from sh in shopTireList  select new {
                //    sh.store_num

                //})).ToList();

                //var r = shopTireList.ToList().Select(x => new
                //{
                //    Tags = String.Join(", ", x.store_num)
                //});



                List<ShopTireDto> dto_shopTireList = new List<ShopTireDto>();
                
                Guid curr_guid = Guid.Empty;
                string guid = "";
                
                var groupList = shopTireList.GroupBy(a => new { a.guid }).Where(g => g.Count() > 1).SelectMany(a => a).ToList();

                List<ShopTire> grouped = new List<ShopTire>();
                Guid listGuid = Guid.Empty;
                var sameguid = Guid.Empty;
                foreach (var group in groupList)
                {

                    List<ShopTireDto> groupedList = new List<ShopTireDto>();
                    groupedList = groupList.Where(x => x.guid == group.guid).ToList();
                    int countgrp = groupedList.Count();
                    List<ShopTireDto> shopTireRemoveList = new List<ShopTireDto>();
                    var guidcheck = group.guid;
                    ShopTireDto obj_shopTireList = new ShopTireDto();
                    string storescombined = "";
                    //for (int i = 0; i >= 0 ; --countgrp)
                    //{
                    if (sameguid == guidcheck)
                    {
                        continue;
                    }
                    else
                    {
                        var itemguid = Guid.Empty;
                        foreach (var itemGrouped in groupedList)
                        {
                            itemguid = itemGrouped.guid;

                            if(countgrp == 1)
                            {
                                storescombined += itemGrouped.Stores;
                            }
                            //else
                            //{
                            else
                            {
                                storescombined += itemGrouped.Stores + ",";
                            }
                            
                            countgrp --;
                            ShopTireDto shopTireRemove = new ShopTireDto();
                            shopTireRemove = shopTireList.Where(a => a.guid == itemGrouped.guid && a.Stores == itemGrouped.Stores).FirstOrDefault();
                            //shopTireList.Remove(a => a.store_num == itemGrouped.store_num && a.guid == itemGrouped.guid);
                            shopTireRemoveList.Add(shopTireRemove);
                            shopTireList = shopTireList.Where(a => a.guid != itemGrouped.guid).ToList();
                            //}


                        }
                        sameguid = itemguid;

                        //}

                        obj_shopTireList = new ShopTireDto();
                        obj_shopTireList.ShopTireId = group.ShopTireId;
                        obj_shopTireList.guid = group.guid;
                        obj_shopTireList.Stores = storescombined;
                        obj_shopTireList.Title = group.Title;
                        obj_shopTireList.LanguageId = group.LanguageId;
                        obj_shopTireList.ImageName = group.ImageName;
                        obj_shopTireList.ViewPage = group.ViewPage;
                        obj_shopTireList.ViewDevice = group.ViewDevice;
                        //obj_shopTireList.LastUser = group.LastUser;
                        obj_shopTireList.ImageStatus = group.ImageStatus;
                        //obj_shopTireList.CreatedDate = group.CreatedDate;
                        //obj_shopTireList.UpdatedDate = group.UpdatedDate;
                        obj_shopTireList.Hyperlink = group.Hyperlink;
                        //obj_shopTireList.Stores = storescombined;
                    }


                    //shopTireList = shopTireList.Where(a=>a.guid != group.guid).ToList();




                    //if(listGuid == Guid.Empty )
                    //{
                    //    listGuid = group.guid;
                    //}
                    //if(listGuid == group.guid )
                    //{
                    //    guid = group.guid + ",";

                    //}
                    //else
                    //{
                    //    listGuid = group.guid;  
                    //}
                    shopTireList.Add(obj_shopTireList);
                }


                //var test2NotInTest1 = shopTireList.Where(t1 => !groupList.Any(t2 => t2.Contains(t2))).ToList();
                //foreach (var item in shopTireList.OrderBy(a => a.guid))
                //{


                //    //if(groupList.Count > 0) {
                //    //    if()
                //    //}
                //    //foreach(var dubs in groupList)
                //    //{
                //    //    ShopTireDto obj_groupedshopList = new ShopTireDto();

                //    //    if()
                //    //}


                //    obj_shopTireList.guid = item.guid;
                //    //obj_shopTireList.ViewDevice = item.view;
                //    //obj_shopTireList.LanguageId = item.language_id;
                //    if (curr_guid == Guid.Empty)
                //    {
                //        curr_guid = item.guid;
                //    }

                //    if (item.guid == curr_guid)
                //    {
                //        if (guid == "")
                //        {
                //            guid = "" + item.store_num;
                //        }
                //        else
                //        {
                //            guid += "," + item.store_num;
                //        }

                //    }
                //    else
                //    {
                //        //obj_shopTireList.Stores = guid;
                //        //dto_shopTireList.Add(obj_shopTireList);
                //        curr_guid = item.guid;
                //        //obj_shopTireList = new ShopTireDto();
                //    }
                //}
                //dto_shopTireList.Add(obj_shopTireList);

                totalRows = shopTireList.Count;
                string lang = searchValue.ToString().Contains("english".ToLower()) ? "1" : searchValue.ToString().Contains("french".ToLower()) ? "2" : searchValue;
                //if (!string.IsNullOrEmpty(searchValue))
                //{
                //    shopTireList = shopTireList.Where(x => x.Stores.Contains(searchValue.ToLower()) || x.Title.Contains(searchValue.ToLower()) ||
                //            x.ImageName.Contains(searchValue.ToLower()) || x.ImageStatus.Contains(searchValue.ToLower()) ||
                //            x.ViewDevice.Contains(searchValue.ToLower()) || x.Hyperlink.Contains(searchValue.ToLower()) || 
                //            x.LanguageId.ToString().Contains(lang)).ToList();
                //}


                totalFilteredRows = shopTireList.Count;
                //Sorting
                //shopTireList = shopTireList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();
                ////Paging
                //shopTireList = shopTireList.Skip(start).Take(length).ToList();




                var jsonData = new
                {
                    data = shopTireList,
                    draw = Request.Form["draw"],
                    recordsTotal = totalRows,
                    recordsFiltered = totalFilteredRows
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

        public IActionResult Details(Guid id)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                ShopTireDto shopTireDto = null;
                List<ShopTire> shopTireData = new List<ShopTire>();
                List<ShopTireDto> shopTireDataDto = new List<ShopTireDto>();
                if (id != null)
                {
                    shopTireData = _shopTireRepo.FindByIdList(id);

                    foreach (var shopTireData_dto in shopTireData)
                    {
                        ShopTireDto shopData = new ShopTireDto();
                        shopData.ShopTireId = shopTireData_dto.shopTire_id;
                        shopData.guid = shopTireData_dto.guid;
                        shopData.LanguageId = shopTireData_dto.language_id;
                        shopData.Stores = Convert.ToString(shopTireData_dto.store_num);
                        shopData.Title = shopTireData_dto.title;
                        shopData.ImageName = shopTireData_dto.image;
                        shopData.ViewPage = shopTireData_dto.page;
                        shopData.ViewDevice = shopTireData_dto.view;
                        shopData.LastUser = shopTireData_dto.last_user;
                        shopData.ImageStatus = shopTireData_dto.status;
                        shopData.CreatedDate = shopTireData_dto.date_created;
                        shopData.UpdatedDate = shopTireData_dto.date_updated;
                        shopData.Hyperlink = shopTireData_dto.ad_hyperlink;

                        shopTireDataDto.Add(shopData);
                    }
                }

                if (shopTireDataDto.Count > 1)
                {
                    var groupList = shopTireDataDto.GroupBy(a => new { a.guid }).Where(g => g.Count() > 1).SelectMany(a => a).ToList();

                    string storescombined = "";
                    List<ShopTire> grouped = new List<ShopTire>();
                    Guid listGuid = Guid.Empty;
                    var sameguid = Guid.Empty;
                    foreach (var group in groupList)
                    {
                        List<ShopTireDto> groupedList = new List<ShopTireDto>();
                        groupedList = groupList.Where(x => x.guid == group.guid).ToList();
                        int countgrp = groupedList.Count();
                        List<ShopTireDto> shopTireRemoveList = new List<ShopTireDto>();
                        var guidcheck = group.guid;

                        var itemguid = Guid.Empty;
                        foreach (var itemGrouped in groupedList)
                        {
                            itemguid = itemGrouped.guid;
                            if (sameguid == guidcheck)
                            {
                                continue;
                            }
                            else
                            {
                                storescombined += itemGrouped.Stores + ",";

                                ShopTireDto shopTireRemove = new ShopTireDto();
                                shopTireRemove = shopTireDataDto.Where(a => a.guid == itemGrouped.guid && a.Stores == itemGrouped.Stores).FirstOrDefault();
                                //shopTireList.Remove(a => a.store_num == itemGrouped.store_num && a.guid == itemGrouped.guid);
                                shopTireRemoveList.Add(shopTireRemove);
                                shopTireDataDto = shopTireDataDto.Where(a => a.guid != itemGrouped.guid && a.Stores != itemGrouped.Stores).ToList();
                            }


                        }
                        sameguid = itemguid;

                        //}

                        shopTireDto = new ShopTireDto();
                        shopTireDto.ShopTireId = group.ShopTireId;
                        shopTireDto.guid = group.guid;
                        shopTireDto.Stores = storescombined;
                        shopTireDto.Title = group.Title;
                        shopTireDto.LanguageId = group.LanguageId;
                        shopTireDto.ImageName = group.ImageName;
                        shopTireDto.ViewPage = group.ViewPage;
                        shopTireDto.ViewDevice = group.ViewDevice;
                        shopTireDto.LastUser = group.LastUser;
                        shopTireDto.ImageStatus = group.ImageStatus;
                        shopTireDto.CreatedDate = group.CreatedDate;
                        shopTireDto.UpdatedDate = group.UpdatedDate;
                        shopTireDto.Hyperlink = group.Hyperlink;
                        //obj_shopTireList.Stores = storescombined;

                    }

                }
                else
                {
                    var group = _shopTireRepo.FindById(id);
                    shopTireDto = new ShopTireDto();
                    shopTireDto.ShopTireId = group.shopTire_id;
                    shopTireDto.guid = group.guid;
                    shopTireDto.Stores = Convert.ToString(group.store_num);
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
                data = new { message = $"Error: ShopTire Banner Detail Issue Id: {id}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        public async Task<IActionResult> Edit(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                ShopTire shopTire = new ShopTire();
                shopTire = _shopTireRepo.FindById(guid);
                List<SelectListItem> storeNumbersList = (List<SelectListItem>)await _shopTireRepo.GetStoreNumbersEditList(guid, shopTire);

                ViewBag.StoreNumbersList = storeNumbersList;



                shopTire.SelectedStores = shopTire.SelectedStores;

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

                return View(shopTire);

            }
            catch (Exception ex)
            {
                data = new { message = $"Error: ShopTire Image Specs Issue Id: {guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            bool isDeleted = false;
            bool isAdded = false;
            string formMode = "Delete";
            string bannerType = "ShopTire";
            string user = "";
            ShopTire shopTireData = new ShopTire();
            Helper helper = new Helper(_configuration);
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            try
            {
                //delete from server
                if (id != null)
                {
                    shopTireData = _shopTireRepo.FindById(id);
                    List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();
                    if (!string.IsNullOrEmpty(shopTireData.image))
                    {
                        var checkdelFile = _appRepo.isFilependingbanner(id, bannerType, ref tblquecmsimage);
                        bool checkprodFile = _shopTireRepo.CheckFileOnProd(shopTireData);
                        if (tblquecmsimage.Count > 0)
                        {
                            foreach (var item in tblquecmsimage)
                            {
                                _appRepo.RemoveImgQueData(item.img_queId);
                            }


                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("ShopTiresPath").Value,
                                FileName = shopTireData.image.ToString()
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                            isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(shopTireData.image.ToString(), "ShopTire", "Dev");
                            //string Filedelimg = shopTireData.image;
                            //isDeleted = AzureService.FTPDeleteFile(Filedelimg, azureFTPDto);

                            if (isDeleted)
                            {
                                isDeleted = _shopTireRepo.Update(shopTireData, formMode);
                                if (checkprodFile == true && isDeleted == true)
                                {
                                    if (HttpContext.User.Identity != null)
                                    {
                                        user = HttpContext.User.Identity.Name ?? "";
                                    }
                                    List<ShopTire> shopTireList = new List<ShopTire>();
                                    shopTireList = _shopTireRepo.FindByIdList(shopTireData.guid);

                                    foreach (ShopTire shopTireRec in shopTireList)
                                    {
                                        isAdded = _shopTireRepo.SaveScriptAndData(formMode, shopTireRec, ref queuesDto, user);
                                    }
                                    //_shopTireRepo.SaveImageDetails(shopTireData, ref queuesDto, formMode);

                                }
                                //isUpdate = _appRepo.SaveQueDataWithnoImage(model, ref tblimgqry, ref tblquery, "NoImage");

                            }
                            if (isDeleted == true)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "ShopTire Banner-" + shopTireData.store_num + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting ShopTire Banner Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {id}");
                                return Json(jsonData);
                            }
                        }

                        else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value, //confirm logic prod or stage?
                                FolderPath = _configuration.GetSection("ShopTiresPath").Value,
                                FileName = shopTireData.image.ToString()
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                            isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(shopTireData.image.ToString(), "ShopTire", "Dev");
                            //string Filedel = shopTireData.image;
                            //isDeleted = AzureService.FTPDeleteFile(Filedel, azureFTPDto);
                            //string user = "";
                            if (isDeleted)
                                isDeleted = _shopTireRepo.Update(shopTireData, formMode);
                            if (checkprodFile == true && isDeleted == true)
                            {

                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                List<ShopTire> shopTireList = new List<ShopTire>();
                                shopTireList = _shopTireRepo.FindByIdList(shopTireData.guid);

                                foreach (ShopTire shopTireRec in shopTireList)
                                {
                                    isAdded = _shopTireRepo.SaveScriptAndData(formMode, shopTireRec, ref queuesDto, user);
                                }
                            }
                            if (isDeleted == true && isAdded == true)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "ShopTire Banner-" + shopTireData.store_num + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting ShopTire Banner Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {id}");
                                return Json(jsonData);
                            }
                        }

                        else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
                        {
                            BlobURIDto blobURIDto = new BlobURIDto()
                            {
                                ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                                FolderPath = _configuration.GetSection("ShopTiresPath").Value,
                                FileName = shopTireData.image.ToString()
                            };
                            string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                            string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                            //if (!string.IsNullOrEmpty(result))
                            isDeleted = true;

                            //azureFTPDto = helper.GenerateAzureUri(shopTireData.image.ToString(), "ShopTire", "Dev");
                            //string Filedel = shopTireData.image;
                            //isDeleted = AzureService.FTPDeleteFile(Filedel, azureFTPDto);

                            if (isDeleted)
                                isDeleted = _shopTireRepo.Update(shopTireData, formMode);

                            if (isDeleted)
                            {
                                var jsonData = new
                                {
                                    data = new { message = "ShopTire Banner-" + shopTireData.store_num + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                                };
                                return Json(jsonData);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "There was an Error on Deleting ShopTire Banner Id: " + id, isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {id}");
                                return Json(jsonData);
                            }

                        }

                        else
                        {
                            var jsonData = new
                            {
                                data = new { message = "There was an Error on Deleting ShopTire Banner Id: " + id, isSuccessfull = false }

                            };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {id}");
                            return Json(jsonData);
                        }

                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting ShopTire Banner Id: " + id, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {id}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "There was an Error on Deleting ShopTire Banner Id: " + id, isSuccessfull = false }

                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"ShopTire banner Id: {id}");
                    return Json(jsonData);
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                Log.Information("ShopTireController");
                Log.Error(ex.ToString());
                var jsonData = new
                {
                    data = new { message = "There is an Error on Deleting ShopTire Banner Id: " + id + " | StakTrace: " + ex.Message, isSuccessfull = false }

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
                    string selectedStores = formValue["SelectedStores"];
                    
                    if (formValue["language"] == "English")
                        shopTire.language_id = 1;
                    else
                        shopTire.language_id = 2;
                    int lang = shopTire.language_id;
                    var device = formValue["viewDevice"];
                    int shopTire_id = Convert.ToInt32(formValue["shopTireID"]);
                    isEmpty = _appRepo.CheckImageQueue();


                    if (selectedStores == null || selectedStores == "")
                    {
                        var jsonData = new
                        {
                            shopTireImageData = TempData["mainImage"],
                            data = new { message = "Please select the Store Number.", isSuccessfull = false, formMode = "Create", IsStoreExist = true }
                        };

                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"StoreNo: {storeNo}");
                        return Json(jsonData);
                    }

                    //IsStoreExist = _appRepo.CheckStoreExist(storeNo, lang, device, shopTire_id);
                    //if (IsStoreExist == true)
                    //{
                    //    var jsonData = new
                    //    {
                    //        shopTireImageData = TempData["mainImage"],
                    //        data = new { message = "Store No: " + storeNo + " banner already exists. Please delete this banner first and Add again for this store.", isSuccessfull = false, formMode = "Create", IsStoreExist = true }
                    //    };
                    //    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                    //      " | Error Message: " + data.message + $"banner Id: {shopTire.guid}");
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
                        shopTire.SelectedStores = selectedStores.Split(",");
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

                        shopTire.page = "ShopTire store page " + Convert.ToInt32(formValue["store_num"]);
                        shopTire.ad_hyperlink = Convert.ToString(formValue["imageHyperLink"]).Replace("'","");
                        shopTire.view = formValue["viewDevice"];
                        shopTire.status = formValue["imageStatus"];
                        if (HttpContext.User.Identity != null)
                        {
                            shopTire.last_user = HttpContext.User.Identity.Name ?? "";
                        }
                        shopTire.date_created = DateTime.Now;
                        shopTire.date_updated = DateTime.Now;

                        isUpdated = _shopTireRepo.Update(shopTire, string.Empty);

                        //string user = "";
                        //if (HttpContext.User.Identity != null)
                        //{
                        //    user = HttpContext.User.Identity.Name ?? "";
                        //}

                        //if (isUpdated)
                        //    isUpdated = _shopTireRepo.SaveScriptAndData(formMode, shopTire, ref queuesDto, user);
                    }

                    //if (formValue != null && isUploaded == true)
                    //{
                    //    shopTire.shopTire_id = Convert.ToInt32(formValue["shopTireID"]);
                    //    if (formValue["language"] == "English")
                    //        shopTire.language_id = 1;
                    //    else
                    //        shopTire.language_id = 2;
                    //    shopTire.store_num = Convert.ToInt32(formValue["storeNum"]);
                    //    shopTire.title = formValue["storeTitle"];
                    //    if (image != null)
                    //    {
                    //        shopTire.image = image.FileName;
                    //    }
                    //    else { shopTire.image = formValue["imageUpload"].ToString(); }
                    //    shopTire.page = "ShopTire store page " + Convert.ToInt32(formValue["storeNum"]);
                    //    shopTire.ad_hyperlink = formValue["imageHyperLink"];
                    //    shopTire.view = formValue["viewDevice"];
                    //    shopTire.status = formValue["imageStatus"];
                    //    //shopTire.last_user = "admin"; //TODO: add login user
                    //    if (HttpContext.User.Identity != null)
                    //    {
                    //        shopTire.last_user = HttpContext.User.Identity.Name ?? "admin";
                    //    }
                    //    shopTire.date_created = DateTime.Now;
                    //    shopTire.date_updated = DateTime.Now;

                    //    isUpdated = _shopTireRepo.Update(shopTire, string.Empty);

                    //    string user = "";
                    //    if (HttpContext.User.Identity != null)
                    //    {
                    //        user = HttpContext.User.Identity.Name ?? "admin";
                    //    }

                    //    if (isUpdated)
                    //        isUpdated = _shopTireRepo.SaveScriptAndData(formMode, shopTire, ref queuesDto, user);
                    //}

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
            ShopTire shopTire = new ShopTire();
            Guid imageId = new Guid(TempData["imageId"].ToString() ?? "");
            int qImageId = Convert.ToInt32(TempData["qImageId"]);
            int qScriptId = Convert.ToInt32(TempData["qScriptId"]);
            string formMode = (string)TempData["formMode"] ?? "";
            var data = new { message = "", isSuccessfull = false };

            try
            {
                imageData = _appRepo.GetImageDetailByGUID(imageId);
                imageName = imageData.img_name.ToString();

                if (!string.IsNullOrEmpty(imageName))
                {
                    blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                        FolderPath = _configuration.GetSection("ShopTiresPath").Value,
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
                            FolderPath = _configuration.GetSection("ShopTiresPath").Value,
                            FormFile = fileForm,
                            FileName = imageName
                        };
                        endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);

                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            isUploaded = true;
                    }

                    //isUploaded = _shopTireRepo.uploadImagetoFTPServer(imageName);
                    //azureFTPDto = helper.GenerateAzureUri(imageName, "ShopTire", "Dev");
                    //imageBytes = AzureService.FTPDownloader(azureFTPDto);

                    //azureFTPDto = helper.GenerateAzureUri(imageName, "ShopTire", "Prod");
                    //isUploaded = AzureService.FTPBytesUploader(imageBytes, azureFTPDto);

                    if (isUploaded)
                    {
                        isUpdated = _appRepo.UpdateShoptireImageQueByGUID(imageId);
                        isUpdated = _appRepo.UpdateScriptQueueDetailByGUID(imageId);
                    }
                }

                if (isUploaded == true && isUpdated == true)
                {
                    List<ShopTire> ShopTires = new List<ShopTire>();
                    ShopTires = _shopTireRepo.GetShopTireByGUID(imageId).ToList();

                    if (formMode == "Create")
                    {
                        prodUpdated = _shopTireRepo.InsertShopTireProd(ShopTires);
                    }
                    else if (formMode == "Edit")
                    {
                        prodUpdated = _shopTireRepo.UpdateShopTireProd(imageId, ShopTires);
                    }
                }

                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "ShopTire Banner Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing ShopTire Banner - {imageId}", isSuccessfull = false }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"banner Id: {imageId}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "ShopTire banner failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                  " | Error Message: " + jsonData.data.message + $"banner Id: {imageId} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }

        /// <summary>
        /// Populate list of shoptires with database data.
        /// </summary>
        /// <param name="shopTireDto"></param>
        /// <param name="shopTireList"></param>
        public void GetData(ShopTireDto shopTireDto, ref List<ShopTireDto> shopTireList)
        {
            try
            {
                ShopTireDto shopTire = new ShopTireDto
                {
                    Stores = shopTireDto.Stores,
                    Title = shopTireDto.Title,
                    ImageName = shopTireDto.ImageName,
                    ImageStatus = shopTireDto.ImageStatus,
                    ViewDevice = shopTireDto.ViewDevice,
                    Hyperlink = shopTireDto.Hyperlink
                };

                List<ShopTireDto> data = _shopTireRepo.GetShopTireList(shopTire);

                shopTireList = data;

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

        //public DateTime ConvertDatebyUserTimezone(string UserTimeZone, DateTime SrcDate)
        //{
        //    UserTimeZone = UserTimeZone.ToLower();  
        //    DateTime Returndate = SrcDate;
        //    TimeZoneInfo Serverzone = null;
        //    System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
        //    foreach (TimeZoneInfo timeZoneInfo in timeZones)
        //    {
        //        if (timeZoneInfo.ToString().Contains(ToConvertTimezone))
        //        {
        //            Serverzone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo.Id);
        //            break;
        //        }
        //    }
        //    if (UserTimeZone != "")
        //    {
        //        foreach (TimeZoneInfo timeZoneInfo in timeZones)
        //        {
        //            if (timeZoneInfo.ToString().Contains(UserTimeZone))
        //            {
        //                TimeZoneInfo timez = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo.Id);
        //                Returndate = TimeZoneInfo.ConvertTime(SrcDate, ToConvertTimezone, timez);
        //                break;
        //            }
        //        }
        //    }
        //    return Returndate;
        //}



    }
}
