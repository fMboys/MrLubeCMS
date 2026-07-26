using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using LinqKit;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Reflection;

namespace CMS.Infrastructure.Data
{
    public class FloatingImageRepository : IFloatingImageRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IApplicationRepository _appRepo;

        public FloatingImageRepository(CMSDbContext dbContext, IConfiguration configuration, IApplicationRepository appRepo)
        {
            _dbContext = dbContext;
            _appRepo = appRepo;
            _configuration = configuration;
        }

        public TrackingQueuesDto Add(string selectedPages, FloatingImage floatingImage)
        {
            bool isAdded = false;
            FloatingImage floatingImageData = null;
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
                            foreach(var id in subMenuIDs)
                            {
                                SubMenu? subMenu = new SubMenu();
                                Menu menu = new Menu();
                                int subMenuID = Convert.ToInt32(id.Trim());
                                subMenu = _dbContext.sub_menu.Find(subMenuID);
                                string menuPage = "";
                                
                                if (subMenu != null)
                                {
                                    subMenu = _dbContext.sub_menu.Where(x => x.item_id == subMenu.item_id && x.language_id == floatingImage.language_id).FirstOrDefault(); //To Get UrlKey for selected language.
                                    if(subMenu.item_id == 35 ) {
                                        menu.title = "Others";
                                    }
                                    else if(subMenu.item_id == 36)
                                    {
                                        menu.title = "Services";
                                    }
                                    else
                                    {
                                        menu = _dbContext.menu.Where(x => (x.menu_id == subMenu.menu_id || subMenu.item_id == 35) && x.language_id == floatingImage.language_id).FirstOrDefault();//Todo: replace with a method create in apprepo
                                    }
                                    

                                    //Change the Tires Menu into Services
                                    if (menu.title == "Tires" && floatingImage.language_id == 1)
                                    {
                                        menuPage = "Services";
                                    }
                                    else if (menu.title == "Pneus" && floatingImage.language_id == 2)
                                    {
                                        menuPage = "Services";
                                    }
                                    else if (menu.title == "Autre" && floatingImage.language_id == 2)
                                    {
                                        menuPage = "Others";
                                    }
                                    else if (menu.title == "Other" && floatingImage.language_id == 1)
                                    {
                                        menuPage = "Others";
                                    }
                                    else if ( subMenu.item_id == 35)
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
                                }

                                if (menu != null)
                                {
                                    floatingImageData = new FloatingImage()
                                    {
                                        guid = floatingImage.guid,
                                        language_id = floatingImage.language_id,
                                        title = floatingImage.title,
                                        image = floatingImage.image,
                                        page = menuPage,
                                        url_key = subMenu.url_key,
                                        view = floatingImage.view,
                                        last_user = floatingImage.last_user,
                                        status = floatingImage.status,
                                        date_created = floatingImage.date_created,
                                        date_updated = floatingImage.date_updated,
                                        ad_hyperlink = floatingImage.ad_hyperlink
                                    };

                                    _dbContext.Add(floatingImageData);
                                    _dbContext.SaveChanges();
                                }

                                //add script to queue
                                SaveScriptAndData("Create", floatingImageData, ref queuesDto, floatingImage.last_user);

                            }

                            SaveImageDetails(floatingImageData, ref queuesDto, string.Empty);

                            transaction.Commit();
                            queuesDto.FloatingImageID = floatingImageData.Id;
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

        public TrackingQueuesDto Update(string selectedPages, FloatingImage floatingImageData)
        {
            FloatingImage floatingImage = null;
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
                            List<FloatingImage> floatingImages = GetFloatingImagesByGUID(floatingImageData.guid).ToList();
                            _dbContext.floating_images.RemoveRange(floatingImages);

                            _appRepo.RemoveQueueImageByGuid(floatingImageData.guid);
                            _appRepo.RemoveAllQueueScriptsByGuid(floatingImageData.guid);

                            foreach (var id in subMenuIDs)
                            {
                                SubMenu? subMenu = new SubMenu();
                                Menu menu = new Menu();
                                int subMenuID = Convert.ToInt32(id.Trim());
                                subMenu = _dbContext.sub_menu.Find(subMenuID);
                                string menuPage = "";

                                if (subMenu != null)
                                {
                                    subMenu = _dbContext.sub_menu.Where(x => x.item_id == subMenu.item_id && x.language_id == floatingImageData.language_id).FirstOrDefault();

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
                                        menu = _dbContext.menu.Where(x => (x.menu_id == subMenu.menu_id || subMenu.item_id == 35) && x.language_id == floatingImageData.language_id).FirstOrDefault();//Todo: replace with a method create in apprepo
                                    }

                                    //menu = _dbContext.menu.Where(x => x.menu_id == subMenu.menu_id && x.language_id == floatingImageData.language_id).FirstOrDefault();
                                    
                                    //Change the Tires Menu into Services
                                    if (menu.title == "Tires" && floatingImageData.language_id == 1)
                                    {
                                        menuPage = "Services";
                                    }
                                    else if (menu.title == "Pneus" && floatingImageData.language_id == 2)
                                    {
                                        menuPage = "Services";
                                    }
                                    else if (menu.title == "Autre" && floatingImageData.language_id == 2)
                                    {
                                        menuPage = "Others";
                                    }
                                    else if (menu.title == "Other" && floatingImageData.language_id == 1)
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
                                }                                    

                                if (menu != null)
                                {                                    
                                    floatingImage = new FloatingImage()
                                    {
                                        guid = floatingImageData.guid,
                                        language_id = floatingImageData.language_id,
                                        title = floatingImageData.title,
                                        image = floatingImageData.image,
                                        page = menuPage,
                                        url_key = subMenu.url_key,
                                        view = floatingImageData.view,
                                        last_user = floatingImageData.last_user,
                                        status = floatingImageData.status,
                                        date_created = DateTime.Now,
                                        date_updated = DateTime.Now,
                                        ad_hyperlink = floatingImageData.ad_hyperlink
                                    };

                                    _dbContext.Add(floatingImage);
                                    _dbContext.SaveChanges();

                                    SaveScriptAndData("Create", floatingImage, ref queuesDto, floatingImageData.last_user);
                                }
                            }

                            SaveImageDetails(floatingImageData, ref queuesDto, string.Empty);

                            transaction.Commit();
                            return queuesDto;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            //return queuesDto;
                            throw new Exception(ex.ToString());
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
        /// Get a list of all the data of floating images from database.
        /// </summary>
        /// <returns>IEnumerable of FloatingImage</returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<FloatingImage> GetAllFloatingImages()
        {
            IEnumerable<FloatingImage> floatingImages = null;
            try
            {
                floatingImages = _dbContext.floating_images.Where(x => x.status != "delete").ToList().DistinctBy(x => x.guid).OrderBy(x => x.date_updated);
                return floatingImages;
                //List<FloatingImage> floatingImages = (List<FloatingImage>)_dbContext.floating_images.ToList();

                //return floatingImages;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Retrieve all the pages that have active floating images.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<FloatingImage> GetAllFloatingImagePages(int lang,string view)
        {
            IEnumerable<FloatingImage> floatingImages = null;
            try
            {
                floatingImages = _dbContext.floating_images.Where(x => x.image != "" && x.url_key != "" && x.status != "delete" && x.status != "inactive" && x.language_id == lang && x.view == view).ToList();
                return floatingImages;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public IEnumerable<FloatingImage> GetFloatingImagesByGUID(Guid imageGUID)
        {
            try
            {
                if (imageGUID != Guid.Empty)
                    return _dbContext.floating_images.Where(x => x.guid == imageGUID).ToList();

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public FloatingImage GetActiveFloatingImagePage(string urlKey, string viewDevice, string language)
        {
            try
            {
                int languageID;
                FloatingImage floatingImage;

                if (language == "English")
                    languageID = 1;
                else
                    languageID = 2;

                if (!string.IsNullOrEmpty(urlKey))
                {
                    floatingImage = _dbContext.floating_images.Where(x => x.url_key == urlKey && x.view == viewDevice && x.language_id == languageID && x.status != "delete" && x.status != "inactive").FirstOrDefault();
                    return floatingImage;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public List<FloatingImage> GetFloatingImageList(FloatingImage floatingImage)
        {
            try
            {
                var predicate = PredicateBuilder.True<FloatingImage>();
                
                if (!string.IsNullOrEmpty(floatingImage.title)) { predicate.And(x => x.title.Equals(floatingImage.title)); }
                if (!string.IsNullOrEmpty(floatingImage.image)) { predicate.And(x => x.image.Equals(floatingImage.image)); }
                if (!string.IsNullOrEmpty(floatingImage.view)) { predicate.And(x => x.view.Equals(floatingImage.view)); }
                if (!string.IsNullOrEmpty(floatingImage.status)) { predicate.And(x => x.status.Equals(floatingImage.status)); }
                if (!string.IsNullOrEmpty(floatingImage.ad_hyperlink)) { predicate.And(x => x.ad_hyperlink.Equals(floatingImage.ad_hyperlink)); }

                List<FloatingImage> floatingImageList = (from q in _dbContext.floating_images.Where(predicate)
                                            //where q.status != "delete"
                                            select new FloatingImage
                                            {
                                                Id = q.Id,
                                                language_id = q.language_id,
                                                title = q.title,
                                                image = q.image,
                                                page = q.page,
                                                url_key = q.url_key,
                                                view = q.view,
                                                last_user = q.last_user,
                                                status = q.status,
                                                ad_hyperlink = q.ad_hyperlink
                                            }).ToList();
                return floatingImageList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        /// <summary>
        /// Get a floating image data based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        public FloatingImage FindByGuid(Guid guid)
        {
            FloatingImage? floatingImage = null;
            try
            {
                if (guid != Guid.Empty)
                {
                    floatingImage = _dbContext.floating_images.Where(x => x.guid == guid).FirstOrDefault();
                }
                return floatingImage;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        /// <summary>
        /// Get a floating image data based on ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        public FloatingImage FindByID(int id)
        {
            FloatingImage? floatingImage = null;
            try
            {
                if (id > 0)
                {
                    floatingImage = _dbContext.floating_images.Find(id);
                }
                return floatingImage;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool SaveScriptAndData(string formMode, FloatingImage floatingImage, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO floating_images(guid, language_id, title, image, page, url_key, view, last_user, status, date_created, date_updated, ad_hyperlink)" +
                        "VALUES('" + floatingImage.guid + "'," + floatingImage.language_id + ",'" + floatingImage.title.Replace("'",@"\'") + "','" + floatingImage.image + "','" + floatingImage.page + "','" + floatingImage.url_key + "','" + floatingImage.view + "','" +
                        floatingImage.last_user + "','" + floatingImage.status + "','" + Convert.ToDateTime(floatingImage.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(floatingImage.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "','" + floatingImage.ad_hyperlink + "')";
                    qScriptData.img_id = floatingImage.Id;
                    qScriptData.img_guid = floatingImage.guid;
                    qScriptData.que_desc = "Uploaded Image: " + floatingImage.image + " with Status: " + floatingImage.status;
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
                    var queryScript = "UPDATE floating_images SET language_id = " + floatingImage.language_id + ", title = '" + floatingImage.title.Replace("'",@"\'") + "', image = '" + floatingImage.image + "', page = '" + floatingImage.page + 
                        "', view = '" + floatingImage.view + "', url_key = '" + floatingImage.url_key + "', last_user = '" + floatingImage.last_user +
                        "', status = '" + floatingImage.status + "', date_created = '" + Convert.ToDateTime(floatingImage.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(floatingImage.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', ad_hyperlink = '" + floatingImage.ad_hyperlink + "' WHERE shopTire_id = " + floatingImage.Id + ";";

                    qScriptData.img_id = floatingImage.Id;
                    qScriptData.img_guid = floatingImage.guid;
                    qScriptData.que_desc = "Uploaded Image: " + floatingImage.image + " with Status: " + floatingImage.status;
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

                    isAdded = SaveImageDetails(floatingImage, ref queuesDto, formMode);

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
                    //var queryScript = "UPDATE floating_images SET language_id = " + floatingImage.language_id + ", title = '" + floatingImage.title + "', image = '" + floatingImage.image + "', page = '" + floatingImage.page +
                    //    "', view = '" + floatingImage.view + "', url_key = '" + floatingImage.url_key + "', last_user = '" + floatingImage.last_user +
                    //    "', status = 'delete', date_created = '" + Convert.ToDateTime(floatingImage.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                    //    "', date_updated = '" + Convert.ToDateTime(floatingImage.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', ad_hyperlink = '" + floatingImage.ad_hyperlink + "' WHERE guid = " + floatingImage.guid + ";";

                    var queryScript = "UPDATE floating_images SET status = 'delete', date_updated = '" + Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + floatingImage.guid + "';";
                    qScriptData.img_id = floatingImage.Id;
                    qScriptData.img_guid = floatingImage.guid;
                    qScriptData.que_desc = "Uploaded Image: " + floatingImage.image + " with Status: Delete";
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

                    isDelete = SaveImageDetails(floatingImage, ref queuesDto, formMode);

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
        public bool SaveImageDetails(FloatingImage floatingImage, ref TrackingQueuesDto queuesDto, string mode)
        {
            tblquecmsimage qImageData;
            try
            {
                if (mode == "Delete")
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_id = floatingImage.Id;
                    qImageData.img_guid = floatingImage.guid;
                    qImageData.img_description = floatingImage.image + " Status: " + floatingImage.status;
                    qImageData.img_name = floatingImage.image;
                    qImageData.img_uploadPath = _configuration.GetSection("FloatingPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = floatingImage.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "FloatingImage";
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
                    qImageData.img_id = floatingImage.Id;
                    qImageData.img_guid = floatingImage.guid;
                    qImageData.img_description = floatingImage.image + " Status: " + floatingImage.status;
                    qImageData.img_name = floatingImage.image;
                    qImageData.img_uploadPath = _configuration.GetSection("FloatingPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = floatingImage.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "FloatingImage";
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

        /// <summary>
        /// Add all the pages of a floating image into Production's table.
        /// </summary>
        /// <param name="floatingImages"></param>
        /// <returns></returns>
        public bool InsertFloatingImageProd(List<FloatingImage> floatingImages)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        foreach (var item in floatingImages)
                        {
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO floating_images (guid,language_id,title,image,page,url_key,view,last_user,status," +
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
        /// Delete all the pages of a floating image by using guid and Add the newely selected image and pages for that guid in Production.
        /// </summary>
        /// <param name="floatingImages"></param>
        /// <param name="GUID"></param>
        /// <returns>bool</returns>
        /// <exception cref="Exception"></exception>
        public bool UpdateFloatingImageProd(List<FloatingImage> floatingImages, Guid GUID)
        {
            try
            {
                using(var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {                     
                        RemoveAllFloatingImageProdByGUID(GUID);
                        InsertFloatingImageProd(floatingImages);

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
        /// Changed status to delete of all records against given guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool UpdateFloatingImageByGUID(Guid GUID)
        {
            bool isUpdated = false;
            List<FloatingImage> floatingImages = null;
            try
            {
                using(var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        floatingImages = GetFloatingImagesByGUID(GUID).ToList();

                        if (floatingImages.Count > 0 && floatingImages != null)
                        {
                            foreach(FloatingImage image in floatingImages)
                            {
                                image.status = "delete";
                                image.date_updated = DateTime.Now;
                                
                                _dbContext.Update(image);
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
        /// Delete all the floating images from Production server using guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public bool RemoveAllFloatingImageProdByGUID(Guid GUID)
        {
            bool isRemoved = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM floating_images WHERE guid = '" + GUID + "'", conn);

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

        public bool CheckFileOnProd(FloatingImage model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM floating_images where guid = '" + model.guid + "';", con);
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
