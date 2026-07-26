using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using LinqKit;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace CMS.Infrastructure.Data
{

    public class ShopTireAllRepository : IShopTireAllRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IApplicationRepository _appRepo;
        public ShopTireAllRepository(CMSDbContext dbContext, IConfiguration configuration, IApplicationRepository appRepo)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _appRepo = appRepo;
        }
        public ShopTire FindById(Guid id)
        {
            ShopTire shopTire = new ShopTire();
            if (id != null)
            {
                shopTire = _dbContext.ShopTire.Where(x => x.guid == id).FirstOrDefault();
            }
            //return _dbContext.ShopTire.Find(id);
            return shopTire;
        }

        public IEnumerable<ShopTire> GetAllShopTiresAll()
        {
            try
            {
                return _dbContext.ShopTire.Where(x => x.store_num == -999).ToList().OrderBy(x => x.date_updated);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public List<ShopTireAllDto> GetShopTireAllList(ShopTireAllDto ShopTireAll)
        {
            try
            {
                var predicate = PredicateBuilder.True<ShopTire>();

                if (!string.IsNullOrEmpty(ShopTireAll.Stores)) { predicate.And(x => Convert.ToString(x.store_num).Equals(ShopTireAll.Stores)); }
                if (!string.IsNullOrEmpty(ShopTireAll.Title)) { predicate.And(x => x.title.Equals(ShopTireAll.Title)); }
                if (!string.IsNullOrEmpty(ShopTireAll.ImageName)) { predicate.And(x => x.image.Equals(ShopTireAll.ImageName)); }
                if (!string.IsNullOrEmpty(ShopTireAll.ViewDevice)) { predicate.And(x => x.view.Equals(ShopTireAll.ViewDevice)); }
                //if (!string.IsNullOrEmpty(shopTire.status)) { predicate.And(x => x.status.Contains("delete")); }
                if (!string.IsNullOrEmpty(ShopTireAll.ImageStatus)) { predicate.And(x => x.status.Equals(ShopTireAll.ImageStatus)); }
                if (!string.IsNullOrEmpty(ShopTireAll.Hyperlink)) { predicate.And(x => x.ad_hyperlink.Equals(ShopTireAll.Hyperlink)); }

                List<ShopTireAllDto> shopTires = (from q in _dbContext.ShopTire.Where(predicate)
                                                  where q.store_num == -999
                                                  select new ShopTireAllDto
                                                  {
                                                      guid = q.guid,
                                                      ShopTireId = q.shopTire_id,
                                                      LanguageId = q.language_id,
                                                      Stores = q.store_num.ToString(),
                                                      Title = q.title.Trim().ToLower().ToString(),
                                                      ImageName = q.image.Trim().ToLower().ToString(),
                                                      ViewPage = q.page,
                                                      ViewDevice = q.view,
                                                      LastUser = q.last_user,
                                                      ImageStatus = q.status,
                                                      Hyperlink = q.ad_hyperlink
                                                  }).ToList();
                return shopTires;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }


        public bool InsertShopTireAllProd(List<ShopTire> ShopTireAll)
        {
            throw new NotImplementedException();
        }

        public bool SaveScriptAndData(string formMode, ShopTire ShopTireAll, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                
                if (formMode == "Edit")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "UPDATE shoptire SET language_id = " + ShopTireAll.language_id + ", title = '" + ShopTireAll.title.Replace("'", @"\'") +
                        "', image = '" + ShopTireAll.image + "', ad_hyperlink = '" + ShopTireAll.ad_hyperlink + "', view = '" + ShopTireAll.view + "', last_user = '" + ShopTireAll.last_user +
                        "', status = '" + ShopTireAll.status + "', date_created = '" + Convert.ToDateTime(ShopTireAll.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(ShopTireAll.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "' WHERE guid = '" + ShopTireAll.guid + "' and store_num = -999;";

                    qScriptData.img_id = ShopTireAll.shopTire_id;
                    qScriptData.que_desc = "Uploaded Shoptire All - generic Image: " + ShopTireAll.image + " with Store No -999";
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;
                    qScriptData.img_guid = ShopTireAll.guid;
                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isAdded = SaveImageDetails(ShopTireAll, ref queuesDto, formMode);

                    if (isAdded)
                    {
                        queuesDto.ScriptQueueId = qScriptData.que_id;
                        queuesDto.ScriptQueueStatus = qScriptData.Status;
                    }

                    return isAdded;
                }
                return isAdded;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                
            }
        }


        public bool SaveImageDetails(ShopTire shopTireAll, ref TrackingQueuesDto queuesDto, string mode)
        {
            tblquecmsimage qImageData;
            try
            {
                if (mode == "Edit")
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_guid = shopTireAll.guid;
                    qImageData.img_id = shopTireAll.shopTire_id;
                    qImageData.img_description = shopTireAll.image + " Store: -999 " ;
                    qImageData.img_name = shopTireAll.image;
                    qImageData.img_uploadPath = _configuration.GetSection("ShopTiresPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = shopTireAll.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "ShopTireAll";
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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }


        public bool Update(ShopTire ShopTireAllData, string mode)
        {
            try
            {
                TrackingQueuesDto queuesDto = new TrackingQueuesDto();
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "Edit")
                        {
                            ShopTire shopTireList = FindByGuidID(ShopTireAllData.guid);
                            if(shopTireList != null)
                            {
                                shopTireList.title = ShopTireAllData.title;
                                shopTireList.image = ShopTireAllData.image;
                                shopTireList.page = ShopTireAllData.page;
                                shopTireList.view = ShopTireAllData.view;
                                shopTireList.last_user = ShopTireAllData.last_user;
                                shopTireList.status = ShopTireAllData.status;
                                shopTireList.ad_hyperlink = ShopTireAllData.ad_hyperlink;
                                shopTireList.date_updated = DateTime.Now;
                                _dbContext.SaveChanges();
                                transaction.Commit();
                                return true;
                            }
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
                        
                    }   
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public ShopTire FindByGuidID(Guid guidId)
        {
            try
            {
                return _dbContext.ShopTire.Where(p => p.guid == guidId && p.store_num == -999).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public ShopTire FindByGuidStore(Guid guidId, int storeNo)
        {
            try
            {
                return _dbContext.ShopTire.Where(p => p.guid == guidId && p.store_num == storeNo).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool UpdateShopTireAllImageProd(Guid imageId, ShopTire shopTire)
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
                        MySqlCommand cmd = new MySqlCommand("SELECT * FROM shoptire WHERE guid = '" + imageId + "' and store_num = -999" , conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            currentVal = Convert.ToInt32(reader["shopTire_id"]);
                        }
                        reader.Close();

                        if (currentVal != null)//&& currentVal.Equals(imageId)
                        {
                            string query = "UPDATE shoptire SET language_id = " + shopTire.language_id +
                            ", title = '" + shopTire.title.Replace("'", @"\'") + "', image = '" + shopTire.image + "',ad_hyperlink = '" + shopTire.ad_hyperlink + "', " +
                            " date_created = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                            "' WHERE guid = '" + imageId + "' and store_num = -999; ";

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

    }
}
