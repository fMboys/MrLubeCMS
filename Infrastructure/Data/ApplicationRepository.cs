using CMS.Core.Entities;
using CMS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using CMS.Core.DTOs;
using System.Data.Entity;

namespace CMS.Infrastructure.Data
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly CMSDbContext _dbContext;
        public readonly IConfiguration _configuration;

        public ApplicationRepository(CMSDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieve details of an image from Image Queue using guid.
        /// </summary>
        /// <param name="imageGUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public tblquecmsimage GetImageDetailByGUID(Guid imageGUID)
        {
			tblquecmsimage imageData = new tblquecmsimage();
			try
			{
                if (imageGUID != Guid.Empty)
				    imageData = _dbContext.Tblquecmsimage.Where(x => x.img_guid == imageGUID).FirstOrDefault();

                return imageData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public List<ImageSpecification> GetAllImagesSpecifications()
        {
            try
            {
                List<ImageSpecification> imageSpecs = _dbContext.image_Specs.ToList();

                return imageSpecs;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public tblquecmsimage GetImageDetailByID(Guid imageID, int imageQueueId)
        {
            tblquecmsimage imageData = new tblquecmsimage();
            try
            {
                imageData = _dbContext.Tblquecmsimage.Where(x => x.img_queId == imageQueueId && x.img_guid == imageID).FirstOrDefault();

                return imageData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool UpdateImageDetailByGUID(Guid imageGUID)
		{
            tblquecmsimage imageData = new tblquecmsimage();
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (imageGUID != Guid.Empty)
                            imageData = _dbContext.Tblquecmsimage.Where(x => x.img_guid == imageGUID).FirstOrDefault();

                        if (imageData != null && imageData.Status == "pending")
                        {
                            imageData.Status = "Completed";
                            imageData.img_updatedDate = DateTime.Now;

                            _dbContext.SaveChanges();
                            transaction.Commit();

                            return true;
                        }
                        else if (imageData != null && imageData.Status == "completed")
                        {
                            imageData.Action = "Delete";
                            imageData.img_updatedDate = DateTime.Now;

                            _dbContext.SaveChanges();
                            transaction.Commit();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
			}
		}

        public bool UpdateScriptQueueDetailByGUID(Guid imageGUID)
        {
            List<tblquecms> queueScripts = null;
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (imageGUID != Guid.Empty)
                            queueScripts = _dbContext.Tblquecms.Where(x => x.img_guid == imageGUID).ToList();
                        //queueScripts = _dbContext.Tblquecms.Where(x => x.que_id == scriptQueueId && x.img_guid == imageGUID).FirstOrDefault();

                        if (queueScripts != null)
                        {
                            foreach (var script in queueScripts)
                            {
                                if (script.Status == "pending")
                                {
                                    script.Status = "completed";
                                    script.updated_date = DateTime.Now;

                                    _dbContext.SaveChanges();
                                }
                                
                            }
                            

                            //_dbContext.Attach(imageData);
                            //_dbContext.Entry(imageData).CurrentValues.SetValues(imageData); //TODO: test

                            //_dbContext.Entry(imageData).Property(x => x.Status).IsModified = true;
                            //_dbContext.Entry(imageData).Property(x => x.img_updatedDate).IsModified = true;
                            
                            transaction.Commit();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }
        /// <summary>
        /// Use to check Image table queue for any pending changes.
        /// </summary>
        /// <returns>Boolean result</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckImageQueue(Guid guid)
        {
            List<tblquecmsimage> qImageData = new List<tblquecmsimage>();
            try
            {
                qImageData = _dbContext.Tblquecmsimage.Where(x => x.Status == "pending" && x.img_guid == guid).ToList();

                if (qImageData.Count <= 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        /// <summary>
        /// Use to check table queues for any pending changes.
        /// </summary>
        /// <returns>Boolean result</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckImageQueue()
        {
            //List<tblquecmsimage> qImageData = new List<tblquecmsimage>();
            var tblque = new List<trackQueModel>();
            try
            {
                //qImageData = _dbContext.Tblquecmsim.Where(x => x.Status == "pending").ToList();
                tblque = (from tp in _dbContext.Tblquecmsimage
                 join ts
                in _dbContext.Tblquecms on tp.tblquecms_id equals ts.que_id
                 //where tp.img_id == ts.img_id
                 select new trackQueModel
                 {
                     guid = ts.img_guid,
                     img_id = tp.img_id,
                     tblquecms_id = ts.que_id,
                     tblquecmsimage_id = tp.img_queId,
                     img_name = tp.img_name,
                     img_uploadPath = tp.img_uploadPath,
                     banner_type = tp.banner_type,
                     action_done = tp.Action,
                     que_script = ts.que_script,
                     status = tp.Status,
                     img_updatedDate = ts.updated_date
                 }).ToList();
                tblque = tblque.Where(x => x.status == "pending").ToList();

                if (tblque.Count <= 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool CheckStoreExist(string storeNo,int lang,string device, int shopTire_id,ref string comastores)
        {
            List<ShopTire> shopTire = new List<ShopTire>();
            var stores = storeNo.Split(",");
            
            foreach (var store in stores) {
                
                shopTire = _dbContext.ShopTire.Where(x => x.store_num == Convert.ToInt32(store.ToString()) && x.language_id == lang && x.view == device && x.status != "delete").ToList();
                comastores += Convert.ToString(store) + ",";
            }
            if(shopTire.Count > 0 ) {
                return true;
            }
            else
            {
                return false;
            }
            //if(shopTire_id == 0)
            //{

            //    List<ShopTire> shopTire = new List<ShopTire>();
            //    shopTire = _dbContext.ShopTire.Where(x => x.store_num == storeNo && x.language_id == lang && x.view == device && x.status != "delete").ToList();
            //    if (shopTire.Count > 0)
            //    {
            //        return true;
            //    }
            //}
            //else
            //{

            //    List<ShopTire> shopTire = new List<ShopTire>();
            //    shopTire = _dbContext.ShopTire.Where(x => x.store_num == storeNo && x.language_id == lang && x.view == device && x.shopTire_id != shopTire_id && x.status != "delete").ToList();
            //    if (shopTire.Count > 0)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }


        [Obsolete]
        /// <summary>
        /// Check Image Queue to verify the file name is already taken or not.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Boolean</returns>
        /// <exception cref="Exception"></exception>
        public bool VerifyNameImageQueue(string fileName)
        {
            bool exist = true;
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                    exist = _dbContext.Tblquecmsimage.Where(x => x.img_name == fileName).Any();

                if (exist == true)
                    return false;
                else if (exist == false)
                    return true;

                return exist;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public List<SubMenu> GetSubMenus(int lang,string view)
        {
            List<SubMenu> subMenus = new List<SubMenu>();
            int[] menuIDs = { 1, 2, 4,6,7 };
            //SubMenu homeMenu = new SubMenu { url_key = "home"};
            try
            {
                int[] itemlist = { 5, 8,15,17 };
                subMenus = _dbContext.sub_menu.Where(x => menuIDs.Contains(x.menu_id) && x.language_id == lang && !(itemlist.Contains(x.item_id))).ToList();
                //subMenus.Add(homeMenu);
                return subMenus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public List<SubMenu> GetSelectedSubMenus(string selectedMenus)
        {
            List<SubMenu> subMenus = new List<SubMenu>();
            string[] subMenuIDs = selectedMenus.Split(',');
            try
            {
                foreach (string id in subMenuIDs)
                {
                    SubMenu? subMenu = new SubMenu();
                    int subMenuID = Convert.ToInt32(id.Trim());
                    subMenu = _dbContext.sub_menu.Find(subMenuID);

                    if (subMenu != null)
                        subMenus.Add(subMenu);
                }

                return subMenus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool isFilependingbanner(Guid id, string bannerType, ref List<tblquecmsimage> tblquecmsimage)
        {
            List<tblquecms> pendingScript = new List<tblquecms>();
            tblquecmsimage = _dbContext.Tblquecmsimage.Where(x => x.img_guid == id && x.Status == "pending" && x.banner_type == bannerType).ToList();
            pendingScript = _dbContext.Tblquecms.Where(a => a.img_guid == id && a.Status == "pending").ToList();
            if (pendingScript.Count >= 0)
            {
                foreach (var img in pendingScript)
                {
                    var scriptdata = GetScriptDetailByID(img.img_guid, img.que_id);
                    if (scriptdata != null)
                    {
                        RemovebyId(scriptdata.img_guid);
                    }
                }
            }
            else { return false; }
            return true;
        }

        public tblquecms GetScriptDetailByID(Guid? guid, int? imageQueueId)
        {
            tblquecms? imageData = new tblquecms();
            try
            {
                imageData = _dbContext.Tblquecms.Where(x => x.que_id == imageQueueId && x.img_guid == guid).FirstOrDefault();

                return imageData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public tblquecms RemovebyId(Guid? id)
        {
            var record = _dbContext.Tblquecms.FirstOrDefault(x => x.img_guid == id);
            if (record != null)
            {
                _dbContext.Remove(record);
                _dbContext.SaveChanges();

            }
            return record;
        }

        public tblquecmsimage RemoveImgQueData(int id)
        {
            var record = _dbContext.Tblquecmsimage.Find(id);
            if (record != null)
            {
                _dbContext.Remove(record);
                _dbContext.SaveChanges();

            }
            return record;
        }

        public bool SaveQueDataWithnoImage(banners banner1, ref tblquecmsimage imgIddata, ref tblquecms tblimgqry, string formMode)
        {
            bool imgque = false;
            if (formMode == "UploadPage")
            {
                try
                {
                    tblimgqry = new tblquecms();
                    var query = "update banner set banner.status = 'delete'" +
                        "where image = '" + banner1.image + "' OR banner_id = " + banner1.banner_id + ";";
                    tblimgqry.img_id = banner1.banner_id;
                    tblimgqry.que_desc = "Uploaded Image: " + banner1.image + " with Status: " + banner1.status;
                    tblimgqry.que_script = query;
                    tblimgqry.que_date = DateTime.Now;
                    tblimgqry.que_user = (from v in _dbContext.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                    tblimgqry.Status = "pending";
                    tblimgqry.created_date = DateTime.Now;
                    tblimgqry.updated_date = DateTime.Now;

                    _dbContext.Add(tblimgqry);
                    _dbContext.SaveChanges();
                    int tblquecmsId = tblimgqry.que_id;

                    imgque = SaveImageQueWithblank(banner1, tblquecmsId, ref imgIddata, formMode);

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


        public bool SaveImageQueWithblank(banners queimages, int tblquecmsId, ref tblquecmsimage imgque, string formMode)
        {

            try
            {
                //var imgque = new tblquecmsimage();
                imgque = new tblquecmsimage();
                imgque.img_id = queimages.banner_id;
                imgque.tblquecms_id = tblquecmsId;
                imgque.img_description = queimages.image + "Status:" + queimages.status;
                imgque.img_name = "UploadPage";
                imgque.img_uploadPath = _configuration.GetSection("BannersPath").Value;
                imgque.upload_date = DateTime.Now;
                imgque.img_user = (from v in _dbContext.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                imgque.Status = "pending";
                imgque.banner_type = "Homebanner";
                if (formMode == "Delete")
                {
                    imgque.Action = "Delete";
                }
                else if (formMode == "UploadPage")
                {
                    imgque.Action = "UploadPage";
                }
                else if (formMode == "Upload")
                {
                    imgque.Action = "Upload";
                }
                imgque.img_createdDate = DateTime.Now;
                imgque.img_updatedDate = DateTime.Now;

                _dbContext.Add(imgque);
                _dbContext.SaveChanges();

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

        public List<SubMenu> GetCheckedPages(Guid GUID)
        {
            List<SubMenu> checkedMenus = new List<SubMenu>();
            try
            {
                List<FloatingImage> floatingImages = _dbContext.floating_images.Where(x => x.guid == GUID).ToList();

                if (floatingImages.Count > 0 && floatingImages != null)
                {
                    foreach (FloatingImage image in floatingImages)
                    {
                        Menu menu = null;
                        var pageTitle = "";
                        if(image.page == "Others" && image.language_id == 1)
                        {
                           pageTitle = "Other";
                           menu  = _dbContext.menu.Where(m => m.title == pageTitle && m.language_id == image.language_id).FirstOrDefault();
                        }
                        else if(image.page == "Others" && image.language_id == 2)
                        {
                            pageTitle = "Autre";
                            menu = _dbContext.menu.Where(m => m.title == pageTitle && m.language_id == image.language_id).FirstOrDefault();
                        }
                        else
                        {
                            menu = _dbContext.menu.Where(m => m.title == image.page && m.language_id == image.language_id).FirstOrDefault();
                        }
                        

                        if (menu != null)
                        {
                            SubMenu subMenu = _dbContext.sub_menu.Where(x => x.url_key == image.url_key && x.language_id == image.language_id).FirstOrDefault();
                            
                            if (subMenu != null)
                                checkedMenus.Add(subMenu);
                        }
                    }

                    return checkedMenus;
                }
                return checkedMenus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        /// <summary>
        /// Delete the image entry from Image queue table by guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool RemoveQueueImageByGuid(Guid GUID)
        {
            bool isRemoved = false;
            tblquecmsimage queueImage = null;
            
            try
            {
                queueImage = _dbContext.Tblquecmsimage.Where(x => x.img_guid == GUID).FirstOrDefault();

                if (queueImage != null)
                {
                    _dbContext.Remove(queueImage);
                    _dbContext.SaveChanges();

                    return isRemoved = true;
                }
                return isRemoved = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool RemoveShopTireQueueImageByGuid(Guid GUID)
        {
            bool isRemoved = false;
            List<tblquecmsimage> queueImage = null;

            try
            {
                queueImage = _dbContext.Tblquecmsimage.Where(x => x.img_guid == GUID).ToList();

                if (queueImage != null)
                {
                    _dbContext.RemoveRange(queueImage);
                    _dbContext.SaveChanges();

                    return isRemoved = true;
                }
                return isRemoved = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Delete all the scripts from Scripts queue table that matchs the given guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool RemoveAllQueueScriptsByGuid(Guid GUID)
        {
            bool isRemoved = false;
            List<tblquecms> queueScripts = null;
            try
            {
                queueScripts = _dbContext.Tblquecms.Where(x => x.img_guid == GUID).ToList();

                if (queueScripts != null)
                {
                    //_dbContext.AttachRange(queueScripts);
                    //_dbContext.Entry<List<tblquecms>>(queueScripts).State = EntityState.Deleted;
                    _dbContext.RemoveRange(queueScripts);
                    _dbContext.SaveChanges();

                   return isRemoved = true;
                }
                return isRemoved = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool CheckFileOnProd(Guid id)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM banner where guid = '" + id + "';", con);
                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    currentVal = Convert.ToString(dr["image"].ToString());

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

        public bool UpdateImageDetailByID(Guid imageID, int imageQueueId)
        {
            tblquecmsimage imageData = new tblquecmsimage();
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        imageData = _dbContext.Tblquecmsimage.Where(x => x.img_queId == imageQueueId && x.img_guid == imageID).FirstOrDefault();

                        if (imageData != null && imageData.Status == "pending")
                        {
                            imageData.Status = "Completed";
                            imageData.img_updatedDate = DateTime.Now;

                            //_dbContext.Attach(imageData);
                            //_dbContext.Entry(imageData).CurrentValues.SetValues(imageData); //TODO: test

                            //_dbContext.Entry(imageData).Property(x => x.Status).IsModified = true;
                            //_dbContext.Entry(imageData).Property(x => x.img_updatedDate).IsModified = true;
                            _dbContext.SaveChanges();
                            transaction.Commit();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }

        public bool UpdateScriptQueueDetailByID(Guid imageID, int scriptQueueId)
        {
            tblquecms scriptQueue = new tblquecms();
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        scriptQueue = _dbContext.Tblquecms.Where(x => x.que_id == scriptQueueId && x.img_guid == imageID).FirstOrDefault();

                        if (scriptQueue != null && scriptQueue.Status == "pending")
                        {
                            scriptQueue.Status = "Completed";
                            scriptQueue.updated_date = DateTime.Now;

                            //_dbContext.Attach(imageData);
                            //_dbContext.Entry(imageData).CurrentValues.SetValues(imageData); //TODO: test

                            //_dbContext.Entry(imageData).Property(x => x.Status).IsModified = true;
                            //_dbContext.Entry(imageData).Property(x => x.img_updatedDate).IsModified = true;
                            _dbContext.SaveChanges();
                            transaction.Commit();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                
            }
        }

        /// <summary>
        /// Set the folder path of blob storage container according to provided banner type.
        /// </summary>
        /// <param name="bannerType"></param>
        /// <returns>Folder path of Blob Storage Container</returns>
        public string GetBlobFolderPathByBanner(string bannerType)
        {
            string folderPath = string.Empty;

            try
            {
                if (bannerType == "Homebanner")
                    folderPath = _configuration.GetSection("BannersPath").Value;
                else if (bannerType == "coupon")
                    folderPath = _configuration.GetSection("CouponImagesPath").Value;
                else if (bannerType == "FloatingImage")
                    folderPath = _configuration.GetSection("FloatingPath").Value;
                else if (bannerType == "LeftAd")
                    folderPath = _configuration.GetSection("AdsPath").Value;
                else if (bannerType == "promo")
                    folderPath = _configuration.GetSection("PromoImagesPath").Value;
                else if (bannerType == "ShopTire")
                    folderPath = _configuration.GetSection("ShopTiresPath").Value;

                return folderPath;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        //TODO: Create a generic solution for Script and Image Queue.
        public bool SaveQueueScriptAndData(string mode, GeneralCMSDto generalDto, ref TrackingQueuesDto queuesDto)
        {
            bool isAdded = false;
            tblquecms qScriptData = new tblquecms();
            var queryScript = string.Empty;
            try
            {
                if (generalDto.BannerType == "LeftAd" && mode == "Create")
                {
                    queryScript = "INSERT INTO ads_images(guid, language_id, title, image, page, url_key, view, last_user, status, date_created, date_updated, ad_hyperlink)" +
                        "VALUES('" + generalDto.guid + "'," + generalDto.LanguageId + ",'" + generalDto.Title + "','" + generalDto.ImageName + "','" + generalDto.ViewPage + "','" + generalDto.UrlKey + "','" + generalDto.ViewDevice + "','" +
                        generalDto.LastUser + "','" + generalDto.ImageStatus + "','" + Convert.ToDateTime(generalDto.CreatedDate).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(generalDto.UpdatedDate).ToString("yyyy-MM-dd H:mm:ss") + "','" + generalDto.Hyperlink + "')";
                    qScriptData.img_id = generalDto.LeftAdID;
                    qScriptData.img_guid = generalDto.guid;
                    qScriptData.que_desc = "Uploaded Image: " + generalDto.ImageName + " with Status: " + generalDto.ImageStatus;
                }
                else if (generalDto.BannerType == "LeftAd" && mode == "Edit")
                {
                    queryScript = "UPDATE ads_images SET language_id = " + generalDto.LanguageId + ", title = '" + generalDto.Title + "', image = '" + generalDto.ImageName + "', page = '" + generalDto.ViewPage +
                        "', view = '" + generalDto.ViewDevice + "', url_key = '" + generalDto.UrlKey + "', last_user = '" + generalDto.LastUser +
                        "', status = '" + generalDto.ImageStatus + "', date_created = '" + Convert.ToDateTime(generalDto.CreatedDate).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(generalDto.UpdatedDate).ToString("yyyy-MM-dd H:mm:ss") + "', ad_hyperlink = '" + generalDto.Hyperlink + "' WHERE id = " + generalDto.LeftAdID + ";";
                    qScriptData.img_id = generalDto.LeftAdID;
                    qScriptData.img_guid = generalDto.guid;
                    qScriptData.que_desc = "Uploaded Image: " + generalDto.ImageName + " with Status: " + generalDto.ImageStatus;
                }
                else if (generalDto.BannerType == "LeftAd" && mode == "Delete")
                {
                    queryScript = "UPDATE ads_images SET status = 'delete', date_updated = '" + Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + generalDto.guid + "';";
                    qScriptData.img_id = generalDto.LeftAdID;
                    qScriptData.img_guid = generalDto.guid;
                    qScriptData.que_desc = "Uploaded Image: " + generalDto.ImageName + " with Status: " + generalDto.ImageStatus;
                }

                qScriptData.que_script = queryScript;
                qScriptData.que_date = DateTime.Now;
                qScriptData.que_user = (from v in _dbContext.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                qScriptData.Status = "Pending";
                qScriptData.created_date = DateTime.Now;
                qScriptData.updated_date = DateTime.Now;

                queuesDto.ScriptQueueId = qScriptData.que_id;
                queuesDto.ImageGUID = qScriptData.img_guid;

                return isAdded;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        //shoTire image script que 
        public bool UpdateShoptireImageQueByGUID(Guid imageGUID)
        {
            List<tblquecmsimage> queueScripts = null;
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (imageGUID != Guid.Empty)
                            queueScripts = _dbContext.Tblquecmsimage.Where(x => x.img_guid == imageGUID).ToList();
                        //queueScripts = _dbContext.Tblquecms.Where(x => x.que_id == scriptQueueId && x.img_guid == imageGUID).FirstOrDefault();

                        if (queueScripts != null)
                        {
                            foreach (var script in queueScripts)
                            {
                                if (script.Status == "pending")
                                {
                                    script.Status = "completed";
                                    script.img_updatedDate = DateTime.Now;

                                    _dbContext.SaveChanges();
                                }

                            }


                            //_dbContext.Attach(imageData);
                            //_dbContext.Entry(imageData).CurrentValues.SetValues(imageData); //TODO: test

                            //_dbContext.Entry(imageData).Property(x => x.Status).IsModified = true;
                            //_dbContext.Entry(imageData).Property(x => x.img_updatedDate).IsModified = true;

                            transaction.Commit();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }

        public bool UpdateImageDetailBycmsqueId(Guid imageID, int imageQueueId)
        {
            tblquecmsimage imageData = new tblquecmsimage();
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        imageData = _dbContext.Tblquecmsimage.Where(x => x.tblquecms_id == imageQueueId && x.img_guid == imageID).FirstOrDefault();

                        if (imageData != null && imageData.Status == "pending")
                        {
                            imageData.Status = "Completed";
                            imageData.img_updatedDate = DateTime.Now;

                            //_dbContext.Attach(imageData);
                            //_dbContext.Entry(imageData).CurrentValues.SetValues(imageData); //TODO: test

                            //_dbContext.Entry(imageData).Property(x => x.Status).IsModified = true;
                            //_dbContext.Entry(imageData).Property(x => x.img_updatedDate).IsModified = true;
                            _dbContext.SaveChanges();
                            transaction.Commit();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }

    }
}
