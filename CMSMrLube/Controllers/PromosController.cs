using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using CommandLine;
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
using System.Security.Policy;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class PromosController : Controller
    {
        private readonly ILogger<PromosController> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly IPromosRepository _promosRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly IConfiguration _configuration;
        public tblquecmsimageModel imageId;//todo ??

        public PromosController(ILogger<PromosController> logger, CMSDbContext dbContext,
            IPromosRepository promosRepo, IApplicationRepository appRepo, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _appRepo = appRepo;
            _promosRepo = promosRepo;
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

        public async Task<ActionResult> PromosList()
        {
            try
            {
                List<PromoPages> promosList = new List<PromoPages>();
                PromoPages objPromo = new PromoPages();
                int i = 0;
                foreach (var item in _promosRepo.GetPromoPagesList().OrderBy(a => a.itemId))
                {
                    i = i + 1;
                    if (i % 2 != 0)
                    {
                        objPromo = new PromoPages();
                        objPromo = item;
                    }
                    else
                    {
                        objPromo.frenchTitle = item.title;
                        promosList.Add(objPromo);
                    }
                }

                var jsonData = new
                {
                    data = promosList
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                var data = new { message = "Promo Page List Error: There was an Error on Listing Promo Page." };
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
                PromoPages pg = new PromoPages();
                return View(pg);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Promo Page Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePromo(IFormCollection form)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            bool isAdded = false;
            bool isEmpty = false;
            PromoPages promo = new PromoPages();
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
                        promo.guid = Guid.NewGuid();
                        var alreadyExists = _promosRepo.TitleExists(Guid.Empty, form["englishTitle"], form["frenchTitle"]);
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
                            int itemId = _promosRepo.MaxItemId();
                            itemId = itemId + 1;
                            for (int i = 0; i < 2; i++)
                            {
                                promo.promo_page_id = 0;
                                if (i == 0)
                                {
                                    promo.title = form["englishTitle"];
                                    promo.language_id = 1;
                                }
                                else if (i == 1)
                                {
                                    promo.title = form["frenchTitle"];
                                    promo.language_id = 2;
                                }
                                promo.itemId = itemId;
                                promo.date_expired = Convert.ToDateTime(form["date_expired"]);

                                promo.url_Key = Convert.ToString(form["englishTitle"]).Replace(" ", "-").ToLower();
                                promo.url_Key = promo.url_Key.Replace("'", "");
                                promo.status = form["imageStatus"];
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                    promo.last_user = HttpContext.User.Identity.Name;
                                }
                                promo.last_user = promo.last_user ?? "";

                                promo.date_created = DateTime.Now;
                                promo.date_updated = DateTime.Now;
                                isAdded = _promosRepo.Add(promo);
                                if (isAdded)
                                    isAdded = _promosRepo.SaveScriptAndData(formMode, promo, ref queuesDto, user);
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
                    TempData["imageId"] = promo.promo_page_id;
                    TempData["qImageId"] = queuesDto.ImageQueueId;
                    TempData["qScriptId"] = queuesDto.ScriptQueueId;
                    TempData["guid"] = promo.guid;
                    TempData["formMode"] = "Create";

                    var jsonData = new
                    {
                        data = new { message = "Promo Saved successfully.", isSuccessfull = true, formMode = "Create", isEmpty }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "There was an error on uploading the Promo Page. Id = " + promo.guid, isSuccessfull = false, formMode = "Create" }
                    };
                    _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"Promo Page Id: {promo.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Promo Not Saved." + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                   ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + jsonData.data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the Page.");
                return Json(jsonData);
            }
        }

        public IActionResult Edit(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                PromoPages pg = new PromoPages();
                foreach (var item in _promosRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
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

                // pg = _promosRepo.FindByItemId(id);
                return View(pg);

            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Page Page Specs Issue Id: {guid}", isSuccessfull = false };
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
            PromoPages promo = new PromoPages();
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
                        promo.guid = new Guid(formValue["guid"]);
                        var alreadyExists = _promosRepo.TitleExists(promo.guid, formValue["englishTitle"], formValue["frenchTitle"]);


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
                            foreach (var item in _promosRepo.FindByGuidID(promo.guid).OrderBy(a => a.language_id))
                            {
                                promo.promo_page_id = item.promo_page_id;
                                promo.itemId = item.itemId;
                                promo.language_id = item.language_id;
                                promo.url_Key = item.url_Key;
                                if (item.language_id == 1)
                                {
                                    promo.title = formValue["englishTitle"];
                                }
                                else if (item.language_id == 2)
                                {
                                    promo.title = formValue["frenchTitle"];
                                }

                                promo.date_expired = Convert.ToDateTime(formValue["date_expired"]);
                                promo.status = formValue["imageStatus"];

                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                    promo.last_user = HttpContext.User.Identity.Name;
                                }
                                promo.last_user = promo.last_user ?? "";
                                promo.date_updated = DateTime.Now;
                                isUpdated = _promosRepo.Update(promo, string.Empty);
                                if (isUpdated)
                                    isUpdated = _promosRepo.SaveScriptAndData(formMode, promo, ref queuesDto, user);
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
                    TempData["guid"] = promo.guid;
                    var jsonData = new
                    {
                        data = new { message = "Promo updated successfully.", isUpdate = true, formMode = "Edit", isEmpty }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {

                        data = new { message = $"Promo Page Updating Error: There was an Error on Updating Promo Page: {promo.guid}", isUpdate = false, formMode = "Edit" }
                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Promo Id: {promo.guid}");
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Promo Page Could not be Uploaded." + ex.Message, isUpdate = false, formMode = "Edit" }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"Promo Id: {promo.guid}");
                return Json(jsonData);
            }
        }

        public IActionResult Details(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                PromoPages promo = new PromoPages();

                foreach (var item in _promosRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                {
                    if (item.language_id == 1)
                    {
                        promo = item;
                    }
                    else if (item.language_id == 2)
                    {
                        promo.frenchTitle = item.title;
                    }
                }
                // Replace with automapper
                PromoPagesDto promoDto = new PromoPagesDto()
                {
                    promo_page_id = promo.promo_page_id,
                    LanguageId = promo.language_id,
                    Title = promo.title,
                    frenchTitle = promo.frenchTitle,
                    ImageStatus = promo.status,
                    LastUser = promo.last_user,
                    CreatedDate = promo.date_created,
                    UpdatedDate = promo.date_updated,
                    date_expired = promo.date_expired,
                    status = promo.status,
                    ItemId = promo.itemId

                };

                return View(promoDto);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Promo Page Detail Issue Id: {guid}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid guid)
        {
            bool isDeleted = false;
            bool isAdded = false;
            string formMode = "Delete";
            string bannerType = "promoPage";
            PromoPages promosData = new PromoPages();
            Helper helper = new Helper(_configuration);
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            PromoPages promosPages = new PromoPages();
            try
            {
                promosPages = _promosRepo.FindByLangGuid(guid);
                List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();
                var checkdelFile = _appRepo.isFilependingbanner(guid, bannerType, ref tblquecmsimage);
                bool checkprodFile = _promosRepo.CheckFileOnProd(promosPages);

                if (tblquecmsimage.Count > 0)
                {
                    foreach (var item in _promosRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                    {
                        promosData = _promosRepo.FindById(item.promo_page_id);
                        isDeleted = _promosRepo.Update(item, formMode);

                        if (isDeleted)
                        {
                            if (checkprodFile == true && isDeleted == true)
                            {
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                isDeleted = _promosRepo.SaveScriptAndData(formMode, promosData, ref queuesDto, user);
                            }
                        }
                    }

                    if (isDeleted == true)
                    {
                        var jsonData = new
                        {
                            data = new { message = "Promo-" + promosData.url_Key + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Promo Image Id: " + guid, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Promo Id: {guid}");
                        return Json(jsonData);
                    }
                }

                else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
                {
                    foreach (var item in _promosRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                    {
                        promosData = _promosRepo.FindById(item.promo_page_id);
                        isDeleted = _promosRepo.Update(item, formMode);

                        if (isDeleted)
                        {
                            if (checkprodFile == true && isDeleted == true)
                            {
                                string user = "";
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                isDeleted = _promosRepo.SaveScriptAndData(formMode, promosData, ref queuesDto, user);
                            }
                        }
                    }

                    if (isDeleted == true)
                    {
                        var jsonData = new
                        {
                            data = new { message = "Promo-" + promosData.url_Key + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Promo Image Id: " + guid, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Promo Id: {guid}");
                        return Json(jsonData);
                    }
                }

                else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
                {
                    foreach (var item in _promosRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
                    {
                        promosData = _promosRepo.FindById(item.promo_page_id);
                        isDeleted = _promosRepo.Update(item, formMode);

                    }

                    if (isDeleted == true)
                    {
                        var jsonData = new
                        {
                            data = new { message = "Promo-" + promosData.url_Key + " is Deleted.", isSuccessfull = true, formMode = "Delete" }
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Promo Image Id: " + guid, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                               " | Error Message: " + jsonData.data.message + $"Promo Id: {guid}");
                        return Json(jsonData);
                    }
                }



                    return View();
            }
            catch (Exception ex)
            {
                Log.Information("PromosController");
                Log.Error(ex.ToString());
                var jsonData = new
                {
                    data = new { message = "Error: Promo Could not be Deleted. " + ex.Message, isSuccessfull = false }

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
            PromoPages promo = new PromoPages();
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
                foreach (var item in _promosRepo.FindByGuidID(guid).OrderBy(a => a.language_id))
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
                        promo = _promosRepo.FindById(item.promo_page_id);

                        if (formMode == "Create")
                        {
                            prodUpdated = _promosRepo.InsertPromoProd(promo);
                        }
                        else if (formMode == "Edit")
                        {
                            prodUpdated = _promosRepo.UpdatePromoProd(item.language_id, item.guid, promo);
                        }
                    }
                }
                if (prodUpdated == true)
                {
                    var jsonData = new
                    {
                        data = new { message = "Promo Successfully Published on Production.", isSuccessfull = true }
                    };
                    return Json(jsonData);
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = $"There was an Error in Publishing Promo Page - {imageId} ", isSuccessfull = false }
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
                    data = new { message = "Promo Page failed to publish: Error. " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                 " | Error Message: " + jsonData.data.message + $"Promo Page Id: {imageId} | StackTrace: {ex.StackTrace}");
                return Json(jsonData);
            }
        }

    }
}
