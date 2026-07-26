using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MrLubeCMS.CustomHandler;
using MrLubeCMS.ViewModels;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;

namespace MrLubeCMS.Controllers
{
    // [Authorize(Roles ="admin")]
    [Authorize]
    public class BannerController : Controller
    {
        private readonly ILogger<BannerController> _logger;
        private readonly CMSDbContext _context;
        private readonly IbannerService _repo;
        private readonly IApplicationRepository _appRepo;
        public readonly IConfiguration _configuration;
        bannerData imgdata;
        public tblquecmsimage tblimgqry;
        public tblquecms tblquery;
        //IFormFile mainimg;

        public BannerController(ILogger<BannerController> logger, CMSDbContext context,
            IbannerService repo, IConfiguration configuration, IApplicationRepository appRepo)
        {
            _logger = logger;
            _context = context;
            _repo = repo;
            _configuration = configuration;
            _appRepo = appRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                banners model = new banners();
                //var BannnerCount = _repo.GetBannerCount();
                //var max = BannnerCount;
                //ViewBag.BannerCount = max;
                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "HomeBanner" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "HomeBanner" && a.view_device == "mobile").FirstOrDefault();
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
                data = new { message = $"Error: Home Page Banner Image Specs Issue: {ex.Message}", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return Problem("Error:MrLubeCMS");
            }
            
        }

