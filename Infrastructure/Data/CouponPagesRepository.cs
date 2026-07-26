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
    public class CouponPagesRepository : ICouponsRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public CouponPagesRepository(CMSDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public bool Add(CouponPages Coupon)
        {
            try
            {
                _dbContext.CouponPages.Add(Coupon);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool Delete(CouponPages coupon)
        {
            throw new NotImplementedException();
        }

        public CouponPages FindById(int id)
        {
            CouponPages coupon = new CouponPages();
            if (id != 0)
            {
                coupon = _dbContext.CouponPages.Where(x => x.coupon_page_id == id).FirstOrDefault();
            }
            return coupon;
        }

        public IEnumerable<CouponPages> GetAllCoupons()
        {
            throw new NotImplementedException();
        }

        public List<CouponPages> GetCouponPagesList()
        {
            try
            {
                List<CouponPages> couponPages = (from q in _dbContext.CouponPages
                                               where q.status != "delete"
                                               select new CouponPages
                                               {
                                                   language_id = q.language_id,
                                                   title = q.title,
                                                   last_user = q.last_user,
                                                   status = q.status,
                                                   itemId = q.itemId,
                                                   guid = q.guid
                                               }).ToList();
                return couponPages;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }


        public bool SaveScriptAndData(string formMode, CouponPages coupon, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO coupon_pages(guid,language_id, ItemId,url_Key, title, date_expired, last_user, status, date_created, date_updated)" +
                        "VALUES('" + coupon.guid + "'," + coupon.language_id + "," + coupon.itemId + ",'" + coupon.url_Key + "','" + coupon.title.Replace("'",@"\'") + "','" + coupon.date_expired.ToString("yyyy-MM-dd H:mm:ss") + "','" + coupon.last_user + "','" + coupon.status + "','" +
                        Convert.ToDateTime(coupon.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(coupon.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "')";
                    qScriptData.img_id = coupon.coupon_page_id;
                    qScriptData.que_desc = "Uploaded Page: " + coupon.title + " with Status : " + coupon.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    qScriptData.img_guid = coupon.guid;
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isAdded = SaveImageQueWithblank(coupon, ref queuesDto, formMode);
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
                    var queryScript = "UPDATE coupon_pages SET  title = '" + coupon.title.Replace("'",@"\'") +
                        "', last_user = '" + coupon.last_user +
                        "', status = '" + coupon.status +
                        "', date_expired = '" + Convert.ToDateTime(coupon.date_expired).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(coupon.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + coupon.guid + "' and language_id = " + coupon.language_id + ";";

                    qScriptData.img_id = coupon.coupon_page_id;
                    qScriptData.que_desc = "Uploaded Page: " + coupon.title + " with Status : " + coupon.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    qScriptData.img_guid = coupon.guid;
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;
                    isAdded = SaveImageQueWithblank(coupon, ref queuesDto, formMode);

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
                    var queryScript = "UPDATE coupon_pages SET title = '" + coupon.title.Replace("'", @"\'") +
                        "', last_user = '" + coupon.last_user +
                        "', status = 'delete', date_created = '" + Convert.ToDateTime(coupon.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(coupon.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + coupon.guid + "' and language_id = " + coupon.language_id + ";";

                    qScriptData.img_id = coupon.coupon_page_id;
                    qScriptData.que_desc = "Uploaded Page: " + coupon.title + " with Status : " + coupon.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    qScriptData.img_guid = coupon.guid;
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;
                    isAdded = SaveImageQueWithblank(coupon, ref queuesDto, formMode);
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

        public bool SaveImageQueWithblank(CouponPages Couponpromo, ref TrackingQueuesDto queuesDto, string formMode)
        {
            var imgque = new tblquecmsimage();
            try
            {
                if (formMode == "Delete")
                {

                    //imgque = new tblquecmsimage();
                    imgque.tblquecms_id = queuesDto.ScriptQueueId;
                    imgque.img_id = Couponpromo.coupon_page_id;
                    imgque.img_guid = Couponpromo.guid;
                    imgque.img_description = Couponpromo.title + " Status: " + Couponpromo.status;
                    imgque.img_name = Couponpromo.title;
                    imgque.img_uploadPath = Couponpromo.title;
                    imgque.upload_date = DateTime.Now;
                    imgque.img_user = Couponpromo.last_user;
                    imgque.Status = "pending";
                    imgque.banner_type = "couponPage";
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
                    imgque.img_id = Couponpromo.coupon_page_id;
                    imgque.img_guid = Couponpromo.guid;
                    imgque.img_description = Couponpromo.title + " Status: " + Couponpromo.status;
                    imgque.img_name = Couponpromo.title;
                    imgque.img_uploadPath = Couponpromo.title;
                    imgque.upload_date = DateTime.Now;
                    imgque.img_user = Couponpromo.last_user;
                    imgque.Status = "pending";
                    imgque.banner_type = "couponPage";
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

        public bool Update(CouponPages coupon, string mode)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "Delete")
                        {
                            CouponPages pg = FindById(coupon.coupon_page_id);
                            if (pg != null)
                            {
                                coupon.status = "delete";
                                coupon.date_created = pg.date_created;
                                coupon.date_updated = DateTime.Now;

                                _dbContext.Entry(pg).CurrentValues.SetValues(coupon);
                                _dbContext.SaveChanges();
                                transaction.Commit();

                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            CouponPages pg = FindById(coupon.coupon_page_id);
                            if (pg != null)
                            {
                                coupon.date_created = pg.date_created;
                                coupon.date_updated = DateTime.Now;

                                _dbContext.Entry(pg).CurrentValues.SetValues(coupon);
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
                if (_dbContext.CouponPages.Count() > 0)
                {
                    maxAge = _dbContext.CouponPages.Max(p => p.itemId);
                }
                return maxAge;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public List<CouponPages> FindByItemId(int itemID)
        {
            try
            {
                return _dbContext.CouponPages.Where(p => p.itemId == itemID).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public List<CouponPages> TitleExists(Guid guid, string EngTitle, string FrTitle)
        {
            try
            {
                if (guid == Guid.Empty)
                {
                    return _dbContext.CouponPages.Where(p => (p.title == EngTitle || p.title == FrTitle) && p.status != "delete").ToList();
                }
                else
                {
                    return _dbContext.CouponPages.Where(p => (p.title == EngTitle || p.title == FrTitle) && p.guid != guid && p.status != "delete").ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
        public List<CouponPages> FindByGuidID(Guid guidId)
        {
            try
            {
                return _dbContext.CouponPages.Where(p => p.guid == guidId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool InsertCouponProd(CouponPages coupon)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO coupon_pages (language_id,guid,ItemId,url_Key,date_expired,title,last_user,status," +
                        "date_created,date_updated) VALUES (" + coupon.language_id + ",'" + coupon.guid + "'," + coupon.itemId + ",'" + coupon.url_Key + "','" + coupon.date_expired.ToString("yyyy-MM-dd H:mm:ss") + "','" + coupon.title.Replace("'",@"\'") + "','" + coupon.last_user + "','" + coupon.status + "','" +
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

        public bool UpdateCouponProd(int language, Guid guid, CouponPages coupon)
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
                        MySqlCommand cmd = new MySqlCommand("SELECT * FROM coupon_pages WHERE language_id = " + language + " and guid = '" + guid + "'", conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            currentVal = Convert.ToInt32(reader["coupon_page_id"]);
                        }
                        reader.Close();

                        if (currentVal != null) // && currentVal == imageId
                        {
                            string query = "UPDATE coupon_pages SET title = '" + coupon.title.Replace("'",@"\'") + "', date_expired = '" + coupon.date_expired.ToString("yyyy-MM-dd H:mm:ss") + "', status = '" +
                            coupon.status + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
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

        public bool CheckFileOnProd(CouponPages model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT guid FROM coupon_pages where guid = '" + model.guid + "';", con);
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

        public CouponPages FindByLangGuid(Guid guidId)
        {
            try
            {
                return _dbContext.CouponPages.Where(p => p.guid == guidId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
    }
}
