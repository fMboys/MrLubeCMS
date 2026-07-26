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
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Claims;

namespace MrLubeCMS.Controllers
{
    // [Authorize(Roles ="admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CMSDbContext _context;
        private readonly IbannerService _repo;
        public readonly IConfiguration _configuration;
        //public readonly bannerService _bannerService;
        tblquecmsimageModel imgid;
        public HomeController(ILogger<HomeController> logger, CMSDbContext context, IbannerService repo, IConfiguration configuration/*, bannerService bannerService*/)
        {
            _logger = logger;
            _context = context;
            _repo = repo;
            _configuration = configuration;
            //_bannerService = bannerService;
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult Banner()

        {
            var ban= _repo.GetAllBanner();
            ViewBag.bann = ban;
            return View();
        }

        private string UploadedFile(IFormFile banner)
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
                request.Method = WebRequestMethods.Ftp.UploadFile;

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
                return banner.FileName;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Banner(IFormCollection model)
        {
            var data = new { message = "", isSuccessfull = false };
            string formMode = "Create";
            var uniqueFileName =""; 

            if (ModelState.IsValid)
            {
                var img = Request.Form.Files.GetFile("imgfile");

                if (img != null)
                {
                    BlobURIDto blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("DevContainerRoot").Value,
                        FormFile = img,
                        FileName = img.FileName.ToString()
                    };
                    Helper helper = new Helper(_configuration);
                    string endPoint = helper.GenerateBlobStorageUri("Upload", blobURIDto);
                    BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                    uniqueFileName = string.IsNullOrEmpty(img?.FileName.ToString()) ? "" : img.FileName.ToString();
                }

                //var uniqueFileName = UploadedFile(img);
                banners banner1 = new banners();
                if (model != null)
                {
                    banner1.title = model["BannerTitle"];
                    banner1.ad_hyperlink = model["Hyperlink"];
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
                    if (HttpContext.User.Identity != null)
                    {
                        banner1.last_user = HttpContext.User.Identity.Name;
                    }

                    banner1.date_created = DateTime.Now;
                    banner1.date_updated = DateTime.Now;

                    _context.Add(banner1);
                    _context.SaveChanges();

                    //_repo.SaveQueData(banner1, ref imgid,formMode);

                }
                //dynamic banserv = _repo.GetAllBanner();
                ViewBag.image = img;

                var jsonData = new
                {
                    bannerimg = ViewBag.image,
                    data = new { message = "Banner Successfully Uploaded.", Isupdated = true }
                    //recordsTotal = totalrows,
                    //recordsFiltered = totalrowsafterfiltering
                };
                return Json(jsonData);

                //return View("Banner");
            }
            return View("Banner");

        }

        //Edit banner
        public ActionResult Edit(int id)
        {
            try
            {
                var Banner = new banners();
                
                return View(Banner);
            }
            catch (Exception ex)
            {
                Log.Information("StoreController");
                Log.Error(ex.ToString());
                return View("Error");
            }
        }

        public IActionResult BannerPartial()
        {
            return PartialView("_partialBanner");
        }

        //Banner List for Detail

        [HttpPost]
        public async Task<ActionResult> BannerList(string title,string image, string view, string Status,  string ad_hyperlink)
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
                string searchValue = Request.Form["search[value]"];
                string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"];
                string sortDirection = Request.Form["order[0][dir]"];
                int totalrows = 0;
                int totalrowsafterfiltering = 0;
                List<bannerModel> BannerList = new List<bannerModel>();
                
                GetData(title, image, view, Status, ad_hyperlink, ref BannerList);
                //BannerList = BannerList.ToList();
                totalrows = BannerList.Count;
                if (!string.IsNullOrEmpty(searchValue))
                {
                    BannerList = (List<bannerModel>)BannerList.Where(a=>a.title.Contains(searchValue.ToLower()) ||
                    a.image.Contains(searchValue.ToLower()) || a.view.Contains(searchValue.ToLower()) || 
                    a.status.Contains(searchValue.ToLower()) || a.ad_hyperlink.Contains(searchValue.ToLower())).ToList();
                }

                totalrowsafterfiltering = BannerList.Count;

                //sorting
                BannerList = BannerList.AsQueryable().OrderBy(sortColumnName + " " + sortDirection).ToList();

                //paging
                BannerList = BannerList.Skip(start).Take(length).ToList();
                
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
                    draw = Request.Form["draw"],
                    recordsTotal = totalrows,
                    recordsFiltered = totalrowsafterfiltering
                };
                return Json(jsonData);

            }
            catch (Exception ex)
            {
                Log.Information("BannerController");
                Log.Error(ex.ToString());
                return View("Error");
            }

        }

        private void GetData(string title,string image, string view, string Status,string ad_hyperlink, ref List<bannerModel> BannerList)
        {
            try
            {
                var bannerMode = new banners();
                
                bannerMode.title = title;
                bannerMode.image = image;
                bannerMode.view = view;
                bannerMode.status = Status;
                bannerMode.ad_hyperlink = ad_hyperlink;
                //bannerMode.date_created = date_created;
                var data = _repo.GetAllBannerList(bannerMode);
                BannerList = data;
                
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public IActionResult Publish(IFormFile imgban)
        {
            //var img = Request.Form.Files.GetFile("imgfile").FileName;
            var modelBanner = new bannerModel();
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                Guid img = ((Guid)TempData["mainimg"]);
                _repo.Getimage(img,ref modelBanner);
                con.Open();
                MySqlCommand cmd = new MySqlCommand("insert into banner ( where image = '" + imgban + "'", con);
                try
                {
                    cmd.ExecuteNonQuery();

                    return View();
                }
                catch (Exception)
                {
                    return View("Error");
                }
                
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