using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Services;
using LinqKit;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace CMS.Infrastructure.Data
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

        public bool Add(banners banner)

        {
            try
            {
                //banner.date_created = DateTime.Now;
                //banner.date_updated = DateTime.Now;
                _context.banner.Add(banner);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }

        }

        //BannerId Max
        public int GetBannerCount()
        {
            int count = 0;
            var countbanner = _context.banner.Max(x => x.banner_id);
            return countbanner;
        }

        public IEnumerable<banners> GetAllBanner()
        {
            return _context.banner.ToList().OrderBy(a => a.date_updated);
        }

        public IEnumerable<banners> GetAll()
        {
            var ids = new List<banners>();
            var lstban = _context.banner.Where(x => x.status != "delete").ToList().OrderBy(x => x.banner_id);
            foreach (var banner in lstban)
            {
                ids.Add(banner);
            }
            return ids;
        }

        public List<bannerModel> GetAllBannerList(banners model)
        {
            var predicate = PredicateBuilder.True<banners>();
            //if (model.guid != Guid.Empty) { predicate = predicate.And(i => i.banner_id.Equals(model.guid)); }
            //if (model.language_id != 0) { predicate = predicate.And(i => i.language_id.Equals(model.language_id)); }
            if (!string.IsNullOrEmpty(model.status)) { predicate = predicate.And(i => i.status.Equals(model.status)); }
            if (!string.IsNullOrEmpty(model.view)) { predicate = predicate.And(i => i.view.Equals(model.view)); }
            if (!string.IsNullOrEmpty(model.ad_hyperlink)) { predicate = predicate.And(i => i.ad_hyperlink.Equals(model.ad_hyperlink)); }
            if (!string.IsNullOrEmpty(model.title)) { predicate = predicate.And(i => i.title.Equals(model.title)); }
            if (!string.IsNullOrEmpty(model.image)) { predicate = predicate.And(i => i.image.Equals(model.image)); }

            var str = (from v in _context.banner.Where(predicate)
                       where v.status != "delete" 
                       select new bannerModel 
                       {
                           banner_id=v.banner_id,
                           guid = v.guid,
                           language_id = v.language_id,
                           title = v.title.Trim().ToLower().ToString(),
                           image = v.image.Trim().ToLower().ToString(),
                           page = v.page,
                           view = v.view,
                           last_user = v.last_user,
                           status = v.status,
                           ad_hyperlink = v.ad_hyperlink
                       }).ToList();
            //str.OrderByDescending(a=>a.date_updated == DateTime.Today).ToList();
            //totalrows = str.Count();
            //var rtdata = str.OrderBy(a => sortColumnName + " " + sortDirection).Skip(start).Take(length).ToList();
            //str = (from v in _context.banner.Where(x => x.status != "delete") select new bannerModel
            //{
            //    banner_id = v.banner_id,
            //    language_id = v.language_id,
            //    title = v.title,
            //    image = v.image,
            //    page = v.page,
            //    view = v.view,
            //    last_user = v.last_user,
            //    status = v.status,
            //    ad_hyperlink = v.ad_hyperlink
            //}).ToList();
            return str;
        }

        public bool SaveQueData(banners banner1, ref tblquecmsimage imgIddata, ref tblquecms tblimgqry, string formMode, string user)
        {
            bool imgque = false;
            if (formMode == "Create")
            {
                try
                {
                    tblimgqry = new tblquecms();
                    var query = "insert into banner (guid,language_id,title,image,page,view,last_user,status," +
                        "date_created,date_updated,ad_hyperlink) Values ('" + banner1.guid + "'," + banner1.language_id + ",'" + banner1.title.Replace("'",@"\'") + "','" +
                        banner1.image + "','" + banner1.page + "','" + banner1.view + "','" + banner1.last_user + "','" + banner1.status + "','" +
                        DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + banner1.ad_hyperlink + "');";
                    tblimgqry.img_guid = banner1.guid;
                    tblimgqry.img_id = banner1.banner_id;
                    tblimgqry.que_desc = "Uploaded Image: " + banner1.image + " with Status: " + banner1.status;
                    tblimgqry.que_script = query;
                    tblimgqry.que_date = DateTime.Now;
                    tblimgqry.que_user = user;
                    tblimgqry.Status = "pending";
                    tblimgqry.created_date = DateTime.Now;
                    tblimgqry.updated_date = DateTime.Now;

                    _context.Add(tblimgqry);
                    _context.SaveChanges();

                    int tblquecmsId = tblimgqry.que_id;
                     

                    imgque = SaveImageQue(banner1, tblquecmsId, ref imgIddata, formMode, user);

                    if (imgque)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                }
            }

            if (formMode == "Edit")
            {
                try
                {
                    tblimgqry = new tblquecms();
                    var query = "update banner set language_id = " + banner1.language_id +
                        ", title = '" + banner1.title.Replace("'",@"\'") + "', image = '" + banner1.image + "', view = '" + banner1.view + "', status = '" +
                        banner1.status + "', date_created = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                        "', ad_hyperlink = '" + banner1.ad_hyperlink + "' where guid ='"  + banner1.guid + "';";
                    tblimgqry.img_guid = banner1.guid;
                    tblimgqry.img_id = banner1.banner_id;
                    tblimgqry.que_desc = "Uploaded Image: " + banner1.image + " with Status: " + banner1.status;
                    tblimgqry.que_script = query;
                    tblimgqry.que_date = DateTime.Now;
                    tblimgqry.que_user = user;
                    tblimgqry.Status = "pending";
                    tblimgqry.created_date = DateTime.Now;
                    tblimgqry.updated_date = DateTime.Now;

                    _context.Add(tblimgqry);
                    _context.SaveChanges();
                    int tblquecmsId = tblimgqry.que_id;
                    imgque = SaveImageQue(banner1, tblquecmsId, ref imgIddata, formMode, user);

                    if (imgque)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                }
            }

            if (formMode == "Delete")
            {
                try
                {
                    tblimgqry = new tblquecms();
                    var query = "update banner set banner.status = 'delete'" +
                        " where guid = '" + banner1.guid + "';";
                    tblimgqry.img_guid = banner1.guid;
                    tblimgqry.img_id = banner1.banner_id;
                    tblimgqry.que_desc = "Uploaded Image: " + banner1.image + " with Status: " + banner1.status;
                    tblimgqry.que_script = query;
                    tblimgqry.que_date = DateTime.Now;
                    tblimgqry.que_user = user;
                    tblimgqry.Status = "pending";
                    tblimgqry.created_date = DateTime.Now;
                    tblimgqry.updated_date = DateTime.Now;

                    _context.Add(tblimgqry);
                    _context.SaveChanges();
                    int tblquecmsId = tblimgqry.que_id;

                    imgque = SaveImageQue(banner1, tblquecmsId, ref imgIddata, formMode, user);

                    if (imgque)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                }
            }
            return imgque;

        }

        bool SaveImageQue(banners queimages, int tblquecmsId, ref tblquecmsimage imgque, string formMode, string user)
        {
            try
            {
                //var imgque = new tblquecmsimage();
                imgque = new tblquecmsimage();
                imgque.img_guid = queimages.guid;
                imgque.img_id = queimages.banner_id;
                imgque.tblquecms_id = tblquecmsId;
                imgque.img_description = queimages.image + "Status:" + queimages.status;
                imgque.img_name = queimages.image;
                imgque.img_uploadPath = _configuration.GetSection("BannersPath").Value;
                imgque.upload_date = DateTime.Now;
                imgque.img_user = user;//(from v in _context.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                imgque.Status = "pending";
                imgque.banner_type = "Homebanner";
                if (formMode == "Delete")
                {
                    imgque.Action = "Delete";
                }
                else
                {
                    imgque.Action = "Upload";
                }
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

        //public bool SaveQueDataDel(int id, string formMode)
        //{
        //    bool imgque = false;
        //    if (formMode == "Delete")
        //    {
        //        try
        //        {
        //            var tblimgqry = new tblquecms();
        //            var query = "delete from banner where banner_id = " + id;
        //            tblimgqry.img_id = banner1.banner_id;
        //            tblimgqry.que_desc = "Uploaded Image: " + banner1.image + " with Status: " + banner1.status;
        //            tblimgqry.que_script = query;
        //            tblimgqry.que_date = DateTime.Now;
        //            tblimgqry.que_user = (from v in _context.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
        //            tblimgqry.Status = "pending";
        //            tblimgqry.date_created = DateTime.Now;
        //            tblimgqry.date_updated = DateTime.Now;

        //            _context.Add(tblimgqry);
        //            _context.SaveChanges();

        //            imgque = SaveImageQue(banner1, ref imgIddata);

        //            if (imgque)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            var error = ex.Message;
        //        }
        //    }
        //    return true;
        //}

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

        public void Getimage(Guid img, ref bannerModel imgbanner)
        {
            //var banner = new bannerModel();
            if (img != null)
            {
                var imgfile = img;
                var strimg = _context.banner.Where(a => a.guid == imgfile).SingleOrDefault();
                imgbanner.guid = strimg.guid;
                imgbanner.language_id = strimg.language_id;
                imgbanner.title = strimg.title;
                imgbanner.image = strimg.image;
                imgbanner.page = strimg.page;
                imgbanner.view = strimg.view;
                imgbanner.last_user = strimg.last_user;
                imgbanner.status = strimg.status;
                imgbanner.date_created = DateTime.Now;
                imgbanner.date_updated = DateTime.Now;
                imgbanner.ad_hyperlink = strimg.ad_hyperlink;
                //_context.Add(imgbanner);
                //_context.SaveChanges();
                //bannerData imgdata = new bannerData();

                //uploadImagetoFTPServer(banner.image);
            }


        }

        public void GetImgbyId(Guid imgId, int tblque, int tblimgque, ref bool uploadedimg)
        {
            bool Ftpimgdone = false;
            tblquecmsimage imgque = new tblquecmsimage();
            //var imgsata = _context.Tblquecmsimage.Where(a => a.img_id == imgId && a.img_queId == tblimgque).FirstOrDefault();
            imgque.img_queId = tblimgque;
            var quedata = FindQueImgByid(imgque.img_queId);
            string imageName = quedata.img_name.ToString();

            if (imageName != null)
            {
                BlobURIDto blobURIDto = new BlobURIDto()
                {
                    ContainerName = _configuration.GetSection("StagingContainerRoot").Value,
                    FolderPath = _configuration.GetSection("BannersPath").Value,
                    FileName = imageName
                };

                //Helper helper = new Helper(_configuration);
                //string endPoint = helper.GenerateBlobStorageUri("Download", blobURIDto);
                string blobMethodName = _configuration.GetSection("BlobStorageDownloadMethod").Value;
                string paramContainer = _configuration.GetSection("ContainerParamName").Value;
                string paramFileName = _configuration.GetSection("ParamFileName").Value;
                string paramPath = _configuration.GetSection("PathParamName").Value;
                 
                string endPoint = blobMethodName + paramContainer + blobURIDto.ContainerName + paramFileName + blobURIDto.FileName + paramPath + blobURIDto.FolderPath;

                BlobDownloadResponseDto downloadDto = BlobStorageAPIService.BlobFileDownloader(endPoint).Result;

                IFormFile fileForm = new FormFile(downloadDto.Content, 0, downloadDto.Content.Length, downloadDto.Name, downloadDto.FileName);

                if (downloadDto != null)
                {
                    blobURIDto = new BlobURIDto()
                    {
                        ContainerName = _configuration.GetSection("ProdContainerRoot").Value,
                        FolderPath = _configuration.GetSection("BannersPath").Value,
                        FormFile = fileForm,
                        FileName = imageName
                    };

                    blobMethodName = _configuration.GetSection("BlobStorageUploadMethod").Value;
                    paramContainer = _configuration.GetSection("ContainerParamName").Value;
                    paramPath = _configuration.GetSection("PathParamName").Value;
                   
                    endPoint = blobMethodName + paramContainer + blobURIDto.ContainerName + paramPath + blobURIDto.FolderPath;

                    BlobUploadResponseDto uploadResponseDto = BlobStorageAPIService.BlobFileUploader(endPoint, blobURIDto).Result;

                    if (uploadResponseDto.status != null)
                        Ftpimgdone = true;
                }

                //Ftpimgdone = uploadImagetoFTPServer(imageName);
            }
            var pendingimagequetbl = _context.Tblquecms.Where(a => a.img_guid == imgId && a.que_id == tblque).FirstOrDefault();
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

                FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(ProdfilefullPath + "/" + imgName);
                uploadRequest.Credentials = new NetworkCredential(FtpUser, FtpPass);
                uploadRequest.UseBinary = true;
                uploadRequest.UsePassive = true;
                uploadRequest.KeepAlive = true;
                //uploadRequest.ContentLength = 4096;
                uploadRequest.EnableSsl = true;

                uploadRequest.Method = WebRequestMethods.Ftp.UploadFile.ToLower();

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

        public banners FindbyId(Guid id)
        {
            if(!string.IsNullOrEmpty(id.ToString()))
            {
                return _context.banner.FirstOrDefault(x=>x.guid==id);
            }
            else
            {
                return null;
            }
        }

        public bool RemovebyId(Guid id)
        {
            bool Isdeleted = false;
            var record = _context.banner.FirstOrDefault(x=>x.guid == id);
            if (record != null)
            {
                //_context.banner.Remove(record);
                record.status = "delete";
                record.date_updated = DateTime.Now;
                Isdeleted = Convert.ToBoolean(_context.SaveChanges());
                if(Isdeleted == true)
                { return Isdeleted; }
                else { return false; }
                
            }
            else { return false; }
        }

        public Boolean Edit(banners model)
        {
            bool IsUpdate = false;
            var existsbanner = FindbyId(model.guid);
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
                    existsbanner.date_updated = DateTime.Now;
                    IsUpdate = Convert.ToBoolean(_context.SaveChanges());

                }
            }
            catch (Exception)
            {

                throw;
            }
            return IsUpdate;

        }

        public bool CheckFileOnProd(banners model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT guid FROM banner where guid = '" + model.guid + "';", con);
                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    currentVal = Convert.ToString(dr["guid"].ToString());

                }
                dr.Close();
                con.Close();
            }

            if (currentVal != null)
            {
                return true;
            }
            else { return false; }
        }
    }
}
