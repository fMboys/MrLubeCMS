using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using Google.Protobuf.WellKnownTypes;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Memcached;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
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
    public class ShopTireRepository : IShopTireRepository
    {
        private readonly ILogger<ShopTireRepository> _logger;
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IApplicationRepository _appRepo;

        public ShopTireRepository(ILogger<ShopTireRepository> logger,CMSDbContext dbContext, IConfiguration configuration, IApplicationRepository appRepo)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _appRepo = appRepo;
            _logger = logger;
        }

        public IEnumerable<ShopTire> GetAllShopTires()
        {
            try
            {
                return _dbContext.ShopTire.ToList().OrderBy(x => x.date_updated);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
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

        public List<ShopTire> FindByIdList(Guid id)
        {
            List<ShopTire> shopTire = new List<ShopTire>();
            if (id != null)
            {
                shopTire = _dbContext.ShopTire.Where(x => x.guid == id).ToList();
            }
            //return _dbContext.ShopTire.Find(id);
            return shopTire;
        }

        //public List<ShopTireDto> FindShopTireIdList(Guid id)
        //{
        //    List<ShopTire> shopTire = new List<ShopTire>();
        //    if (id != null)
        //    {
        //        shopTire = _dbContext.ShopTire.Where(x => x.guid == id).ToList();
        //    }
        //    //return _dbContext.ShopTire.Find(id);
        //    return shopTire;
        //}

        public List<ShopTireDto> GetShopTireList(ShopTireDto shopTire)
        {
            try
            {
                var predicate = PredicateBuilder.True<ShopTire>();

                if (!string.IsNullOrEmpty(shopTire.Stores)) { predicate.And(x => Convert.ToString(x.store_num).Equals(shopTire.Stores )); }
                if (!string.IsNullOrEmpty(shopTire.Title)) { predicate.And(x => x.title.Equals(shopTire.Title)); }
                if (!string.IsNullOrEmpty(shopTire.ImageName)) { predicate.And(x => x.image.Equals(shopTire.ImageName)); }
                if (!string.IsNullOrEmpty(shopTire.ViewDevice)) { predicate.And(x => x.view.Equals(shopTire.ViewDevice)); }
                //if (!string.IsNullOrEmpty(shopTire.status)) { predicate.And(x => x.status.Contains("delete")); }
                if (!string.IsNullOrEmpty(shopTire.ImageStatus)) { predicate.And(x => x.status.Equals(shopTire.ImageStatus)); }
                if (!string.IsNullOrEmpty(shopTire.Hyperlink)) { predicate.And(x => x.ad_hyperlink.Equals(shopTire.Hyperlink)); }

                List<ShopTireDto> shopTires = (from q in _dbContext.ShopTire.Where(predicate)
                                            where q.status != "delete" && q.store_num != -999
                                            select new ShopTireDto
                                            {
                                                guid = q.guid,
                                                ShopTireId = q.shopTire_id,
                                                LanguageId = q.language_id,
                                                Stores = q.store_num.ToString(),
                                                Title = q.title.Trim().ToLower().ToString(),
                                                ImageName = q.image,
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

        public bool Add(ShopTire shopTire)
        {
            try
            {
                if (shopTire.SelectedStores != null)
                {
                    TrackingQueuesDto queuesDto = new TrackingQueuesDto();

                    string[] selectesStoreNo = shopTire.SelectedStores.ToArray();
                    foreach (string select in selectesStoreNo)
                    {
                        ShopTire shopTire1 = new ShopTire();
                        shopTire1.guid = shopTire.guid;
                        shopTire1.language_id = shopTire.language_id;
                        shopTire1.store_num = Convert.ToInt32(select.Trim());
                        shopTire1.title = shopTire.title;
                        shopTire1.image = shopTire.image;
                        shopTire1.page = shopTire.page;
                        shopTire1.view = shopTire.view;
                        shopTire1.last_user = shopTire.last_user;
                        shopTire1.status = shopTire.status;
                        shopTire1.date_created = shopTire.date_created;
                        shopTire1.date_updated = shopTire.date_updated;
                        shopTire1.ad_hyperlink = shopTire.ad_hyperlink;

                        _dbContext.ShopTire.Add(shopTire1);
                        _dbContext.SaveChanges();

                        SaveScriptAndData("Create", shopTire1, ref queuesDto, shopTire1.last_user);
                    }
                    SaveImageDetails(shopTire, ref queuesDto, "Create");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        /// <summary>
        /// Add new ShopTire data into production database.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool InsertShopTireProd(List<ShopTire> shopTire)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        foreach (var item in shopTire)
                        {
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO shoptire (guid,language_id,store_num,title,image,page,view,last_user,status," +
                        "date_created,date_updated,ad_hyperlink) VALUES ('" + item.guid + "'," + item.language_id + "," + item.store_num + ",'" +
                        item.title.Replace("'",@"\'") + "','" + item.image + "','" + item.page + "','" + item.view + "','" + item.last_user + "','" + item.status + "','" +
                        DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + item.ad_hyperlink + "')", con);

                            cmd.ExecuteNonQuery();
                        }
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

        /// <summary>
        /// Update ShopTire data of production database.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool UpdateShopTireProd(Guid imageId, List<ShopTire> shopTire)
        {
            try
            {

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        RemoveAllShoptireImageProdByGUID(imageId);
                        //InsertShopTireImageProd(shopTire);
                        InsertShopTireProd(shopTire);

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

            //        {
            //            conn.Open();

            //            //cheking the banner id
            //            string? currentVal = null;
            //            MySqlCommand cmd = new MySqlCommand("SELECT * FROM shopTire WHERE guid = '" + imageId + "';", conn);
            //            MySqlDataReader reader = cmd.ExecuteReader();
            //            while (reader.Read())
            //            {
            //                currentVal = Convert.ToString(reader["shopTire_id"]);
            //            }
            //            reader.Close();

            //            if (currentVal != null )
            //            {
            //                string query = "UPDATE shoptire SET language_id = " + shopTire.language_id +
            //                ", store_num = " + shopTire.store_num + ", title = '" + shopTire.title + "', image = '" + shopTire.image + "',page = '" + shopTire.page + "', status = '" +
            //                shopTire.status + "', date_created = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "', date_updated = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
            //                "', ad_hyperlink = '" + shopTire.ad_hyperlink + "' WHERE guid = '" + imageId + "'; ";

            //                MySqlCommand sqlCommand = new MySqlCommand(query, conn);

            //                sqlCommand.ExecuteNonQuery();
            //                conn.Close();
            //                return true;

            //            }
            //            else
            //            {
            //                return false;
            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            var msg = ex.Message.ToString();
            //            return false;
            //        }
            //    }
            //}

            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Update a specific record of ShopTire in Database.
        /// </summary>
        /// <param name="shopTireData"></param>
        /// <returns>Boolean result</returns>
        /// <exception cref="Exception"></exception>
        public bool Update(ShopTire shopTireData, string mode)
        {
            try
            {
                TrackingQueuesDto queuesDto = new TrackingQueuesDto();
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "Delete")
                        {
                            List<ShopTire> shopTireList = FindByIdList(shopTireData.guid);
                            //string[] selectesStoreNo = shopTireData.SelectedStores.ToArray();
                            if (shopTireList != null)
                            {
                                foreach (ShopTire shopTire in shopTireList)
                                {
                                    shopTire.status = "delete";
                                    shopTire.date_created = shopTire.date_created;
                                    shopTire.date_updated = DateTime.Now;

                                    _dbContext.Entry(shopTire).CurrentValues.SetValues(shopTire);
                                    _dbContext.SaveChanges();

                                    //SaveScriptAndData("Delete", shopTire, ref queuesDto, shopTire.last_user);
                                }

                                transaction.Commit();



                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            ShopTire shopTireImage = null;
                            List<ShopTire> shopTireList = FindByIdList(shopTireData.guid);
                            _dbContext.ShopTire.RemoveRange(shopTireList);

                            _appRepo.RemoveShopTireQueueImageByGuid(shopTireData.guid);
                            _appRepo.RemoveAllQueueScriptsByGuid(shopTireData.guid);
                            string[] selectesStoreNo = shopTireData.SelectedStores.ToArray();
                            if (selectesStoreNo != null)
                            {

                                foreach (string select in selectesStoreNo)
                                {

                                    //shopTireData.store_num = Convert.ToInt32(select.Trim());
                                    //shopTireData.date_created = shopTire.date_created;
                                    //shopTireData.date_updated = DateTime.Now;

                                    //_dbContext.Entry(shopTire).CurrentValues.SetValues(shopTireData);
                                    //_dbContext.SaveChanges();
                                    shopTireImage = new ShopTire();


                                    shopTireImage.guid = shopTireData.guid;
                                    shopTireImage.language_id = shopTireData.language_id;
                                    shopTireImage.store_num = Convert.ToInt32(select.Trim());
                                    shopTireImage.title = shopTireData.title;
                                    shopTireImage.image = shopTireData.image;
                                    shopTireImage.page = shopTireData.page;
                                    shopTireImage.view = shopTireData.view;
                                    shopTireImage.last_user = shopTireData.last_user;
                                    shopTireImage.status = shopTireData.status;
                                    shopTireImage.date_created = shopTireData.date_created;
                                    shopTireImage.date_updated = shopTireData.date_updated;
                                    shopTireImage.ad_hyperlink = shopTireData.ad_hyperlink;

                                    _dbContext.ShopTire.Add(shopTireImage);
                                    _dbContext.SaveChanges();

                                    SaveScriptAndData("Create", shopTireImage, ref queuesDto, shopTireData.last_user);


                                }
                                SaveImageDetails(shopTireData, ref queuesDto, string.Empty);
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

        public bool Delete(ShopTire shopTireData)
        {
            try
            {
                //shopTireData = _dbContext.ShopTire.SingleOrDefault(x => x.shopTire_id == id);

                if (shopTireData != null)
                {
                    _dbContext.Attach(shopTireData);
                    //_dbContext.Entry(shopTire).State = EntityState.Deleted;
                    _dbContext.ShopTire.Remove(shopTireData);
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
        public bool SaveScriptAndData(string formMode, ShopTire shopTire, ref TrackingQueuesDto queuesDto, string user)
        {
            bool isAdded = false;
            try
            {
                if (formMode == "Create")
                {
                    tblquecms qScriptData = new tblquecms();
                    var queryScript = "INSERT INTO shopTire(guid,language_id, store_num, title, image, page, view, last_user, status, date_created, date_updated, ad_hyperlink)" +
                        "VALUES('" + shopTire.guid + "'," + shopTire.language_id + "," + shopTire.store_num + ",'" + shopTire.title.Replace("'",@"\'") + "','" + shopTire.image + "','" + shopTire.page + "','" + shopTire.view + "','" +
                        shopTire.last_user + "','" + shopTire.status + "','" + Convert.ToDateTime(shopTire.date_created).ToString("yyyy-MM-dd H:mm:ss") + "','" +
                        Convert.ToDateTime(shopTire.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "','" + shopTire.ad_hyperlink + "')";
                    qScriptData.img_guid = shopTire.guid;
                    qScriptData.img_id = shopTire.shopTire_id;
                    qScriptData.que_desc = "Uploaded Image: " + shopTire.image + " with Status: " + shopTire.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    //isAdded = SaveImageDetails(shopTire, ref queuesDto, formMode);
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
                    var queryScript = "UPDATE shopTire SET language_id = " + shopTire.language_id + ", store_num = " + shopTire.store_num + ", title = '" + shopTire.title.Replace("'",@"\'") +
                        "', image = '" + shopTire.image + "', page = '" + shopTire.page + "', view = '" + shopTire.view + "', last_user = '" + shopTire.last_user +
                        "', status = '" + shopTire.status + "', date_created = '" + Convert.ToDateTime(shopTire.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(shopTire.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', ad_hyperlink = '" + shopTire.ad_hyperlink + "' WHERE guid = '" + shopTire.guid + "';";
                    qScriptData.img_guid = shopTire.guid;
                    qScriptData.img_id = shopTire.shopTire_id;
                    qScriptData.que_desc = "Uploaded Image: " + shopTire.image + " with Status: " + shopTire.status;
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isAdded = SaveImageDetails(shopTire, ref queuesDto, formMode);

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
                    var queryScript = "UPDATE shopTire SET language_id = " + shopTire.language_id + ", store_num = " + shopTire.store_num + ", title = '" + shopTire.title.Replace("'",@"\'") +
                        "', image = '" + shopTire.image + "', page = '" + shopTire.page + "', view = '" + shopTire.view + "', last_user = '" + shopTire.last_user +
                        "', status = 'delete', date_created = '" + Convert.ToDateTime(shopTire.date_created).ToString("yyyy-MM-dd H:mm:ss") +
                        "', date_updated = '" + Convert.ToDateTime(shopTire.date_updated).ToString("yyyy-MM-dd H:mm:ss") + "', ad_hyperlink = '" + shopTire.ad_hyperlink + "' WHERE guid = '" + shopTire.guid + "';";
                    qScriptData.img_guid = shopTire.guid;
                    qScriptData.img_id = shopTire.shopTire_id;
                    qScriptData.que_desc = "Uploaded Image: " + shopTire.image + " with Status: Delete";
                    qScriptData.que_script = queryScript;
                    qScriptData.que_date = DateTime.Now;
                    qScriptData.que_user = user;
                    qScriptData.Status = "Pending";
                    qScriptData.created_date = DateTime.Now;
                    qScriptData.updated_date = DateTime.Now;

                    _dbContext.Add(qScriptData);
                    _dbContext.SaveChanges();

                    queuesDto.ScriptQueueId = qScriptData.que_id;

                    isDelete = SaveImageDetails(shopTire, ref queuesDto, formMode);
                    //isDelete = true;
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
        public bool SaveImageDetails(ShopTire shopTire, ref TrackingQueuesDto queuesDto, string mode)
        {
            tblquecmsimage qImageData;
            try
            {
                if (mode == "Delete")
                {
                    qImageData = new tblquecmsimage();
                    qImageData.tblquecms_id = queuesDto.ScriptQueueId;
                    qImageData.img_guid = shopTire.guid;
                    qImageData.img_id = shopTire.shopTire_id;
                    qImageData.img_description = shopTire.image + " Status: " + shopTire.status;
                    qImageData.img_name = shopTire.image;
                    qImageData.img_uploadPath = _configuration.GetSection("ShopTiresPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = shopTire.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "ShopTire";
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
                    qImageData.img_guid = shopTire.guid;
                    qImageData.img_id = shopTire.shopTire_id;
                    qImageData.img_description = shopTire.image + " Status: " + shopTire.status;
                    qImageData.img_name = shopTire.image;
                    qImageData.img_uploadPath = _configuration.GetSection("ShopTiresPath").Value;
                    qImageData.upload_date = DateTime.Now;
                    qImageData.img_user = shopTire.last_user;
                    qImageData.Status = "pending";
                    qImageData.banner_type = "ShopTire";
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

        public bool DeleteImageDetails(ShopTire shopTire)
        {
            List<tblquecmsimage> qImageData = new List<tblquecmsimage>();
            try
            {
                qImageData = _dbContext.Tblquecmsimage.Where(x => x.img_guid == shopTire.guid && x.banner_type == "ShopTire").ToList();

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

        public bool CheckFileOnProd(ShopTire model)
        {
            string? currentVal = null;
            using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM shoptire where guid = '" + model.guid + "';", con);
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

        public async Task<IEnumerable<SelectListItem>> GetStoreNumbersList(int lang, string view)
        {
            try
            {
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                //List<Store> storeList = new List<Store>();
                List<StoreViewModel> storeListPud = new List<StoreViewModel>();
                List<ShopTire> usedStores = new List<ShopTire>();

                //storeList = _dbContext.store.ToList();

                //API call for Stores from PUD
                //GetStoresAllAsync(storeListPud);
                //////////////////////////////////// Mrlube Data
                var client = new RestClient(_configuration.GetSection("API").Value);
                RestRequest restRequest;

                restRequest = new RestRequest("Store/GetAllStores", RestSharp.Method.Post);
                //}
                //else
                //{
                //    restRequest = new RestRequest("Store/GetStoresByCity", Method.Post);
                //    restRequest.AddQueryParameter("City", SearchFilter);
                //}
                var response =await client.ExecuteAsync(restRequest);
                if (response.ErrorMessage != null)
                {
                    _logger.LogError("Locations - SearchStores: " + response.ErrorMessage);
                    _logger.LogError(response.ErrorMessage);
                }
                if (response.ErrorMessage != null)
                {
                    _logger.LogError(response.ErrorMessage);
                    if (response.ErrorException != null)
                    {
                        _logger.LogError("Locations - SearchStores: " + response.ErrorMessage);
                        _logger.LogError(response.ErrorException.InnerException.Message);
                    }
                }
                List<StoreViewModel> store_lst = new List<StoreViewModel>();
                //var province = new List<string>(); ;
                if (response.IsSuccessful)
                {
                    store_lst = JsonConvert.DeserializeObject<List<StoreViewModel>>(response.Content);
                    storeListPud = store_lst.OrderBy(a => a.StoreNumber).ToList();

                }


                usedStores = _dbContext.ShopTire.Where(x => x.language_id == lang && x.view == view && (x.status == "active" || x.status == "inactive")).ToList();
                foreach (var item in usedStores)
                {
                    storeListPud = storeListPud.Where(x => x.StoreNumber != item.store_num).ToList();
                }

                if (storeListPud.Count > 0)
                {

                    //storeList.RemoveRange(usedStores.Count,usedStores.Count);


                    foreach (StoreViewModel store in storeListPud)
                    {



                        selectListItems.Add(new SelectListItem
                        {
                            Value = store.StoreNumber.ToString(),
                            Text = store.StoreNumber.ToString()
                        });


                    }

                    //SelectListItem storeTip = new SelectListItem()
                    //{
                    //    Value = null,
                    //    Text = " --- Select store number --- "
                    //};

                    //selectListItems.Insert(0, storeTip);
                }


                //List<SelectListItem> countries = _dbContext.store
                //        .Select(n =>
                //        new SelectListItem
                //        {
                //            Value = n.store_num.ToString(),
                //            Text = n.store_num.ToString()
                //        }).ToList();


                return selectListItems;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        //Pud Stores List
        //public async Task<StoreViewModel> GetStoresAllAsync(List<StoreViewModel> storeListPud)
        //{
        //    //////////////////////////////////// Mrlube Data
        //    var client = new RestClient(_configuration.GetSection("API").Value);
        //    RestRequest restRequest;

        //    restRequest = new RestRequest("Store/GetAllStores", RestSharp.Method.Post);
        //    //}
        //    //else
        //    //{
        //    //    restRequest = new RestRequest("Store/GetStoresByCity", Method.Post);
        //    //    restRequest.AddQueryParameter("City", SearchFilter);
        //    //}
        //    var response = await client.ExecuteAsync(restRequest);
        //    //if (response.Exception != null)
        //    //{
        //    //    _logger.LogError("Locations - SearchStores: " + response.Exception);
        //    //    _logger.LogError(response.Exception.Message);
        //    //}
        //    //if (response.Exception != null)
        //    //{
        //    //    _logger.LogError(response.Exception.Message);
        //    //    if (response.Exception.InnerException != null)
        //    //    {
        //    //        _logger.LogError("Locations - SearchStores: " + response.Exception);
        //    //        _logger.LogError(response.Exception.InnerException.Message);
        //    //    }
        //    //}
        //    List<StoreViewModel> store_lst = new List<StoreViewModel>();
        //    //var province = new List<string>(); ;
        //    if (response.IsSuccessful)
        //    {
        //        store_lst = JsonConvert.DeserializeObject<List<StoreViewModel>>(response.Content);
        //        storeListPud = store_lst.OrderBy(a => a.StoreNumber).ToList();
        //        //province = store_lst.Select(x => x.ProvinceFullName).Distinct().ToList();
        //        //var cities = store_lst.Where(a=>a.ProvinceAbbr = )

        //    }
        //    //ViewBag.Provinces = province;
        //    //ViewBag.CityPages = store_lst;
        //}


        //Get Store List on Edit
        public async Task<IEnumerable<SelectListItem>> GetStoreNumbersEditList(Guid guid, ShopTire shopTire)
        {
            try
            {
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                //List<Store> storeList = new List<Store>();
                List<StoreViewModel> storeListPud = new List<StoreViewModel>();
                List<ShopTire> usedStores = new List<ShopTire>();

                //shoptire Stores on guid 
                List<ShopTire> EditStores = new List<ShopTire>();

                //API call for Stores from PUD
                //GetStoresAll(storeListPud);
                //API call for Stores from PUD
                //GetStoresAllAsync(storeListPud);
                //////////////////////////////////// Mrlube Data
                var client = new RestClient(_configuration.GetSection("API").Value);
                RestRequest restRequest;

                restRequest = new RestRequest("Store/GetAllStores", RestSharp.Method.Post);
                //}
                //else
                //{
                //    restRequest = new RestRequest("Store/GetStoresByCity", Method.Post);
                //    restRequest.AddQueryParameter("City", SearchFilter);
                //}
                var response = await client.ExecuteAsync(restRequest);
                if (response.ErrorMessage != null)
                {
                    _logger.LogError("Locations - SearchStores: " + response.ErrorMessage);
                    _logger.LogError(response.ErrorMessage);
                }
                if (response.ErrorMessage != null)
                {
                    _logger.LogError(response.ErrorMessage);
                    if (response.ErrorException != null)
                    {
                        _logger.LogError("Locations - SearchStores: " + response.ErrorMessage);
                        _logger.LogError(response.ErrorException.InnerException.Message);
                    }
                }
                List<StoreViewModel> store_lst = new List<StoreViewModel>();
                //var province = new List<string>(); ;
                if (response.IsSuccessful)
                {
                    store_lst = JsonConvert.DeserializeObject<List<StoreViewModel>>(response.Content);
                    storeListPud = store_lst.OrderBy(a => a.StoreNumber).ToList();

                }
                //List<StoreViewModel> storeList = new List<StoreViewModel>();
                //storeList = storeListPud.Select(a => a.StoreNumber).ToList();   

                EditStores = _dbContext.ShopTire.Where(x => x.guid == guid).ToList();
                var itms = EditStores.Select(a => a.store_num).ToList();
                //storeList = _dbContext.store.ToList();
                usedStores = _dbContext.ShopTire.Where(x => x.language_id == shopTire.language_id && x.view == shopTire.view &&
                (x.status == "active" || x.status == "inactive")).ToList();
                usedStores = usedStores.Where(x => !itms.Contains(x.store_num)).ToList();
                foreach (var item in usedStores)
                {
                    storeListPud = storeListPud.Where(x => x.StoreNumber != item.store_num).ToList();

                }
                List<StoreViewModel> EditableStore = new List<StoreViewModel>();
                StoreViewModel editedstore = new StoreViewModel();
                //editedstore = _dbContext.store.ToList();
                //if (EditStores.Count > 0)
                //{
                //    foreach (var itemstore in EditStores)
                //    {
                //        //editedstore = _dbContext.store.Where(x => x.store_num == itemstore.store_num).FirstOrDefault();
                //        editedstore = storeListPud.Where(x => x.StoreNumber == itemstore.store_num).FirstOrDefault();
                //        if (editedstore != null)
                //        {
                //            storeListPud.Add(editedstore);
                //        }
                //    }
                //}
                storeListPud = storeListPud.OrderBy(a => a.StoreNumber).ToList();
                if (storeListPud.Count > 0)
                {
                    foreach (StoreViewModel store in storeListPud)
                    {
                        selectListItems.Add(new SelectListItem
                        {
                            Value = store.StoreNumber.ToString(),
                            Text = store.StoreNumber.ToString()
                        });
                    }
                }
                List<string> selectstor = new List<string>();
                foreach (var store in EditStores)
                {

                    selectstor.Add(store.store_num.ToString());

                }
                //shopTire.SelectedStores = (string.Join(",", store.store_num).ToString().Split(",").ToArray());
                shopTire.SelectedStores = selectstor.ToArray();


                return selectListItems;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }


        public bool uploadImagetoFTPServer(string imgName)
        {
            try
            {
                //Staging FTP server
                string FileDomain = _configuration.GetSection("FTP_Server").Value;
                string FilePath = _configuration.GetSection("ShopTiresPath").Value;
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
                string ProdFilePath = _configuration.GetSection("ShopTiresPath").Value;
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

        public IEnumerable<ShopTire> GetShopTireByGUID(Guid imageGUID)
        {
            try
            {
                if (imageGUID != Guid.Empty)
                    return _dbContext.ShopTire.Where(x => x.guid == imageGUID).ToList();

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.FullName, ex);
            }
        }

        public bool RemoveAllShoptireImageProdByGUID(Guid GUID)
        {
            bool isRemoved = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM shoptire WHERE guid = '" + GUID + "'", conn);

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

        public bool InsertShopTireImageProd(List<ShopTire> shoptire)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetSection("ConnectionStrings:ProdMySqlConnection").Value))
                {
                    try
                    {
                        con.Open();
                        foreach (var item in shoptire)
                        {
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO shoptire (guid,language_id,title,image,page,url_key,view,last_user,status," +
                            "date_created,date_updated,ad_hyperlink) VALUES ('" + item.guid + "'," + item.language_id + ",'" + item.title + "','" + item.image +
                            "','" + item.view + "','" + item.last_user + "','" + item.status + "','" +
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
    }
}
