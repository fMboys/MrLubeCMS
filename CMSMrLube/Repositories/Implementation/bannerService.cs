using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using LinqKit;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text;

namespace MrLubeCMS.Repositories.Implementation
{
    public class bannerService : IbannerService
    {
        private readonly CMSDbContext _context;
        public readonly IConfiguration _configuration;

        public bannerService(CMSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public void Add(banners banner)

        {
            banner.date_created = DateTime.Now;
            banner.date_updated = DateTime.Now;
            _context.banner.Add(banner);
            _context.SaveChanges();
        }

        public IEnumerable<banners> GetAllBanner()
        {
            return _context.banner.ToList().OrderBy(a => a.date_updated);
        }

        public List<bannerModel> GetAllBannerList(banners model)
        {
            var predicate = PredicateBuilder.True<banners>();
            if (!string.IsNullOrEmpty(model.status)) { predicate = predicate.And(i => i.status.Equals(model.status)); }
            if (!string.IsNullOrEmpty(model.view)) { predicate = predicate.And(i => i.view.Equals(model.view)); }
            if (!string.IsNullOrEmpty(model.ad_hyperlink)) { predicate = predicate.And(i => i.ad_hyperlink.Equals(model.ad_hyperlink)); }
            if (!string.IsNullOrEmpty(model.title)) { predicate = predicate.And(i => i.title.Equals(model.title)); }
            if (!string.IsNullOrEmpty(model.image)) { predicate = predicate.And(i => i.image.Equals(model.image)); }

            var str = (from v in _context.banner.Where(predicate)
                       select new bannerModel
                       {
                           banner_id = v.banner_id,
                           language_id = v.language_id,
                           title = v.title,
                           image = v.image,
                           page = v.page,
                           view = v.view,
                           last_user = v.last_user,
                           status = v.status,
                           ad_hyperlink = v.ad_hyperlink
                       }).ToList();
            //str.OrderByDescending(a=>a.date_updated == DateTime.Today).ToList();
            //totalrows = str.Count();
            //var rtdata = str.OrderBy(a => sortColumnName + " " + sortDirection).Skip(start).Take(length).ToList();
            return str;
        }

        public void SaveQueData(banners banner1, ref tblquecmsimageModel imgIddata)
        {
            try
            {
                var bannerQue = new tblquecms();
                var query = "insert into mrlubedb_test (banner_id,language_id,title,image,page,view,last_user,status," +
                    "date_created,date_updated,ad_hyperlink) Values (" + banner1.language_id + "," + banner1.title + "," +
                    banner1.image + "," + banner1.page + "," + banner1.view + "," + banner1.last_user + "," + banner1.status + "," +
                    DateTime.Now + "," + DateTime.Now + ");";
                bannerQue.img_id = banner1.banner_id;
                bannerQue.que_desc = "Uploaded Image: " + banner1.image + " with Status: " + banner1.status;
                bannerQue.que_script = query;
                bannerQue.que_date = DateTime.Now;
                bannerQue.que_user = (from v in _context.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                bannerQue.Status = "pending";
                bannerQue.created_date = DateTime.Now;
                bannerQue.updated_date = DateTime.Now;

                _context.Add(bannerQue);
                _context.SaveChanges();

                bool imgque = SaveImageQue(banner1, ref imgIddata);



            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

        }

        bool SaveImageQue(banners queimages, ref tblquecmsimageModel idImgdata)
        {
            try
            {
                var imgque = new tblquecmsimage();
                imgque.img_id = queimages.banner_id;
                imgque.img_description = queimages.image + "Status:" + queimages.status;
                imgque.img_name = queimages.image;
                imgque.img_uploadPath = _configuration.GetSection("BannersPath").Value;
                imgque.upload_date = DateTime.Now;
                imgque.img_user = queimages.last_user;
                imgque.Status = "pending";
                imgque.img_createdDate = DateTime.Now;
                imgque.img_updatedDate = DateTime.Now;

                _context.Add(imgque);
                _context.SaveChanges();

                //var imgsata = _context.Tblquecmsimage.Where(a => a.img_id == imgque.img_id && a.img_queId == imgque.img_queId).FirstOrDefault();
                //if (imgsata != null)
                //{
                //    tblquecmsimageModel imcms = new tblquecmsimageModel(imgsata);
                //    idImgdata = imcms;
                //}

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public void FtpUploadImage(IFormFile imgFile)
        {
            bool status = false;
            try
            {
                bannerData imgdata = new bannerData();

                //imgFile = "2020.jpg";
                byte[] filebytes = null;
                string FileDomain = _configuration.GetSection("FTP_Server").Value;
                string FilePath = _configuration.GetSection("BannersPath").Value;
                string FtpUser = _configuration.GetSection("FTP_Username").Value;
                string FtpPass = _configuration.GetSection("FTP_Password").Value;
                string filefullPath = FileDomain + FilePath;
                string fullimgpath = filefullPath + "/" + imgFile;
                string filename = Path.GetFileName(imgFile.FileName);
                using (StreamReader sr = new StreamReader(imgFile.OpenReadStream()))
                {
                    filebytes = Encoding.UTF8.GetBytes(sr.ReadToEnd());

                    sr.Close();
                }

                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filefullPath + "/" + imgFile);

                    // This assumes the FTP site uses anonymous logon.
                    request.Credentials = new NetworkCredential(FtpUser, FtpPass);
                    request.ContentLength = filebytes.Length;

                    request.UseBinary = true;
                    request.UsePassive = true;
                    //request.ServicePoint.ConnectionLimit = filebytes.Length;
                    request.EnableSsl = true;
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    using (FileStream fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (Stream requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(filebytes, 0, filebytes.Length);
                            requestStream.Close();
                        }
                    }


                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    string msg = imgFile + " uploaded.";
                    response.Close();
                }
                catch (WebException ex)
                {
                    throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
                }


            }
            catch (Exception ex)
            {
                var msg = ex.InnerException.Message.Select(p => p.ToString());
                status = false;
            }

            status = true;
        }

        public void Getimage(int img, ref bannerModel imgbanner)
        {
            //var banner = new bannerModel();
            if (img != null)
            {
                var imgfile = img;
                var strimg = _context.banner.Where(a => a.banner_id == imgfile).SingleOrDefault();
                imgbanner.banner_id = strimg.banner_id;
                imgbanner.language_id = strimg.language_id;
                imgbanner.title = strimg.title;
                imgbanner.image = strimg.image;
                imgbanner.page = strimg.page;
                imgbanner.view = strimg.view;
                imgbanner.last_user = strimg.last_user;
                imgbanner.status = strimg.status;
                imgbanner.date_created = strimg.date_created;
                imgbanner.date_updated = strimg.date_updated;
                imgbanner.ad_hyperlink = strimg.ad_hyperlink;
                //_context.Add(imgbanner);
                //_context.SaveChanges();
                //bannerData imgdata = new bannerData();

                //uploadImagetoFTPServer(banner.image);
            }


        }

        public void GetImgbyId(int imgId, ref bool uploadedimg)
        {
            bool Ftpimgdone = false;
            tblquecmsimage imgque = new tblquecmsimage();
            var imgsata = _context.Tblquecmsimage.Where(a => a.img_id == imgId).FirstOrDefault();
            imgque.img_queId = imgsata.img_queId;
            var quedata = FindQueImgByid(imgque.img_queId);
            string imageName = imgsata.img_name.ToString();

            if (imageName != null)
            {
                Ftpimgdone = uploadImagetoFTPServer(imageName);

            }
            var pendingimagequetbl = _context.Tblquecms.Where(a => a.img_id == imgId).FirstOrDefault();
            tblquecms imageModal = new tblquecms();
            imageModal.que_id = pendingimagequetbl.que_id;
            var trackque = FindquecmsById(imageModal.que_id);
            if (Ftpimgdone == true)
            {
                if (quedata != null && quedata.Status == "pending")
                {
                    quedata.Status = "completed";
                    quedata.img_updatedDate = DateTime.Now;
                    //_context.Add(imgsata);
                    _context.SaveChanges();
                }
                if (trackque != null && trackque.Status == "pending")
                {
                    trackque.Status = "completed";
                    trackque.updated_date = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            uploadedimg = true;
        }

        bool uploadImagetoFTPServer(string imgName)
        {
            try
            {
                //Staging FTP server
                string FileDomain = _configuration.GetSection("FTP_Server").Value;
                string FilePath = _configuration.GetSection("BannersPath").Value;
                string FtpUser = _configuration.GetSection("FTP_Username").Value;
                string FtpPass = _configuration.GetSection("FTP_Password").Value;
                string filefullPath = FileDomain + FilePath;
                string fullimgpath = filefullPath + "/" + imgName;


                FtpWebRequest downloadrequest = (FtpWebRequest)WebRequest.Create(filefullPath + "/" + imgName);
                downloadrequest.Method = WebRequestMethods.Ftp.DownloadFile;
                // This assumes the FTP site uses anonymous logon.
                downloadrequest.Credentials = new NetworkCredential(FtpUser, FtpPass);
                //request.ContentLength = filebytes.Length;
                downloadrequest.KeepAlive = true;
                //downloadrequest.ContentType = null;
                downloadrequest.UseBinary = true;
                downloadrequest.UsePassive = true;
                //downloadrequest.ContentType = "img/jpg,img/png";
                //request.ServicePoint.ConnectionLimit = filebytes.Length;
                downloadrequest.EnableSsl = true;

                FtpWebResponse response = (FtpWebResponse)downloadrequest.GetResponse();
                Stream responseStream = response.GetResponseStream();
                

                string ProdFileDomain = _configuration.GetSection("ProdFTP_Server").Value;
                string ProdFilePath = _configuration.GetSection("ProdBannersPath").Value;
                string ProdFtpUser = _configuration.GetSection("ProdFTP_Username").Value;
                string ProdFtpPass = _configuration.GetSection("ProdFTP_Password").Value;
                string ProdfilefullPath = ProdFileDomain + ProdFilePath;
                string Prodfullimgpath = ProdfilefullPath + "/" + imgName;

                //uploading image on Production Server

                Upload(Prodfullimgpath, ToByteArray(responseStream), ProdFtpUser, ProdFtpPass);
                responseStream.Close();

                //FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(ProdfilefullPath + "/" + imgName);
                //uploadRequest.Credentials = new NetworkCredential(FtpUser, FtpPass);
                //uploadRequest.UseBinary = true;
                //uploadRequest.UsePassive = true;
                //uploadRequest.KeepAlive = true;
                ////uploadRequest.ContentLength = 4096;
                //uploadRequest.EnableSsl = true;

                //uploadRequest.Method = WebRequestMethods.Ftp.UploadFile.ToLower();

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public static Byte[] ToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] chunk = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(chunk, 0, chunk.Length)) > 0)
            {
                ms.Write(chunk, 0, bytesRead);
            }

            return ms.ToArray();
        }

        public static bool Upload(string FileName, byte[] Image, string FtpUsername, string FtpPassword)
        {
            try
            {
                FtpWebRequest clsRequest = (FtpWebRequest)WebRequest.Create(FileName);
                clsRequest.Credentials = new NetworkCredential(FtpUsername, FtpPassword);
                clsRequest.Method = WebRequestMethods.Ftp.UploadFile;
                clsRequest.EnableSsl = true;
                clsRequest.UseBinary = true;
                clsRequest.UsePassive = true;
                clsRequest.KeepAlive = true;
                Stream clsStream = clsRequest.GetRequestStream();
                clsStream.Write(Image, 0, Image.Length);

                clsStream.Close();
                clsStream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        tblquecmsimage? FindQueImgByid(int id)
        {
            return _context.Tblquecmsimage.Find(id);
        }

        tblquecms? FindquecmsById(int id)
        {
            return _context.Tblquecms.Find(id);
        }

        public banners FindbyId(int id)
        {
            return _context.banner.Find(id);
        }

        public Boolean Edit(bannerModel model)
        {
            bool IsUpdate = false;
            var existsbanner = FindbyId(model.banner_id);
            try
            {
                if (existsbanner != null)
                {
                   //existsbanner.banner_id = model.banner_id;
                   existsbanner.language_id = model.language_id;
                   existsbanner.title = model.title;
                   existsbanner.image = model.image;
                   existsbanner.view = model.view;
                   existsbanner.status = model.status;
                   existsbanner.ad_hyperlink = model.ad_hyperlink;
                   IsUpdate= Convert.ToBoolean(_context.SaveChanges());
                   
                }
            }
            catch (Exception)
            {

                throw;
            }
            return IsUpdate;
            
        }
    }
}