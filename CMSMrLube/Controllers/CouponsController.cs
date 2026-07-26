using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MrLubeCMS.CustomHandler;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;


namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class CouponsController : Controller
    {
        private readonly ILogger<CouponsController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly ICouponsRepository _couponsRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;
        public tblquecmsimageModel imageId;//todo ??

        public CouponsController(ILogger<CouponsController> logger, CMSDbContext dbContext,
            ICouponsRepository couponsRepo, IApplicationRepository appRepo, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _appRepo = appRepo;
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

        public async Task<ActionResult> CouponsList()
        {
            try
            {
                List<CouponPages> couponsList = new List<CouponPages>();
                CouponPages objCoupon = new CouponPages();
                int i = 0;
                foreach (var item in _couponsRepo.GetCouponPagesList().OrderBy(a => a.itemId))
                {
                    i = i + 1;
                    if (i % 2 != 0)
                    {
                        objCoupon = new CouponPages();
                        objCoupon = item;
                    }
                    else
                    {
                        objCoupon.frenchTitle = item.title;
                        couponsList.Add(objCoupon);
                    }
                }

                var jsonData = new
                {
                    data = couponsList
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "Coupon Page List Error: There was an Error on Listing Coupon Page." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return base.View("Error");
            }
        }

        public IActionResult Create()
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                CouponPages pg = new CouponPages();
                return View(pg);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Coupon Page Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCoupon(IFormCollection form)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            bool isAdded = false;
            bool isEmpty = false;
            CouponPages coupon = new CouponPages();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            Helper helper = new Helper(_configuration);
            // AzureFTPDto azureFTPDto = new AzureFTPDto();

            try
            {
                if (ModelState.IsValid)
                {
                    string fileName = "";
                    isEmpty = _appRepo.CheckImageQueue();

                    if (form != null)
                    {
                        coupon.guid = Guid.NewGuid();
                        var alreadyExists = _couponsRepo.TitleExists(Guid.Empty, form["englishTitle"], form["frenchTitle"]);
                        if (alreadyExists.Count() > 0)
                        {
                            var jsonData1 = new
                            {
                                data = new { message = "Title exists already.", isSuccessfull = false, formMode = "Create" }
                            };
                            return Json(jsonData1);
                        }
                        else
                        {
                            int itemId = _couponsRepo.MaxItemId();
                            itemId = itemId + 1;
                            for (int i = 0; i < 2; i++)
                            {
                                coupon.coupon_page_id = 0;
                                if (i == 0)
                                {
                                    coupon.title = form["englishTitle"];
                                    coupon.language_id = 1;
                                }
                                else if (i == 1)
                                {
                                    coupon.title = form["frenchTitle"];
                                    coupon.language_id = 2;
                                }
                                coupon.itemId = itemId;
                                coupon.date_expired = Convert.ToDateTime(form["date_expired"]);
                                coupon.url_Key = Convert.ToString(form["englishTitle"]).Replace(" ", "-").ToLower();
                                coupon.url_Key = coupon.url_Key.Replace("'", "");
                                coupon.status = form["imageStatus"];
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                    coupon.last_user = HttpContext.User.Identity.Name;
                                }
                                coupon.last_user = coupon.last_user ?? "";

                                coupon.date_created = DateTime.Now;
                                coupon.date_updated = DateTime.Now;
                                isAdded = _couponsRepo.Add(coupon);
                                if (isAdded)
                                    isAdded = _couponsRepo.SaveScriptAndData(formMode, coupon, ref queuesDto, user);
                                if (i == 0)
                                {
                                    TempData["qScriptIdEng"] = queuesDto.ScriptQueueId;
                                }
                                else if (i == 1)
                                {
                                    TempData["qScriptIdFr"] = queuesDto.ScriptQueueId;
                                }
                            }
                        }
                    }

                    // TempData["mainImage"] = image;
                    TempData["imageId"] = coupon.coupon_page_id;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["guid"] = coupon.guid;
                    TempData["formMode"] = "Create";

                    var jsonData = new
                    {
                        data = new { message = "Coupon Saved successfully.", isSuccessfull = true, formMode = "Create", isEmpty }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "There was an error on uploading the Coupon Page. Id = " + coupon.guid, isSuccessfull = false, formMode = "Create" }
                    };
                    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + data.message + $"Coupon Page Id: {coupon.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "There was an error on uploading the Coupon Page. Error = " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                   ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the Page.");
                return Json(jsonData);
            }
        }

        public IActionResult Edit(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                CouponPages pg = new CouponPages();
                foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                {
                    if (item.language_id == 1)
                    {
                        pg = item;
                    }
                    else if (item.language_id == 2)
                    {
                        pg.frenchTitle = item.title;
                    }
                }

                // pg = _couponsRepo.FindByItemId(id);
                return View(pg);

            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Coupon Page Specs Issue Id: {guid}", isSuccessfull = false };
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
            CouponPages coupon = new CouponPages();
            Helper helper = new Helper(_configuration);
            // AzureFTPDto azureFTPDto = new AzureFTPDto();
            isEmpty = _appRepo.CheckImageQueue();
            try
            {
                if (ModelState.IsValid)
                {
                    string fileName = "";
                    //var image = Request.Form?.Files?.GetFile("imageFile");
                    bool isUploaded = false;

                    if (formValue != null)
                    {
                        coupon.guid = new Guid(formValue["guid"]);
                        var alreadyExists = _couponsRepo.TitleExists(coupon.guid, formValue["englishTitle"], formValue["frenchTitle"]);


                        if (alreadyExists.Count() > 0)
                        {
                            var jsonData1 = new
                            {
                                data = new { message = "Title exists already.", isUpdate = false, formMode = "Edit" }
                            };
                            return Json(jsonData1);
                        }
                        else
                        {
                            foreach (var item in _couponsRepo.FindByGuidID(coupon.guid).OrderBy(a => a.language_id))
                            {
                                coupon.coupon_page_id = item.coupon_page_id;
                                coupon.itemId = item.itemId;
                                coupon.language_id = item.language_id;
                                coupon.url_Key = item.url_Key;
                                if (item.language_id == 1)
                                {
                                    coupon.title = formValue["englishTitle"];
                                }
                                else if (item.language_id == 2)
                                {
                                    coupon.title = formValue["frenchTitle"];
                                }

                                coupon.date_expired = Convert.ToDateTime(formValue["date_expired"]);
                                coupon.status = formValue["imageStatus"];

                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                    coupon.last_user = HttpContext.User.Identity.Name;
                                }
                                coupon.last_user = coupon.last_user ?? "";
                                coupon.date_updated = DateTime.Now;
                                isUpdated = _couponsRepo.Update(coupon, string.Empty);
                                if (isUpdated)
                                    isUpdated = _couponsRepo.SaveScriptAndData(formMode, coupon, ref queuesDto, user);
                                if (item.language_id == 1)
                                {
                                    TempData["qScriptIdEng"] = queuesDto.ScriptQueueId;
                                }
                                else if (item.language_id == 2)
                                {
                                    TempData["qScriptIdFr"] = queuesDto.ScriptQueueId;
                                }
                            }
                        }
                    }

                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["formMode"] = "Edit";
                    TempData["guid"] = coupon.guid;
                    var jsonData = new
                    {
                        data = new { message = "Coupon updated successfully.", isUpdate = true, formMode = "Edit", isEmpty }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {

                        data = new { message = $"Coupon Page Updating Error: There was an Error on Updating Coupon Page: {coupon.guid}", isUpdate = false, formMode = "Edit" }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + data.message + $"Coupon Id: {coupon.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Coupon Page Could not be Uploaded." + ex.Message, isUpdate = false, formMode = "Edit" }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        public IActionResult Details(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                CouponPages coupon = new CouponPages();

                foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                {
                    if (item.language_id == 1)
                    {
                        coupon = item;
                    }
                    else if (item.language_id == 2)
                    {
                        coupon.frenchTitle = item.title;
                    }
                }
                // Replace with automapper
                CouponPagesDto couponDto = new CouponPagesDto()
                {
                    coupon_page_id = coupon.coupon_page_id,
                    LanguageId = coupon.language_id,
                    Title = coupon.title,
                    frenchTitle = coupon.frenchTitle,
                    ImageStatus = coupon.status,
                    LastUser = coupon.last_user,
                    CreatedDate = coupon.date_created,
                    UpdatedDate = coupon.date_updated,
                    date_expired = coupon.date_expired,
                    status = coupon.status,
                    ItemId = coupon.itemId

                };

                return View(couponDto);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Coupon Page Detail Issue Id: {guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Delete(Guid guid)
        {
            bool isDeleted = false;
            bool isAdded = false;
            string formMode = "Delete";
            string bannerType = "couponPage";
            CouponPages couponsData = new CouponPages();
            Helper helper = new Helper(_configuration);
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            CouponPages couponPages = new CouponPages();
            try
            {
                couponPages = _couponsRepo.FindByLangGuid(guid);
                List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();
                var checkdelFile = _appRepo.isFilependingbanner(guid, bannerType, ref tblquecmsimage);
                bool checkprodFile = _couponsRepo.CheckFileOnProd(couponPages);
                if (tblquecmsimage.Count > 0)
                {
                    foreach (var prod in tblquecmsimage)
                    {
                        _appRepo.RemoveImgQueData(prod.img_queId);
                    }
                    foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                    {
                        couponsData = _couponsRepo.FindById(item.coupon_page_id);

                        isDeleted = _couponsRepo.Update(item, formMode);

                        if (isDeleted)
                        {
                            if (checkprodFile == true && isDeleted == true)
                            {
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                isDeleted = _couponsRepo.SaveScriptAndData(formMode, couponsData, ref queuesDto, user);
                            }
                            
                        }
                    }

                    if (isDeleted == true)
                    {
                        var jsonData = new
                        {
                            data = new { message = "Coupon-" + couponsData.url_Key + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Coupon Image Id: " + guid, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Coupon Id: {guid}");
                        return Json(jsonData);
                    }
                }
                else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
                {
                    foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                    {
                        couponsData = _couponsRepo.FindById(item.coupon_page_id);

                        isDeleted = _couponsRepo.Update(item, formMode);

                        if (isDeleted)
                        {
                            if (checkprodFile == true && isDeleted == true)
                            {
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                isDeleted = _couponsRepo.SaveScriptAndData(formMode, couponsData, ref queuesDto, user);
                            }
                            
                        }
                    }

                    if (isDeleted == true )
                    {
                        var jsonData = new
                        {
                            data = new { message = "Coupon-" + couponsData.url_Key + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Coupon Image Id: " + guid, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Coupon Id: {guid}");
                        return Json(jsonData);
                    }
                }

                else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
                {
                    foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                    {
                        couponsData = _couponsRepo.FindById(item.coupon_page_id);

                        isDeleted = _couponsRepo.Update(item, formMode);

                    }

                    if (isDeleted == true)
                    {
                        var jsonData = new
                        {
                            data = new { message = "Coupon-" + couponsData.url_Key + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Coupon Image Id: " + guid, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Coupon Id: {guid}");
                        return Json(jsonData);
                    }
                }

                    return View();
            }
            catch (Exception ex)
            {

                var jsonData = new
                {
                    data = new { message = "Error: Coupon Page Could not be Deleted. " + ex.Message, isSuccessfull = false }

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
            // string imageName = string.Empty;
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            // tblquecmsimage imageData = new tblquecmsimage();
            CouponPages coupon = new CouponPages();
            Helper helper = new Helper(_configuration);
            // int imageId = Convert.ToInt32(TempData["imageId"]);
            int qImageId = Convert.ToInt32(TempData["qImageId"]);
            int qScriptId = Convert.ToInt32(TempData["qScriptId"]);
            Guid guid = new Guid(TempData["guid"].ToString() ?? "");
            // Guid guid = new Guid(TempData["guid"]);
            int qScriptIdEng = Convert.ToInt32(TempData["qScriptIdEng"]);
            int qScriptIdFr = Convert.ToInt32(TempData["qScriptIdFr"]);

            string formMode = (string)TempData["formMode"] ?? "";
            var data = new { message = "", isSuccessfull = false };

            try
            {
                //  imageData = _appRepo.GetImageDetailByID(imageId, qImageId);
                // imageName = imageData.img_name.ToString();
                foreach (var item in _couponsRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                {
                    if (item.language_id == 1)
                    {
                        isUpdated = _appRepo.UpdateScriptQueueDetailByID(item.guid, qScriptIdEng);
                        isUpdated = _appRepo.UpdateImageDetailBycmsqueId(item.guid, qScriptIdEng);
                    }
                    else if (item.language_id == 2)
                    {
                        isUpdated = _appRepo.UpdateScriptQueueDetailByID(item.guid, qScriptIdFr);
                        isUpdated = _appRepo.UpdateImageDetailBycmsqueId(item.guid, qScriptIdFr);
                    }
                    if (isUpdated == true)
                    {
                        coupon = _couponsRepo.FindById(item.coupon_page_id);

                        if (formMode == "Create")
                        {
                            prodUpdated = _couponsRepo.InsertCouponProd(coupon);
                        }
                        else if (formMode == "Edit")
                        {
                            prodUpdated = _couponsRepo.UpdateCouponProd(item.language_id, item.guid, coupon);
                        }
                    }
                }
                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Coupon Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing Coupon Page - {imageId} ", isSuccessfull = false }
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
                    data = new { message = "Coupon Page failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                 " | Error Message: " + jsonData.data.message + $"Coupon Page Id: {imageId} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }

    }
}
