using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace CMS.Infrastructure.Data
{
    public class LeftAdRepository : ILeftAdRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IApplicationRepository _appRepo;

        public LeftAdRepository(CMSDbContext dbContext, IConfiguration configuration, IApplicationRepository appRepo)
        {
            _dbContext = dbContext;
            _appRepo = appRepo;
            _configuration = configuration;
        }

        /// <summary>
        /// Get a list of all the data of left ads from database.
        /// </summary>
        /// <returns>IEnumerable of leftAds</returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<LeftAd> GetAllLeftAds()
        {
            IEnumerable<LeftAd> leftAds = null;
            try
            {                
                leftAds = _dbContext.ads_images.Where(x => x.status != "delete").ToList().DistinctBy(x => x.guid).OrderBy(x => x.date_updated);
                return leftAds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Retrieve all the pages that have active left ad images.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<LeftAd> GetAllLeftAdPages(int lang,string view)
        {
            IEnumerable<LeftAd> leftAds = null;
            try
            {
                leftAds = _dbContext.ads_images.Where(x => x.image != "" && x.url_key != "" && x.status != "delete" && x.status != "inactive" && x.language_id == lang && x.view == view).ToList();
                return leftAds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Retieve list of pages against a specific left ad image.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<SubMenu> GetLeftAdCheckedPages(Guid GUID)
        {
            List<SubMenu> checkedMenus = new List<SubMenu>();
            try
            {
                List<LeftAd> leftAds = _dbContext.ads_images.Where(x => x.guid == GUID).ToList();
                
                if (leftAds.Count > 0 && leftAds != null)
                {
                    foreach (LeftAd ad in leftAds)
                    {
                        Menu menu = null;
                        var pageTitle = "";
                        if (ad.page == "Others" && ad.language_id == 1)
                        {
                            pageTitle = "Other";
                            menu = _dbContext.menu.Where(m => m.title == pageTitle && m.language_id == ad.language_id).FirstOrDefault();
                        }
                        else if (ad.page == "Others" && ad.language_id == 2)
                        {
                            pageTitle = "Autre";
                            menu = _dbContext.menu.Where(m => m.title == pageTitle && m.language_id == ad.language_id).FirstOrDefault();

                        }
                        else
                        {
                            menu = _dbContext.menu.Where(m => m.title == ad.page && m.language_id == ad.language_id).FirstOrDefault();
                        }
                        
                        if (menu != null)
                        {
                            SubMenu subMenu = _dbContext.sub_menu.Where(x => x.url_key == ad.url_key && x.language_id == ad.language_id).FirstOrDefault();

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

        public TrackingQueuesDto Add(string selectedPages, LeftAd leftAd)
        {
            bool isAdded = false;
            LeftAd leftAdData = null;
            try
            {
                if (!string.IsNullOrEmpty(selectedPages))
                {
                    string[] subMenuIDs = selectedPages.Split(',');
                    
                    TrackingQueuesDto queuesDto = new TrackingQueuesDto();

                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var id in subMenuIDs)
                            {
                                SubMenu? subMenu = new SubMenu();
                                Menu menu = new Menu();
                                int subMenuID = Convert.ToInt32(id.Trim());
                                subMenu = _dbContext.sub_menu.Find(subMenuID);
                                string menuPage = "";

                                if (subMenu != null)
                                {
                                    subMenu = _dbContext.sub_menu.Where(x => x.item_id == subMenu.item_id && x.language_id == leftAd.language_id).FirstOrDefault(); //To Get UrlKey for selected language.
                                    if (subMenu.item_id == 35)
                                    {
                                        menu.title = "Others";
                                    }
                                    else if (subMenu.item_id == 36)
                                    {
                                        menu.title = "Services";
                                    }
                                    else
                                    {
                                        menu = _dbContext.menu.Where(x => (x.menu_id == subMenu.menu_id || subMenu.item_id == 35 || subMenu.item_id == 36) && x.language_id == leftAd.language_id).FirstOrDefault();//Todo: replace with a method create in apprepo
                                    }
                                    //menu = _dbContext.menu.Where(x => x.menu_id == subMenu.menu_id && x.language_id == leftAd.language_id).FirstOrDefault();
                                }
                                
                                //Change the Tires Menu into Services
                                if (menu.title == "Tires" && leftAd.language_id == 1)
                                {
                                    menuPage = "Services";
                                }
                                else if (menu.title == "Pneus" && leftAd.language_id == 2)
                                {
                                    menuPage = "Services";
                                }
                                else if (menu.title == "Autre" && leftAd.language_id == 2)
                                {
                                    menuPage = "Others";
                                }
                                else if (menu.title == "Other" && leftAd.language_id == 1)
                                {
                                    menuPage = "Others";
                                }
                                else if (subMenu.item_id == 35)
                                {
                                    menuPage = "Others";
                                }
                                else if (subMenu.item_id == 36)
                                {
                                    menuPage = "Services";
                                }
                                else
                                {
                                    menuPage = menu.title.ToString();
                                }

                                if (menu != null)
                                {
                                    leftAdData = new LeftAd()
                                    {
                                        guid = leftAd.guid,
                                        language_id = leftAd.language_id,
                                        title = leftAd.title,
                                        image = leftAd.image,
                                        page = menuPage,
                                        url_key = subMenu.url_key,
                                        view = leftAd.view,
                                        last_user = leftAd.last_user,
                                        status = leftAd.status,
                                        date_created = leftAd.date_created,
                                        date_updated = leftAd.date_updated,
                                        ad_hyperlink = leftAd.ad_hyperlink
                                    };

                                    _dbContext.Add(leftAdData);
                                    _dbContext.SaveChanges();
                                }

                                //add script to queue
                                SaveScriptAndData("Create", leftAdData, ref queuesDto, leftAd.last_user);

                            }

                            SaveImageDetails(leftAdData, ref queuesDto, string.Empty);

                            transaction.Commit();
                            queuesDto.LeftAdID = leftAdData.Id;
                            queuesDto.ScriptQueueId = queuesDto.ScriptQueueId;

                            return queuesDto;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public TrackingQueuesDto Update(string selectedPages, LeftAd leftAdData)
        {
            LeftAd leftAd = null;
            TrackingQueuesDto queuesDto = new TrackingQueuesDto();
            try
            {
                if (!string.IsNullOrEmpty(selectedPages))
                {
                    string[] subMenuIDs = selectedPages.Split(',');
                    

                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            List<LeftAd> leftAds = GetLeftAdsByGUID(leftAdData.guid).ToList();
                            _dbContext.ads_images.RemoveRange(leftAds);

                            _appRepo.RemoveQueueImageByGuid(leftAdData.guid);
                            _appRepo.RemoveAllQueueScriptsByGuid(leftAdData.guid);

                            foreach (var id in subMenuIDs)
                            {
                                SubMenu? subMenu = new SubMenu();
                                Menu menu = new Menu();
                                int subMenuID = Convert.ToInt32(id.Trim());
                                subMenu = _dbContext.sub_menu.Find(subMenuID);
                                string menuPage = "";

                                if (subMenu != null)
                                {
                                    subMenu = _dbContext.sub_menu.Where(x => x.item_id == subMenu.item_id && x.language_id == leftAdData.language_id).FirstOrDefault();

                                    if (subMenu.item_id == 35)
                                    {
                                        menu.title = "Others";
                                    }
                                    else if (subMenu.item_id == 36)
                                    {
                                        menu.title = "Services";
                                    }
                                    else
                                    {
                                        menu = _dbContext.menu.Where(x => (x.menu_id == subMenu.menu_id || subMenu.item_id == 35 || subMenu.item_id == 36) && x.language_id == leftAdData.language_id).FirstOrDefault();//Todo: replace with a method create in apprepo
                                    }

                                    //menu = _dbContext.menu.Where(x => x.menu_id == subMenu.menu_id && x.language_id == leftAdData.language_id).FirstOrDefault();
                                }
                                
                                //Change the Tires Menu into Services
                                if (menu.title == "Tires" && leftAdData.language_id == 1)
                                {
                                    menuPage = "Services";
                                }
                                else if (menu.title == "Pneus" && leftAdData.language_id == 2)
                                {
                                    menuPage = "Services";
                                }
                                else if (menu.title == "Autre" && leftAdData.language_id == 2)
                                {
                                    menuPage = "Others";
                                }
                                else if (menu.title == "Other" && leftAdData.language_id == 1)
                                {
                                    menuPage = "Others";
                                }
                                else if (subMenu.item_id == 35)
                                {
                                    menuPage = "Others";
                                }
                                else if (subMenu.item_id == 36)
                                {
                                    menuPage = "Services";
                                }
                                else
                                {
                                    menuPage = menu.title.ToString();
                                }


                                if (menu != null)
                                {
                                    leftAd = new LeftAd()
                                    {
                                        guid = leftAdData.guid,
                                        language_id = leftAdData.language_id,
                                        title = leftAdData.title,
                                        image = leftAdData.image,
                                        page = menuPage,
                                        url_key = subMenu.url_key,
                                        view = leftAdData.view,
                                        last_user = leftAdData.last_user,
                                        status = leftAdData.status,
                                        date_created = DateTime.Now,
                                        date_updated = DateTime.Now,
                                        ad_hyperlink = leftAdData.ad_hyperlink
                                    };

                                    _dbContext.Add(leftAd);
                                    _dbContext.SaveChanges();

                                    SaveScriptAndData("Create", leftAd, ref queuesDto, leftAdData.last_user);
                                }
                            }

                            SaveImageDetails(leftAdData, ref queuesDto, string.Empty);

                            transaction.Commit();
                            return queuesDto;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            return queuesDto;
                        }
                    }
                }
                return queuesDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        /// <summary>
        /// Get a left ad image data based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        public LeftAd FindByGuid(Guid guid)
        {
            LeftAd? leftAd = null;
            try
            {
                if (guid != Guid.Empty)
                {
                    leftAd = _dbContext.ads_images.Where(x => x.guid == guid).FirstOrDefault();
                }
                return leftAd;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Get a left ad data based on ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        public LeftAd FindByID(int id)
        {
            LeftAd? leftAd = null;
            try
            {
                if (id > 0)
                {
                    leftAd = _dbContext.ads_images.Find(id);
                }
                return leftAd;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool SaveScriptAndData(string formMode, LeftAd leftAd, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO ads_images(guid, language_id, title, image, page, url_key, view, last_user, status, date_created, date_updated, ad_hyperlink)" +
                        "VALUES('" + leftAd.guid + "'," + leftAd.language_id + ",'" + leftAd.title.Replace("'",@"\'") + "','" + leftAd.image + "','" + leftAd.page + "','" + leftAd.url_key + "','" + leftAd.view + "','" +
                        leftAd.last_user + "','" + leftAd.status + "','" + Convert.ToDateTime(leftAd.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(leftAd.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "','" + leftAd.ad_hyperlink + "')";
                    qScriptData.img_id = leftAd.Id;
                    qScriptData.img_guid = leftAd.guid;
                    qScriptData.que_desc = "Uploaded Image: " + leftAd.image + " with Status: " + leftAd.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;// (from v in _dbContext.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;
                    queuesDto.ImageGUID = qScriptData.img_guid;

                    return isAdded;
                }
                else if (formMode == "Edit")
                {
                    //Todo: for floating image scenario
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "UPDATE ads_images SET language_id = " + leftAd.language_id + ", title = '" + leftAd.title.Replace("'",@"\'") + "', image = '" + leftAd.image + "', page = '" + leftAd.page +
                        "', view = '" + leftAd.view + "', url_key = '" + leftAd.url_key + "', last_user = '" + leftAd.last_user +
                        "', status = '" + leftAd.status + "', date_created = '" + Convert.ToDateTime(leftAd.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(leftAd.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', ad_hyperlink = '" + leftAd.ad_hyperlink + "' WHERE shopTire_id = " + leftAd.Id + ";";

                    qScriptData.img_id = leftAd.Id;
                    qScriptData.img_guid = leftAd.guid;
                    qScriptData.que_desc = "Uploaded Image: " + leftAd.image + " with Status: " + leftAd.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;//(from v in _dbContext.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;
                    queuesDto.ImageGUID = qScriptData.img_guid;

                    isAdded = SaveImageDetails(leftAd, ref queuesDto, formMode);

                    if (isAdded)
                    {
                        queuesDto.ScriptQueueId = qScriptData.que_id;
                        queuesDto.ScriptQueueStatus = qScriptData.Status;
                    }

                    return isAdded;
                }
                else if (formMode == "Delete")
                {//Todo: for floating image scenario
                    bool isDelete;

                    tblquecms qScriptData = new tblquecms();

                    var queryScript = "UPDATE ads_images SET status = 'delete', date_updated = '" + Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + leftAd.guid + "';";
                    qScriptData.img_id = leftAd.Id;
                    qScriptData.img_guid = leftAd.guid;
                    qScriptData.que_desc = "Uploaded Image: " + leftAd.image + " with Status: Delete";
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;//(from v in _dbContext.users_manages.Where(v => v.Login.Contains("admin")) select v.Email).SingleOrDefault();
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;
                    queuesDto.ImageGUID = qScriptData.img_guid;

                    isDelete = SaveImageDetails(leftAd, ref queuesDto, formMode);

                    return isDelete = true;
                }
                return isAdded;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        //Move to TackingQueueSerive and make it generic
        public bool SaveImageDetails(LeftAd leftAd, ref TrackingQueuesDto queuesDto, string mode)
        {
            tblquecmsimage qImageData;
            try
            {
                if (mode == "Delete")
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_id = leftAd.Id;
                    qImageData.img_guid = leftAd.guid;
                    qImageData.img_description = leftAd.image + " Status: " + leftAd.status;
                    qImageData.img_name = leftAd.image;
                    qImageData.img_uploadPath = _configuration.GetSection("AdsPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = leftAd.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "LeftAd";
                    qImageData.Action = "Delete";
                    qImageData.img_createdDate = DateTime.Now;
                    qImageData.img_updatedDate = DateTime.Now;

                    _dbContext.Add(qImageData);
                    _dbContext.SaveChanges();

                    //Bind value that need to be returned in ref object(TrackingQueuesDto)
                    queuesDto.ImageGUID = qImageData.img_guid;
                    queuesDto.ImageQueueId = qImageData.img_queId;
                    queuesDto.ImageUploadPath = qImageData.img_uploadPath;
                    queuesDto.ImageQueueStatus = qImageData.Status;
                    queuesDto.BannerType = qImageData.banner_type;

                    return true;
                }
                else
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_id = leftAd.Id;
                    qImageData.img_guid = leftAd.guid;
                    qImageData.img_description = leftAd.image + " Status: " + leftAd.status;
                    qImageData.img_name = leftAd.image;
                    qImageData.img_uploadPath = _configuration.GetSection("AdsPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = leftAd.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "LeftAd";
                    qImageData.Action = "Upload";
                    qImageData.img_createdDate = DateTime.Now;
                    qImageData.img_updatedDate = DateTime.Now;

                    _dbContext.Add(qImageData);
                    _dbContext.SaveChanges();

                    //Bind value that need to be returned in ref object(TrackingQueuesDto)
                    queuesDto.ImageQueueId = qImageData.img_queId;
                    queuesDto.ImageUploadPath = qImageData.img_uploadPath;
                    queuesDto.ImageQueueStatus = qImageData.Status;
                    queuesDto.BannerType = qImageData.banner_type;
                    queuesDto.ImageGUID = qImageData.img_guid;

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public IEnumerable<LeftAd> GetLeftAdsByGUID(Guid imageGUID)
        {
            try
            {
                if (imageGUID != Guid.Empty)
                    return _dbContext.ads_images.Where(x => x.guid == imageGUID).ToList();

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Changed status to delete of all records against given guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool UpdateLeftAdByGUID(Guid GUID)
        {
            bool isUpdated = false;
            List<LeftAd> leftAds = null;
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        leftAds = GetLeftAdsByGUID(GUID).ToList();

                        if (leftAds.Count > 0 && leftAds != null)
                        {
                            foreach (LeftAd ad in leftAds)
                            {
                                ad.status = "delete";
                                ad.date_updated = DateTime.Now;

                                _dbContext.Update(ad);
                                _dbContext.SaveChanges();
                            }

                            transaction.Commit();
                            return isUpdated = true;
                        }
                        return isUpdated = false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return isUpdated = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Add all the pages of a left ad image into Production's table.
        /// </summary>
        /// <param name="leftAds"></param>
        /// <returns></returns>
        public bool InsertLeftAdProd(List<LeftAd> leftAds)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        foreach (var item in leftAds)
                        {
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO ads_images (guid,language_id,title,image,page,url_key,view,last_user,status," +
                            "date_created,date_updated,ad_hyperlink) VALUES ('" + item.guid + "'," + item.language_id + ",'" + item.title.Replace("'",@"\'") + "','" + item.image + "','" + item.page + "','" +
                            item.url_key + "','" + item.view + "','" + item.last_user + "','" + item.status + "','" +
                            DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + item.ad_hyperlink + "')", con);

                            cmd.ExecuteNonQuery();
                        }

                        con.Close();

                        return true;
                    }
                    catch (Exception)
                    {
                        con.Close();
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
        /// Delete all the pages of a left ad by using guid and Add the newely selected image and pages for that guid in Production.
        /// </summary>
        /// <param name="leftAds"></param>
        /// <param name="GUID"></param>
        /// <returns>bool</returns>
        /// <exception cref="Exception"></exception>
        public bool UpdateLeftAdsProd(List<LeftAd> leftAds, Guid GUID)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        RemoveAllLeftAdsProdByGUID(GUID);
                        InsertLeftAdProd(leftAds);

                        _dbContext.SaveChanges();
                        transaction.Commit();
                        return true;
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
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Delete all the LeftAds from Production server for given guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public bool RemoveAllLeftAdsProdByGUID(Guid GUID)
        {
            bool isRemoved = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM ads_images WHERE guid = '" + GUID + "'", conn);

                        cmd.ExecuteNonQuery();

                        conn.Close();

                        return true;
                    }
                    catch (Exception)
                    {
                        conn.Close();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public LeftAd GetActiveLeftAdPage(string urlKey, string viewDevice, string language)
        {
            try
            {
                int languageID;
                LeftAd leftAd;

                if (language == "English")
                    languageID = 1;
                else
                    languageID = 2;

                if (!string.IsNullOrEmpty(urlKey))
                {
                    leftAd = _dbContext.ads_images.Where(x => x.url_key == urlKey && x.view == viewDevice && x.language_id == languageID && x.status != "delete" && x.status != "inactive").FirstOrDefault();
                    return leftAd;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool CheckFileOnProd(LeftAd model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM ads_images where guid = '" + model.guid + "';", con);
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
