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
    public class CouponImagesRepository : ICouponImagesRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public CouponImagesRepository(CMSDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public IEnumerable<CouponImages> GetAllCouponImages()
        {
            try
            {
                return _dbContext.CouponImages.ToList().OrderBy(x => x.date_updated);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public CouponImages FindById(int id)
        {
            CouponImages coupon = new CouponImages();
            if (id != 0)
            {
                coupon = _dbContext.CouponImages.Where(x => x.coupon_image_id == id).FirstOrDefault();
            }
            return coupon;
        }

        public CouponImages FindByGuidID(Guid guidId)
        {
            try
            {
                return _dbContext.CouponImages.Where(p => p.guid == guidId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public List<CouponImages> GetCouponImageList()
        {
            try
            {
                List<CouponImages> coupon = (from q in _dbContext.CouponImages//.Where(predicate)
                                           where q.status != "delete"
                                           select new CouponImages
                                           {
                                               guid = q.guid,
                                               coupon_image_id = q.coupon_image_id,
                                               language_id = q.language_id,
                                               title = q.title,
                                               image = q.image,
                                               url_key = q.url_key,
                                               view = q.view,
                                               last_user = q.last_user,
                                               status = q.status 
                                           }).ToList();
                foreach (var item in coupon)
                {
                    CouponPages couponPage = _dbContext.CouponPages.Where(x => x.url_Key == item.url_key).FirstOrDefault();

                    if (couponPage != null)
                    {
                        item.page = couponPage.title;
                    }
                }

                return coupon;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool Add(CouponImages coupon)
        {
            try
            {
                _dbContext.CouponImages.Add(coupon);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool IsAlreadyExists(CouponImages coupon)
        {
            try
            {
                if (coupon.coupon_image_id == 0)
                {
                    if (_dbContext.CouponImages.Where(x => x.url_key == coupon.url_key
                                                        && x.view == coupon.view
                                                        && x.language_id == coupon.language_id
                                                        && x.status == "active").ToList().Count() > 0)
                    {
                        return true;
                    }
                }
                else
                {

                    if (_dbContext.CouponImages.Where(x => x.url_key == coupon.url_key
                                                        && x.view == coupon.view
                                                        && x.language_id == coupon.language_id
                                                        && x.status == "active"
                                                        && x.coupon_image_id != coupon.coupon_image_id).ToList().Count() > 0)
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

        public bool InsertCouponImageProd(CouponImages coupon)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO coupon_images (guid,language_id,title,image,url_key,view,last_user,status," +
                        "date_created,date_updated) VALUES ('" + coupon.guid + "'," + coupon.language_id + ",'" +
                        coupon.title.Replace("'",@"\'") + "','" + coupon.image + "','" + coupon.url_key + "','" + coupon.view + "','" + coupon.last_user + "','" + coupon.status + "','" +
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

        public bool UpdateCouponImageProd(Guid imageId, CouponImages coupon)
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
                        MySqlCommand cmd = new MySqlCommand("SELECT * FROM coupon_images WHERE guid = '" + imageId + "'", conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            currentVal = Convert.ToInt32(reader["coupon_image_id"]);
                        }
                        reader.Close();

                        if (currentVal != null)//&& currentVal.Equals(imageId)
                        {
                            string query = "UPDATE coupon_images SET language_id = " + coupon.language_id +
                            ", title = '" + coupon.title.Replace("'",@"\'") + "', image = '" + coupon.image + "',url_key = '" + coupon.url_key + "', status = '" +
                            coupon.status + "', date_created = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                            "' WHERE guid = '" + imageId + "'; ";

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
        public bool Update(CouponImages CouponImageData, string mode)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "Delete")
                        {
                            CouponImages coupon = FindByGuidID(CouponImageData.guid);
                            if (coupon != null)
                            {
                                CouponImageData.status = "delete";
                                CouponImageData.date_created = coupon.date_created;
                                CouponImageData.date_updated = DateTime.Now;

                                _dbContext.Entry(coupon).CurrentValues.SetValues(CouponImageData);
                                _dbContext.SaveChanges();
                                transaction.Commit();

                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            CouponImages coupon = FindByGuidID(CouponImageData.guid);
                            if (coupon != null)
                            {
                                CouponImageData.date_created = coupon.date_created;
                                CouponImageData.date_updated = DateTime.Now;

                                _dbContext.Entry(coupon).CurrentValues.SetValues(CouponImageData);
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

        public bool Delete(CouponImages CouponImageData)
        {
            try
            {
                if (CouponImageData != null)
                {
                    _dbContext.Attach(CouponImageData);
                    _dbContext.CouponImages.Remove(CouponImageData);
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
        public bool SaveScriptAndData(string formMode, CouponImages coupon, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO coupon_images(language_id,guid, title, image, url_key, view, last_user, status, date_created, date_updated)" +
                        "VALUES(" + coupon.language_id + ",'" + coupon.guid + "','" + coupon.title.Replace("'",@"\'") + "','" + coupon.image + "','" + coupon.url_key + "','" + coupon.view + "','" +
                        coupon.last_user + "','" + coupon.status + "','" + Convert.ToDateTime(coupon.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(coupon.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "')";
                    qScriptData.img_id = coupon.coupon_image_id;
                    qScriptData.que_desc = "Uploaded Image: " + coupon.image + " with Status: " + coupon.status;
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

                    isAdded = SaveImageDetails(coupon, ref queuesDto, formMode);
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
                    var queryScript = "UPDATE coupon_images SET language_id = " + coupon.language_id + ", title = '" + coupon.title.Replace("'",@"\'") +
                        "', image = '" + coupon.image + "', url_key = '" + coupon.url_key + "', view = '" + coupon.view + "', last_user = '" + coupon.last_user +
                        "', status = '" + coupon.status + "', date_created = '" + Convert.ToDateTime(coupon.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(coupon.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + coupon.guid + "';";

                    qScriptData.img_id = coupon.coupon_image_id;
                    qScriptData.que_desc = "Uploaded Image: " + coupon.image + " with Status: " + coupon.status;
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

                    isAdded = SaveImageDetails(coupon, ref queuesDto, formMode);

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
                    var queryScript = "UPDATE coupon_images SET language_id = " + coupon.language_id + ", title = '" + coupon.title.Replace("'",@"\'") +
                        "', image = '" + coupon.image + "', url_key = '" + coupon.url_key + "', view = '" + coupon.view + "', last_user = '" + coupon.last_user +
                        "', status = 'delete', date_created = '" + Convert.ToDateTime(coupon.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(coupon.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + coupon.guid + "';";
                    qScriptData.img_guid = coupon.guid;
                    qScriptData.img_id = coupon.coupon_image_id;
                    qScriptData.que_desc = "Uploaded Image: " + coupon.image + " with Status: Delete";
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isDelete = SaveImageDetails(coupon, ref queuesDto, formMode);

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
        public bool SaveImageDetails(CouponImages coupon, ref TrackingQueuesDto queuesDto, string mode)
        {
            tblquecmsimage qImageData;
            try
            {
                if (mode == "Delete")
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_guid = coupon.guid;
                    qImageData.img_id = coupon.coupon_image_id;
                    qImageData.img_description = coupon.image + " Status: " + coupon.status;
                    qImageData.img_name = coupon.image;
                    qImageData.img_uploadPath = _configuration.GetSection("CouponImagesPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = coupon.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "coupon";
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
                    qImageData.img_guid = coupon.guid;
                    qImageData.img_id = coupon.coupon_image_id;
                    qImageData.img_description = coupon.image + " Status: " + coupon.status;
                    qImageData.img_name = coupon.image;
                    qImageData.img_uploadPath = _configuration.GetSection("CouponImagesPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = coupon.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "coupon";
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

        public bool DeleteImageDetails(CouponImages coupon)
        {
            List<tblquecmsimage> qImageData = new List<tblquecmsimage>();
            try
            {
                qImageData = _dbContext.Tblquecmsimage.Where(x => x.img_guid == coupon.guid && x.banner_type == "coupon").ToList();

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


        public List<CouponPages> ddlCouponPages()
        {
            try
            {
                return _dbContext.CouponPages.Where(p => p.status != "delete" && p.language_id == 1).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        //public List<CouponPages> ddlCouponPagesSelected(Guid guid)
        //{
        //    try
        //    {
        //        CouponImages couponImages = new CouponImages();
        //        couponImages = _dbContext.CouponImages.Where(p => p.status != "delete" && p.guid == guid).FirstOrDefault(); 
        //        if(couponImages != null)
        //        {
        //            return _dbContext.CouponPages.Where(p => p.status != "delete" && p.language_id == 1 && p.url_Key == couponImages.url_key).ToList();
        //        }
        //        else
        //        {
        //            return null;
        //        } 
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
        //    }
        //}

        //public bool checkCouponURL(Guid id, ref CouponPages couponPages)
        //{
        //    try
        //    {
        //        var result = _dbContext.CouponImages.Where(a => a.guid == id).FirstOrDefault();
        //        couponPages = new CouponPages();
        //        if (result != null)
        //        {
        //            couponPages = _dbContext.CouponPages.Where(a => a.url_Key == result.url_key && a.language_id == result.language_id && a.status != "delete").FirstOrDefault();
        //            if(couponPages != null)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            } 

        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        public bool CheckFileOnProd(CouponImages model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT guid FROM coupon_images where guid = '" + model.guid + "';", con);
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