        public IActionResult Banner()

        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                var ban = _repo.GetAllBanner();
                ViewBag.bann = ban;
                dynamic getall = _repo.GetAll();
                ViewBag.bannerIds = getall;
                return View();
            }
            catch (Exception ex)
            {
                data = new { message = "Error: Home Page Banner Data List Issue", isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserData: " + data.message + " | StackTrace: " + ex.StackTrace);
                _logger.LogWarning("Error in MySQL Connection"); 
                throw ;
            }
            
        }

        public bool UploadedFile(IFormFile banner)
        {
            try
            {
                bannerData img = new bannerData();
                /*var uniqueFileName = banner["file"]*/
                byte[] filebytes = null;
                string FileDomain = _configuration.GetSection("FTP_Server").Value;
                string FilePath = _configuration.GetSection("BannersPath").Value;
                string FtpUser = _configuration.GetSection("FTP_Username").Value;
                string FtpPass = _configuration.GetSection("FTP_Password").Value;
                string filefullPath = FileDomain + FilePath;
                string fullimgpath = filefullPath + "/" + banner.FileName;
                string filename = Path.GetFileName(banner.FileName);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filefullPath + "/" + banner.FileName);

                // This assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(FtpUser, FtpPass);
                //request.ContentLength = filebytes.Length;

                request.UseBinary = true;
                request.UsePassive = true;
                //request.ServicePoint.ConnectionLimit = filebytes.Length;
                request.EnableSsl = true;
                request.Method = WebRequestMethods.Ftp.UploadFile.ToLower();

                using (Stream sr = request.GetRequestStream())
                {
                    banner.CopyTo(sr);

                    sr.Close();
                }

                //if (banner["file"] != "" )
                //{
                //    //Ftp Upload Image
                //    //_repo.FtpUploadImage(uniqueFileName);

                //    return img.BannerImage.FileName;
                //}
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Banner(IFormCollection model)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            bool IsAdded = false;
            bool isEmpty = false;
            //Helper helper = new Helper(_configuration);
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            try
            {
                if (ModelState.IsValid)
                {
                    banners banner1 = new banners();
                    var guid = Guid.NewGuid();
                    banner1.guid = guid;
                    var uniqueFileName = "";
                    Helper timeHelp = new Helper(_configuration);
                    var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
                    var imageName = timestamp + "_";
                    var img = Request.Form?.Files?.GetFile("imgfile");
                    
                    //mainimg = img;
                    bool uploadedimg = false;

                    isEmpty = _appRepo.CheckImageQueue();

                    if (img != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("BannersPath").Value,
                            FormFile = img,
                            FileName = imageName + img.FileName.ToString()
                        };

                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            uploadedimg = true;

                        //azureFTPDto = helper.GenerateAzureUri(img.FileName.ToString(), "HomeBanner", "Stage");
                        //uploadedimg = AzureService.FTPUploader(img, azureFTPDto);
                        uniqueFileName = string.IsNullOrEmpty(imageName + img?.FileName.ToString()) ? "" : imageName + img.FileName.ToString();
                        //uploadedimg = UploadedFile(img);

                    }

                    //imgdata.BannerImage = img;
                    uniqueFileName = imageName + img?.FileName.ToString();

                    if (uploadedimg == true && model != null)
                    {
                        banner1.title = model["BannerTitle"];
                        banner1.ad_hyperlink = Convert.ToString(model["Hyperlink"]).Replace("'","");
                        banner1.image = uniqueFileName;
                        banner1.page = "Home";
                        if (model["language"].ToString() == "English")
                        {
                            banner1.language_id = 1;
                        }
                        else { banner1.language_id = 2; }
                        banner1.status = model["Status"];
                        banner1.view = model["viewSelect"];
                        //banner1.last_user = "admin";
                        banner1.last_user = "";
                        if (HttpContext.User.Identity != null)
                        { 
                            banner1.last_user = HttpContext.User.Identity.Name??""; 
                        }

                        banner1.date_created = DateTime.Now;
                        banner1.date_updated = DateTime.Now;

                        IsAdded = _repo.Add(banner1);

                        if (IsAdded)
                            IsAdded = _repo.SaveQueData(banner1, ref tblimgqry, ref tblquery, formMode, banner1.last_user);

                        //var dataimage = tblquery;
                        //int imId = dataimage;

                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an error on uploading the image. Id = " + banner1.guid, isSuccessfull = false }
                        };
                        
                        _logger.LogError($"Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {banner1.guid}");

                        return Json(jsonData);
                    }
                    //dynamic banserv = _repo.GetAllBanner();
                    //ViewBag.image = mainimg;
                    //imgdata.BannerImage = img;
                    if (IsAdded == true)
                    {
                        TempData["imgid"] = banner1.guid;
                        TempData["tblquery"] = tblquery.que_id;
                        TempData["tblimgqry"] = tblimgqry.img_queId;
                        TempData["formMode"] = formMode;

                        var jsonData = new
                        {
                            //bannerimgdata = TempData["mainimg"],
                            //bannerimg = ViewBag.image,
                            data = new { message = "Banner Successfully Uploaded.", isSuccessfull = true, formMode = "Create", isEmpty }
                            //recordsTotal = totalrows,
                            //recordsFiltered = totalrowsafterfiltering
                        };
                        return Json(jsonData);
                    }
                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an error on uploading the image. Id = " + banner1.guid, isSuccessfull = false }
                    };

                        
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {banner1.guid}");

                        return Json(jsonData);
                    }


                    //return View("Banner");
                }
            }
            catch (Exception ex)
            {
                
                var jsonData = new
                {
                    data = new { message = "There was an error on uploading the image. Error = "  + ex.Message, isSuccessfull = false }

                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " +
                    ControllerContext.ActionDescriptor.ActionName + " | Error Message: " + jsonData.data.message + $" | StackTrace: {ex.StackTrace}");
                _logger.LogWarning("There was an error on uploading the image.");

                return Json(jsonData);
            }

            return View("Banner");

        }

        public ActionResult Edit(Guid guid)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                var model = _repo.FindbyId(guid);
                var BannerSize = _appRepo.GetAllImagesSpecifications();

                var dBannerSize = BannerSize.Where(a => a.banner_type == "HomeBanner" && a.view_device == "desktop").FirstOrDefault();
                var mBannerSize = BannerSize.Where(a => a.banner_type == "HomeBanner" && a.view_device == "mobile").FirstOrDefault();
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

        public ActionResult Delete(Guid id)
        {
            var data = new { message = "", isSuccessfull = false };
            Helper helper = new Helper(_configuration);
            AzureFTPDto azureFTPDto = new AzureFTPDto();
            bool isUpdate = false;
            string bannerType = "Homebanner";
            string formMode = "Delete";
            string user = "";
            try
            {
                var model = _repo.FindbyId(id);
                List<tblquecmsimage> tblquecmsimage = new List<tblquecmsimage>();

                if (!string.IsNullOrEmpty(model.image) && model != null)
                {
                    var checkdelFile = _appRepo.isFilependingbanner(id, bannerType, ref tblquecmsimage);
                    bool checkprodFile = _repo.CheckFileOnProd(model);
                    if (tblquecmsimage.Count > 0)
                    {
                        foreach (var item in tblquecmsimage)
                        {
                            _appRepo.RemoveImgQueData(item.img_queId);
                        }

                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("BannersPath").Value,
                            FileName = model.image.ToString()
                        };
                        string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                        string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                        if (!string.IsNullOrEmpty(result))
                            isUpdate = true;

                        //azureFTPDto = helper.GenerateAzureUri(model.image.ToString(), "HomeBanner", "Stage");
                        string Filedelimg = model.image;
                        //isUpdate = AzureService.FTPDeleteFile(Filedelimg, azureFTPDto);

                        if (isUpdate)
                        {
                            bool IsDeleted = false;
                            IsDeleted = Convert.ToBoolean(_repo.RemovebyId(id));
                            if (checkprodFile == true && IsDeleted == true)
                            {
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                isUpdate = _repo.SaveQueData(model, ref tblimgqry, ref tblquery, formMode, user);
                            }
                            //else
                            //{
                            //    var jsonData = new
                            //    {
                            //        data = new { message = "Error: Banner Could not be Deleted Properly.", isSuccessfull = false }

                            //    };
                            //    return Json(jsonData);
                            //}
                            //isUpdate = _appRepo.SaveQueDataWithnoImage(model, ref tblimgqry, ref tblquery, "NoImage");

                        }
                        if (isUpdate)
                        {
                            var jsonData = new
                            {
                                data = new { message = "Home Page Banner - " + model.guid + " Successfully Deleted.", isSuccessfull = true, formMode = "Delete" }
                            };
                            return Json(jsonData);
                        }
                        else
                        {
                            
                            var jsonData = new
                            {
                                data = new { message = "There was an Error on Deleting Banner Id: " + id, isSuccessfull = false }

                            };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {id}");
                            return Json(jsonData);
                        }
                    }
                    else if (tblquecmsimage.Count <= 0 && checkprodFile == true)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("BannersPath").Value,
                            FileName = model.image.ToString()
                        };
                        string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                        string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                        if (!string.IsNullOrEmpty(result))
                            isUpdate = true;

                        //azureFTPDto = helper.GenerateAzureUri(model.image.ToString(), "HomeBanner", "Stage");
                        //string Filedel = model.image;
                        //isUpdate = AzureService.FTPDeleteFile(Filedel, azureFTPDto);

                        if (isUpdate)
                        {
                            bool IsDeleted = false;
                            IsDeleted = Convert.ToBoolean(_repo.RemovebyId(id));
                            if (checkprodFile == true && IsDeleted == true)
                            {
                                if (HttpContext.User.Identity != null)
                                {
                                    user = HttpContext.User.Identity.Name ?? "";
                                }
                                isUpdate = _repo.SaveQueData(model, ref tblimgqry, ref tblquery, formMode, user);
                            }
                            else
                            {
                                var jsonData = new
                                {
                                    data = new { message = "Error: Home Page Banner Could not be Deleted Properly. Data Missing Or Not found.", isSuccessfull = false }

                                };
                                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                            " | Error Message: " + jsonData.data.message + $"banner Id: {id}");
                                return Json(jsonData);
                            }

                        }
                        if (isUpdate)
                        {
                            var jsonData = new
                            {
                                data = new { message = "Home Page Banner - " + model.guid + " Successfully Deleted.", isSuccessfull = true, formMode = "Delete" }
                            };
                            return Json(jsonData);
                        }
                        else
                        {
                            var jsonData = new
                            {
                                data = new { message = "There was an Error on Deleting Banner Id: " + id, isSuccessfull = false }

                            };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"banner Id: {id}");
                            return Json(jsonData);
                        }
                    }

                    else if (tblquecmsimage.Count <= 0 && checkprodFile == false)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("BannersPath").Value,
                            FileName = model.image.ToString()
                        };
                        string endPoint = helper.GenerateBlobStorageUri("Delete", blobURIDto);
                        string result = BlobStorageAPIService.BlobDeleteFile(endPoint).Result;

                        if (!string.IsNullOrEmpty(result))
                            isUpdate = true;

                        //azureFTPDto = helper.GenerateAzureUri(model.image.ToString(), "HomeBanner", "Stage");
                        //string Filedel = model.image;
                        //isUpdate = AzureService.FTPDeleteFile(Filedel, azureFTPDto);

                        if (isUpdate)
                        {
                            bool IsDeleted = false;
                            IsDeleted = Convert.ToBoolean(_repo.RemovebyId(id));
                        }

                        if (isUpdate)
                        {
                            var jsonData = new
                            {
                                data = new { message = "Home Page Banner - " + model.guid + " Successfully Deleted.", isSuccessfull = true, formMode = "Delete" }
                            };
                            return Json(jsonData);
                        }
                        else
                        {
                            var jsonData = new
                            {
                                data = new { message = "There was an Error on Deleting Banner Id: " + id, isSuccessfull = false }

                            };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"banner Id: {id}");
                            return Json(jsonData);
                        }

                    }

                    else
                    {
                        var jsonData = new
                        {
                            data = new { message = "There was an Error on Deleting Banner Id: " + id, isSuccessfull = false }

                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"banner Id: {id}");
                        return Json(jsonData);
                    }
                }
                else
                {
                    var jsonData = new
                    {
                        data = new { message = "There was an Error on Deleting Banner Id: " + id, isSuccessfull = false }

                    };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                           " | Error Message: " + jsonData.data.message + $"banner Id: {id}");
                    return Json(jsonData);
                }


            }

            catch (Exception ex)
            {
                var jsonData = new
                {
                    data = new { message = "Error: Home Page Banner Could not be Deleted Properly. Data Missing Or Not found." + ex.Message, isSuccessfull = false }

                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IFormCollection updmodel)
        {
            banners model = new banners();
            tblquery = new tblquecms();
            tblimgqry = new tblquecmsimage();
            //bool IsAdded = false;
            bool isEmpty = false;
            //Helper helper = new Helper(_configuration);
            //AzureFTPDto azureFTPDto = new AzureFTPDto();
            bool isUpdate = false;
            string formMode = "Edit";
            Helper timeHelp = new Helper(_configuration);
            var timestamp = timeHelp.GetTimestamp(DateTime.Now).ToString();
            var imageName = timestamp + "_";
            try
            {
                if (ModelState.IsValid)
                {
                    var img = Request.Form?.Files?.GetFile("imgfile");
                    Guid guid = new Guid(updmodel["img_guid"]);
                    isEmpty = _appRepo.CheckImageQueue();
                    model.guid = guid;
                    model.banner_id = Convert.ToInt32(updmodel["bannerId"]);
                    if (updmodel["language"] == "English")
                    {
                        model.language_id = 1;
                    }
                    else
                    {
                        model.language_id = 2;
                    }
                    if (img != null) { model.image = imageName+img.FileName; }
                    else { model.image = updmodel["imgupd"]; }

                    model.title = updmodel["BannerTitle"];
                    model.ad_hyperlink = Convert.ToString(updmodel["Hyperlink"]).Replace("'","");
                    model.view = updmodel["viewSelect"];
                    model.status = updmodel["Status"];
                    bool uploadedimg = false;
                    if (img != null)
                    {
                        BlobURIDto blobURIDto = new BlobURIDto()
                        {
                            ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                            FolderPath = _configuration.GetSection("BannersPath").Value,
                            FormFile = img,
                            FileName = imageName + img.FileName.ToString()
                        };
                        Helper helper = new Helper(_configuration);
                        string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                        BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                        if (uploadResponseDto.status != null)
                            uploadedimg = true;

                        //azureFTPDto = helper.GenerateAzureUri(img.FileName.ToString(), "HomeBanner", "Stage");
                        //uploadedimg = AzureService.FTPUploader(img, azureFTPDto);
                        //uploadedimg= string.IsNullOrEmpty(img?.FileName.ToString()) ? "" : img.FileName.ToString();
                        //uploadedimg = UploadedFile(img);
                    }

                    //var bannerEdit = AutoMapper.Mapper.Map<bannerModel, banners>(model);
                    //tblimgqry = new tblquecmsimage();
                    isUpdate = _repo.Edit(model);
                    if (isUpdate == true)
                    {
                        string user = "";
                        if (HttpContext.User.Identity != null)
                        {
                            user = HttpContext.User.Identity.Name ?? "";
                        }
                        isUpdate = _repo.SaveQueData(model, ref tblimgqry, ref tblquery, formMode, user);

                        if (isUpdate == true)
                        {
                            //Save Query in query Table and image path.

                            TempData["imgid"] = model.guid;
                            TempData["tblquery"] = tblquery.que_id;
                            TempData["tblimgqry"] = tblimgqry.img_queId;
                            TempData["formMode"] = formMode;

                            var data = new { message = "Banner Updated Successfully.", formMode = "Edit", isUpdate = isUpdate, isSuccessfull = true, isEmpty };
                            return Json(data);
                        }
                        else
                        {
                            var data = new { message = $"Home Page Banner Updating Error: There was an Error on Updating Home Page Banner: {model.guid}", formMode = "Edit", isUpdate = isUpdate, isSuccessfull = false };
                            _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + data.message + $"banner Id: {model.guid}");
                            return Json(data);
                        }

                    }
                    else
                    {
                        var data = new { message = $"Home Page Banner Updating Error: There was an Error on Updating Home Page Banner: {model.guid}", formMode = "Edit", isUpdate = isUpdate, isSuccessfull = false };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                      " | Error Message: " + data.message + $"banner Id: {model.guid}");
                        return Json(data);
                    }
                }
                else
                {
                    var data = new { message = $"Home Page Banner Updating Error: There was an Error on Updating Home Page Banner: {model.guid}", formMode = "Edit", isUpdate = isUpdate, isSuccessfull = false };
                    _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                  " | Error Message: " + data.message + $"banner Id: {model.guid}");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                
                var data = new { message = "Home Page Banner Updating Error: Record Could be not Updated", formMode = "Edit", isUpdate = isUpdate, isSuccessfull = false };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return Json(data);
            }

            return View(model);
        }

        public IActionResult BannerPartial()
        {
            return PartialView("_partialBanner");
        }

        public ActionResult BannerList(string? title, string? view, /*string? lang,*/ string? Status, string? image,string? ad_hyperlink)
        {
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
                string searchValue = Request.Form["search[value]"].ToString().ToLowerInvariant();
                string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"];
                string sortDirection = Request.Form["order[0][dir]"];
                int totalrows = 0;
                int totalrowsafterfiltering = 0;
                List<bannerModel> BannerList = new List<bannerModel>();

                GetData(title, view, /*lang,*/ Status, image, ad_hyperlink, ref BannerList);
                //BannerList = BannerList.ToList();
                //totalrows = BannerList.Count;
                //BannerList.ForEach(a => a.ad_hyperlink?.Equals(string.IsNullOrEmpty(a.ad_hyperlink) ? a.ad_hyperlink == Convert.ToString(DBNull.Value) : a.ad_hyperlink));
                //BannerList.Select(s => s.ad_hyperlink ?? Convert.ToString(DBNull.Value)).ToList();
                //searchValue = searchValue.Replace(" ", "");
                //string lang = searchValue.ToString().Contains("english".ToLower()) ? "1" : searchValue.ToString().Contains("french".ToLower()) ? "2" : searchValue;
                
                //if (!string.IsNullOrEmpty(searchValue))
                //{
                    
                //    BannerList = BannerList.Where(a => a.title.Contains(searchValue.ToLower().ToString()) ||
                //    a.image.Contains(searchValue.ToLower()) || a.view.Contains(searchValue.ToLower()) ||
                //    a.status.Contains(searchValue.ToLower()) || a.language_id.ToString().Contains(searchValue.ToLower())).ToList();
                //}
                BannerList = BannerList.Where(x => x.status != "delete").ToList();
                //totalrowsafterfiltering = BannerList.Count;

                ////sorting
                //BannerList = BannerList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();

                ////paging
                //BannerList = BannerList.Skip(start).Take(length).ToList();

                //var Collection = BannerList.Select(x => new
                //{
                //    title = x.title,
                //    image = x.image,
                //    view = x.view,
                //    Status = x.status,
                //    ad_hyperlink = x.ad_hyperlink,


                //    Edit = Edit,
                //    View = View
                //}).ToList();

                //return ok(new
                //{
                //    data = Collection,
                //    draw = Request.Form["draw"],
                //    recordsTotal = totalrows,
                //    recordsFiltered = totalrowsafterfiltering
                //}, JsonRequestBehavior.AllowGet);

                var jsonData = new
                {
                    data = BannerList,
                    //draw = Request.Form["draw"],
                    //recordsTotal = totalrows,
                    //recordsFiltered = totalrowsafterfiltering
                };
                return Json(jsonData);

            }
            catch (Exception ex)
            {
                var data = new { message = "Home Page Banner List Error: There was an Error on Listing Home Page Banner."};
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + " | StackTrace: " + ex.StackTrace);
                return View("Error");
            }

        }

        private void GetData(string title, string view, /*string lang,*/ string Status, string image,string ad_hyperlink, ref List<bannerModel> BannerList)
        {
            try
            {
                var bannerMode = new banners();

                
                bannerMode.title = title;
                bannerMode.image = image;
                bannerMode.view = view;
                bannerMode.status = Status;
                //bannerMode.language_id = Convert.ToInt32(lang);
                //bannerMode.date_created = date_created;
                bannerMode.ad_hyperlink = ad_hyperlink;
                var data = _repo.GetAllBannerList(bannerMode);
                BannerList = data;

            }
            catch (Exception ex)
            {
                var data = new { message = "Home Page Banner List Error: There was an Error on Listing Home Page Banner." };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    $" | UserError: " + data.message + "| StackTrace: " + ex.StackTrace);
                throw ex.InnerException;
            }
        }

        public IActionResult Publish(IFormFile imgban)
        {
            bool imguploaded = false;
            //var img = Request.Form.Files.GetFile("imgfile").FileName;
            var modelBanner = new bannerModel();
            Guid img = new Guid(TempData["imgid"].ToString());
            //var img = new Guid("72bcf5cd-0784-4b4b-88d9-93f58da1a568");
            int tblque = Convert.ToInt32(TempData["tblquery"]);
            int imgque = Convert.ToInt32(TempData["tblimgqry"]);
            string formMode = (string)TempData["formMode"] ?? "";
            var data = new { message = "", isSuccessfull = false };
            //var img = imgdata.BannerImage;
            _repo.GetImgbyId(img, tblque, imgque, ref imguploaded);
            if (imguploaded == true)
            {
                _repo.Getimage(img, ref modelBanner);
            }
            else
            {
                
                var jsonData = new
                {
                    data = new { message = $"There was an Error in Publishing Home Page Banner - {img} ", isSuccessfull = false }
                };
                _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                          " | Error Message: " + jsonData.data.message + $"banner Id: {img}");
                return Json(jsonData);
            }
            
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                if (formMode == "Create")
                {
                    try
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("insert into banner (guid,language_id,title,image,page,view,last_user,status," +
                        "date_created,date_updated,ad_hyperlink) Values ('" + modelBanner.guid + "'," + modelBanner.language_id + ",'" +
                        modelBanner.title.Replace("'",@"\'") + "','" + modelBanner.image + "','" + modelBanner.page + "','" +
                        modelBanner.view + "','" + modelBanner.last_user + "','" + modelBanner.status + "','" +
                        DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + modelBanner.ad_hyperlink + "')", con);

                        cmd.ExecuteNonQuery();
                        con.Close();
                        var jsonData = new
                        {
                            data = new { message = "Banner Successfully Published on Production.", isSuccessfull = true }
                        };
                        return Json(jsonData);
                    }
                    catch (Exception ex)
                    {

                        var jsonData = new
                        {
                            data = new { message = $"There was an Error in Publishing Home Page Banner - {img} ", isSuccessfull = false }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                  " | Error Message: " + jsonData.data.message + $"banner Id: {img} | StackTrace: {ex.StackTrace}");
                        return Json(jsonData);
                    }
                }

                if (formMode == "Edit")
                {
                    try
                    {
                        con.Open();


                        string sql = "update banner set language_id = " + modelBanner.language_id +
                    ", title = '" + modelBanner.title.Replace("'",@"\'") + "', image = '" + modelBanner.image + "',page = '" + modelBanner.page + "', status = '" +
                    modelBanner.status + "', date_created = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                    "', ad_hyperlink = '" + modelBanner.ad_hyperlink + "' where guid = '" + modelBanner.guid + "'; ";

                        MySqlCommand cmd1 = new MySqlCommand(sql, con);

                        cmd1.ExecuteNonQuery();
                        con.Close();
                        var jsonData = new
                        {
                            data = new { message = "Banner Successfully Published on Production.", isSuccessfull = true }
                        };
                        return Json(jsonData);

                    }
                    catch (Exception ex)
                    {
                        var jsonData = new
                        {
                            data = new { message = $"There was an Error in Publishing Home Page Banner - {img} ", isSuccessfull = false }
                        };
                        _logger.LogError($"Error: Controller Name: " + ControllerContext.ActionDescriptor.ControllerName + " | Method Name: " + ControllerContext.ActionDescriptor.ActionName +
                                  " | Error Message: " + jsonData.data.message + $"banner Id: {img} | StackTrace: {ex.StackTrace}");
                        return Json(jsonData);
                    }
                }

                return Json(null);
            }
        }

        public ActionResult Details(Guid id)
        {
            var data = new { message = "", isSuccessfull = false };
            try
            {
                var model = _repo.FindbyId(id);

                return View(model);
            }
            catch (Exception ex)
            {
                data = new { message = $"Error: Home Page Banner Detail Issue Id: {id}", isSuccessfull = false };
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
                        if (spec.banner_type == "HomeBanner" && viewDevice.ToLower() == "desktop")
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
                        else if (spec.banner_type == "HomeBanner" && viewDevice.ToLower() == "mobile")
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
                        data = new { message = "Home Page Banner Image is valid.", isSuccessful = true }
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
                    data = new { message = "Home Page Banner Image Specs Error: " + ex.Message, isSuccessfull = false }
                };
                _logger.LogError("Error: Controller: " + ControllerContext.ActionDescriptor.ControllerName + " | Method: " + ControllerContext.ActionDescriptor.ActionName +
                    " | UserError: " + jsonData.data.message + " | StackTrace: " + ex.StackTrace);
                return Json(jsonData);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

         
    }
}
