using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

namespace CMS.Infrastructure.Data
{
    public class PromoImagesRepository : IPromoImagesRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PromoImagesRepository(CMSDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public IEnumerable<PromoImages> GetAllPromoImages()
        {
            try
            {
                return _dbContext.PromoImages.ToList().OrderBy(x => x.date_updated);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public PromoImages FindById(int id)
        {
            PromoImages promo = new PromoImages();
            if (id != 0)
            {
                promo = _dbContext.PromoImages.Where(x => x.promo_image_id == id).FirstOrDefault();
            }
            return promo;
        }

        public PromoImages FindByGuidID(Guid guidId)
        {
            try
            {
                return _dbContext.PromoImages.Where(p => p.guid == guidId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public List<PromoImages> GetPromoImageList()
        {
            try
            {
                List<PromoImages> promo = (from q in _dbContext.PromoImages//.Where(predicate)
                                           where q.status != "delete"
                                           select new PromoImages
                                           {
                                               guid = q.guid,
                                               promo_image_id = q.promo_image_id,
                                               language_id = q.language_id,
                                               title = q.title,
                                               image = q.image,
                                               url_key = q.url_key,
                                               view = q.view,
                                               last_user = q.last_user,
                                               status = q.status,
                                               promo_hyperlink = q.promo_hyperlink
                                           }).ToList();
                foreach(var item in promo)
                {
                    PromoPages promoPage = _dbContext.PromoPages.Where(x => x.url_Key == item.url_key).FirstOrDefault();
                    
                    if (promoPage != null)
                    {
                        item.page = promoPage.title;
                    }
                }

                return promo;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool Add(PromoImages promo)
        {
            try
            {
                _dbContext.PromoImages.Add(promo);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public bool IsAlreadyExists(PromoImages promo)
        {
            try
            {
                if (promo.promo_image_id == 0)
                {
                    if (_dbContext.PromoImages.Where(x => x.url_key == promo.url_key
                                                        && x.view == promo.view
                                                        && x.language_id == promo.language_id
                                                        && x.status == "active").ToList().Count() > 0)
                    {
                        return true;
                    } 
                }
                else
                {

                    if (_dbContext.PromoImages.Where(x => x.url_key == promo.url_key
                                                        && x.view == promo.view
                                                        && x.language_id == promo.language_id
                                                        && x.status == "active"
                                                        && x.promo_image_id != promo.promo_image_id).ToList().Count() > 0)
                    {
                        return true;
                    } 
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool InsertPromoImageProd(PromoImages promo)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO promo_images (guid,language_id,title,image,url_key,view,last_user,status," +
                        "date_created,date_updated,promo_hyperlink) VALUES ('" + promo.guid + "'," + promo.language_id + ",'" +
                        promo.title.Replace("'",@"\'") + "','" + promo.image + "','" + promo.url_key + "','" + promo.view + "','" + promo.last_user + "','" + promo.status + "','" +
                        DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + promo.promo_hyperlink + "')", con);

                        cmd.ExecuteNonQuery();
                        con.Close();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return false;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool UpdatePromoImageProd(Guid imageId, PromoImages promo)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        conn.Open();

                        //cheking the banner id
                        int? currentVal = null;
                        MySqlCommand cmd = new MySqlCommand("SELECT * FROM promo_images WHERE guid = '" + imageId + "'", conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            currentVal = Convert.ToInt32(reader["promo_image_id"]);
                        }
                        reader.Close();

                        if (currentVal != null)//&& currentVal.Equals(imageId)
                        {
                            string query = "UPDATE promo_images SET language_id = " + promo.language_id +
                            ", title = '" + promo.title.Replace("'",@"\'") + "', image = '" + promo.image + "',url_key = '" + promo.url_key + "', status = '" +
                            promo.status + "', date_created = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                            "', promo_hyperlink = '" + promo.promo_hyperlink + "' WHERE guid = '" + imageId + "'; ";

                            MySqlCommand sqlCommand = new MySqlCommand(query, conn);

                            sqlCommand.ExecuteNonQuery();
                            conn.Close();

                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        conn.Close();
                        return false;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool Update(PromoImages PromoImageData, string mode)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "Delete")
                        {
                            PromoImages promo = FindByGuidID(PromoImageData.guid);
                            if (promo != null)
                            {
                                PromoImageData.status = "delete";
                                PromoImageData.date_created = promo.date_created;
                                PromoImageData.date_updated = DateTime.Now;

                                _dbContext.Entry(promo).CurrentValues.SetValues(PromoImageData);
                                _dbContext.SaveChanges();
                                transaction.Commit();

                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            PromoImages promo = FindByGuidID(PromoImageData.guid);
                            if (promo != null)
                            {
                                PromoImageData.date_created = promo.date_created;
                                PromoImageData.date_updated = DateTime.Now;

                                _dbContext.Entry(promo).CurrentValues.SetValues(PromoImageData);
                                _dbContext.SaveChanges();
                                transaction.Commit();

                                return true;
                            }
                            return false;
                        }

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

        public bool Delete(PromoImages PromoImageData)
        {
            try
            {
                if (PromoImageData != null)
                {
                    _dbContext.Attach(PromoImageData);
                    _dbContext.PromoImages.Remove(PromoImageData);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }
        //TODO: create a generic version
        public bool SaveScriptAndData(string formMode, PromoImages promo, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO promo_images(language_id,guid, title, image, url_key, view, last_user, status, date_created, date_updated, promo_hyperlink)" +
                        "VALUES(" + promo.language_id + ",'" + promo.guid + "','" + promo.title.Replace("'",@"\'") + "','" + promo.image + "','" + promo.url_key + "','" + promo.view + "','" +
                        promo.last_user + "','" + promo.status + "','" + Convert.ToDateTime(promo.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(promo.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "','" + promo.promo_hyperlink + "')";
                    qScriptData.img_id = promo.promo_image_id;
                    qScriptData.que_desc = "Uploaded Image: " + promo.image + " with Status: " + promo.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    qScriptData.img_guid = promo.guid;
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isAdded = SaveImageDetails(promo, ref queuesDto, formMode);
                    //Bind value that need to be returned in ref object(TrackingQueuesDto)
                    if (isAdded)
                    {
                        queuesDto.ScriptQueueId = qScriptData.que_id;
                        queuesDto.ScriptQueueStatus = qScriptData.Status;
                    }

                    return isAdded;
                }
                else if (formMode == "Edit")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "UPDATE promo_images SET language_id = " + promo.language_id + ", title = '" + promo.title.Replace("'",@"\'") +
                        "', image = '" + promo.image + "', url_key = '" + promo.url_key + "', view = '" + promo.view + "', last_user = '" + promo.last_user +
                        "', status = '" + promo.status + "', date_created = '" + Convert.ToDateTime(promo.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(promo.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', promo_hyperlink = '" + promo.promo_hyperlink + "' WHERE guid = '" + promo.guid + "';";

                    qScriptData.img_id = promo.promo_image_id;
                    qScriptData.que_desc = "Uploaded Image: " + promo.image + " with Status: " + promo.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    qScriptData.img_guid = promo.guid;
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isAdded = SaveImageDetails(promo, ref queuesDto, formMode);

                    if (isAdded)
                    {
                        queuesDto.ScriptQueueId = qScriptData.que_id;
                        queuesDto.ScriptQueueStatus = qScriptData.Status;
                    }

                    return isAdded;
                }
                else if (formMode == "Delete")
                {
                    bool isDelete;

                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "UPDATE promo_images SET language_id = " + promo.language_id + ", title = '" + promo.title.Replace("'",@"\'") +
                        "', image = '" + promo.image + "', url_key = '" + promo.url_key + "', view = '" + promo.view + "', last_user = '" + promo.last_user +
                        "', status = 'delete', date_created = '" + Convert.ToDateTime(promo.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(promo.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', promo_hyperlink = '" + promo.promo_hyperlink + "' WHERE guid = '" + promo.guid + "';";
                    qScriptData.img_guid = promo.guid;
                    qScriptData.img_id = promo.promo_image_id;
                    qScriptData.que_desc = "Uploaded Image: " + promo.image + " with Status: Delete";
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isDelete = SaveImageDetails(promo, ref queuesDto, formMode);

                    return isDelete;
                }
                return isAdded;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }

        //Move to TackingQueueSerive and make it generic
        public bool SaveImageDetails(PromoImages promo, ref TrackingQueuesDto queuesDto, string mode)
        {
            tblquecmsimage qImageData;
            try
            {
                if (mode == "Delete")
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_guid = promo.guid;
                    qImageData.img_id = promo.promo_image_id;
                    qImageData.img_description = promo.image + " Status: " + promo.status;
                    qImageData.img_name = promo.image;
                    qImageData.img_uploadPath = _configuration.GetSection("PromoImagesPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = promo.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "promo";
                    qImageData.Action = "Delete";
                    qImageData.img_createdDate = DateTime.Now;
                    qImageData.img_updatedDate = DateTime.Now;

                    _dbContext.Add(qImageData);
                    _dbContext.SaveChanges();

                    //Bind value that need to be returned in ref object(TrackingQueuesDto)
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
                    qImageData.img_guid = promo.guid;
                    qImageData.img_id = promo.promo_image_id;
                    qImageData.img_description = promo.image + " Status: " + promo.status;
                    qImageData.img_name = promo.image;
                    qImageData.img_uploadPath = _configuration.GetSection("PromoImagesPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = promo.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "promo";
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

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool DeleteImageDetails(PromoImages promo)
        {
            List<tblquecmsimage> qImageData = new List<tblquecmsimage>();
            try
            {
                qImageData = _dbContext.Tblquecmsimage.Where(x => x.img_guid == promo.guid && x.banner_type == "promo").ToList();

                if (qImageData != null)
                {
                    _dbContext.Attach(qImageData);
                    //_dbContext.Entry(qImageData).State = EntityState.Deleted;
                    _dbContext.Tblquecmsimage.RemoveRange(qImageData);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }


        public List<PromoPages> ddlPromoPages()
        {
            try
            {
                return _dbContext.PromoPages.Where(p => p.language_id == 1 && p.status != "delete").ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool CheckFileOnProd(PromoImages model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT guid FROM promo_images where guid = '" + model.guid + "';", con);
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
