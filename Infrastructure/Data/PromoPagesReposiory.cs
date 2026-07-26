using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using LinqKit;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Infrastructure.Data
{
    public class PromoPagesReposiory : IPromosRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public PromoPagesReposiory(CMSDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public bool Add(PromoPages Promo)
        {
            try
            {
                _dbContext.PromoPages.Add(Promo);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool Delete(PromoPages promo)
        {
            throw new NotImplementedException();
        }

        public PromoPages FindById(int id)
        {
            PromoPages promo = new PromoPages();
            if (id != 0)
            {
                promo = _dbContext.PromoPages.Where(x => x.promo_page_id == id).FirstOrDefault();
            }
            return promo;
        }

        public IEnumerable<PromoPages> GetAllPromos()
        {
            throw new NotImplementedException();
        }

        public List<PromoPages> GetPromoPagesList()
        {
            try
            {
                List<PromoPages> promoPages = (from q in _dbContext.PromoPages
                                               where q.status != "delete"
                                               select new PromoPages
                                               {
                                                   language_id = q.language_id,
                                                   title = q.title,
                                                   last_user = q.last_user,
                                                   status = q.status,
                                                   itemId = q.itemId,
                                                   guid = q.guid
                                               }).ToList();
                return promoPages;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }


        public bool SaveScriptAndData(string formMode, PromoPages promo, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO promo_pages(guid,language_id,ItemId,url_Key, title, date_expired, last_user, status, date_created, date_updated)" +
                        "VALUES('" + promo.guid + "'," + promo.language_id + "," + promo.itemId + ",'" + promo.url_Key + "','" + promo.title.Replace("'",@"\'") + "','" + promo.date_expired.ToString("yyyy-MM-dd H:mm:ss") + "','" + promo.last_user + "','" + promo.status + "','" +
                        Convert.ToDateTime(promo.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(promo.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "')";
                    qScriptData.img_id = promo.promo_page_id;
                    qScriptData.img_guid = promo.guid;
                    qScriptData.que_desc = "Uploaded Page: " + promo.title + " with Status : " + promo.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isAdded = SaveImageQueWithblank(promo,ref queuesDto,formMode);

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
                    var queryScript = "UPDATE promo_pages SET  title = '" + promo.title.Replace("'",@"\'") +
                        "', last_user = '" + promo.last_user +
                        "', status = '" + promo.status +
                        "', date_expired = '" + Convert.ToDateTime(promo.date_expired).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(promo.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + promo.guid + "' and language_id = " + promo.language_id + ";";

                    qScriptData.img_id = promo.promo_page_id;
                    qScriptData.que_desc = "Uploaded Page: " + promo.title + " with Status : " + promo.status;
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
                    isAdded = SaveImageQueWithblank(promo, ref queuesDto, formMode);

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
                    var queryScript = "UPDATE promo_pages SET title = '" + promo.title.Replace("'",@"\'") +
                        "', last_user = '" + promo.last_user +
                        "', status = 'delete', date_created = '" + Convert.ToDateTime(promo.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(promo.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + promo.guid + "' and language_id = " + promo.language_id + ";";

                    qScriptData.img_id = promo.promo_page_id;
                    qScriptData.que_desc = "Uploaded Page: " + promo.title + " with Status : " + promo.status;
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
                    isAdded = SaveImageQueWithblank(promo, ref queuesDto, formMode);
                    if (isAdded)
                    {
                        queuesDto.ScriptQueueId = qScriptData.que_id;
                        queuesDto.ScriptQueueStatus = qScriptData.Status;
                    }

                    return true;
                }
                return isAdded;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                return false;
            }
        }

        public bool SaveImageQueWithblank(PromoPages promo, ref TrackingQueuesDto queuesDto, string formMode)
        {
            var imgque = new tblquecmsimage();
            try
            {
                if(formMode == "Delete")
                {
                    
                    //imgque = new tblquecmsimage();
                    imgque.tblquecms_id = queuesDto.ScriptQueueId;
                    imgque.img_id = promo.promo_page_id;
                    imgque.img_guid = promo.guid;
                    imgque.img_description = promo.title + " Status: " + promo.status;
                    imgque.img_name = promo.title;
                    imgque.img_uploadPath = promo.title;
                    imgque.upload_date = DateTime.Now;
                    imgque.img_user = promo.last_user;
                    imgque.Status = "pending";
                    imgque.banner_type = "promoPage";
                    imgque.Action = "DeletePage";

                    imgque.img_createdDate = DateTime.Now;
                    imgque.img_updatedDate = DateTime.Now;

                    _dbContext.Add(imgque);
                    _dbContext.SaveChanges();

                    queuesDto.ImageQueueId = imgque.img_queId;
                    queuesDto.ImageUploadPath = imgque.img_uploadPath;
                    queuesDto.ImageQueueStatus = imgque.Status;
                    queuesDto.BannerType = imgque.banner_type;
                    //var imgsata = _context.Tblquecmsimage.Where(a => a.img_id == imgque.img_id && a.img_queId == imgque.img_queId).FirstOrDefault();
                    //if (imgsata != null)
                    //{
                    //    tblquecmsimageModel imcms = new tblquecmsimageModel(imgsata);
                    //    idImgdata = imcms;
                    //}

                    return true;
                }
                else
                {
                    //var imgque = new tblquecmsimage();
                    //imgque = new tblquecmsimage();
                    imgque.tblquecms_id = queuesDto.ScriptQueueId;
                    imgque.img_id = promo.promo_page_id;
                    imgque.img_guid = promo.guid;
                    imgque.img_description = promo.title + " Status: " + promo.status;
                    imgque.img_name = promo.title;
                    imgque.img_uploadPath = promo.title;
                    imgque.upload_date = DateTime.Now;
                    imgque.img_user = promo.last_user;
                    imgque.Status = "pending";
                    imgque.banner_type = "promoPage";
                    imgque.Action = "UploadPage";

                    imgque.img_createdDate = DateTime.Now;
                    imgque.img_updatedDate = DateTime.Now;

                    _dbContext.Add(imgque);
                    _dbContext.SaveChanges();

                    queuesDto.ImageQueueId = imgque.img_queId;
                    queuesDto.ImageUploadPath = imgque.img_uploadPath;
                    queuesDto.ImageQueueStatus = imgque.Status;
                    queuesDto.BannerType = imgque.banner_type;
                    //var imgsata = _context.Tblquecmsimage.Where(a => a.img_id == imgque.img_id && a.img_queId == imgque.img_queId).FirstOrDefault();
                    //if (imgsata != null)
                    //{
                    //    tblquecmsimageModel imcms = new tblquecmsimageModel(imgsata);
                    //    idImgdata = imcms;
                    //}

                    return true;
                }
                
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool Update(PromoPages promo, string mode)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "Delete")
                        {
                            PromoPages pg = FindById(promo.promo_page_id);
                            if (pg != null)
                            {
                                promo.status = "delete";
                                promo.date_created = pg.date_created;
                                promo.date_updated = DateTime.Now;

                                _dbContext.Entry(pg).CurrentValues.SetValues(promo);
                                _dbContext.SaveChanges();
                                transaction.Commit();

                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            PromoPages pg = FindById(promo.promo_page_id);
                            if (pg != null)
                            {
                                promo.date_created = pg.date_created;
                                promo.date_updated = DateTime.Now;

                                _dbContext.Entry(pg).CurrentValues.SetValues(promo);
                                _dbContext.SaveChanges();
                                transaction.Commit();

                                return true;
                            }
                            return false;
                        }
                    }
                    catch (Exception ex)
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

        public int MaxItemId()
        {
            try
            {
                int maxAge = 0;
                if (_dbContext.PromoPages.Count() > 0)
                {
                    maxAge = _dbContext.PromoPages.Max(p => p.itemId);
                }
                return maxAge;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public List<PromoPages> FindByItemId(int itemID)
        {
            try
            {
                return _dbContext.PromoPages.Where(p => p.itemId == itemID).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public List<PromoPages> TitleExists(Guid guid, string EngTitle, string FrTitle)
        {
            try
            {
                if (guid == Guid.Empty)
                {
                    return _dbContext.PromoPages.Where(p => (p.title == EngTitle || p.title == FrTitle) && p.status != "delete").ToList();
                }
                else
                {
                    return _dbContext.PromoPages.Where(p => (p.title == EngTitle || p.title == FrTitle) && p.guid != guid && p.status != "delete").ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public List<PromoPages> FindByGuidID(Guid guidId)
        {
            try
            {
                return _dbContext.PromoPages.Where(p => p.guid == guidId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool InsertPromoProd(PromoPages promo)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO promo_pages (language_id,guid,ItemId,url_Key,date_expired,title,last_user,status," +
                        "date_created,date_updated) VALUES (" + promo.language_id + ",'" + promo.guid + "'," + promo.itemId + ",'" + promo.url_Key + "','" + promo.date_expired.ToString("yyyy-MM-dd H:mm:ss") + "','" + promo.title.Replace("'",@"\'") + "','" + promo.last_user + "','" + promo.status + "','" +
                        DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "')", con);

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

        public bool UpdatePromoProd(int language, Guid guid, PromoPages promo)
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
                        MySqlCommand cmd = new MySqlCommand("SELECT * FROM promo_pages WHERE language_id = " + language + " and guid = '" + guid + "'", conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            currentVal = Convert.ToInt32(reader["promo_page_id"]);
                        }
                        reader.Close();

                        if (currentVal != null) // && currentVal == imageId
                        {
                            string query = "UPDATE promo_pages SET title = '" + promo.title.Replace("'",@"\'") + "', date_expired = '" + promo.date_expired.ToString("yyyy-MM-dd H:mm:ss") + "', status = '" +
                            promo.status + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                            "' WHERE language_id = " + language + " and guid = '" + guid + "'; ";

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

        public PromoPages FindByLangGuid(Guid guidId)
        {
            try
            {
                return _dbContext.PromoPages.Where(p => p.guid == guidId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool CheckFileOnProd(PromoPages model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT guid FROM promo_pages where guid = '" + model.guid + "';", con);
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
