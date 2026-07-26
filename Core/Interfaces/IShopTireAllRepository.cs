using CMS.Core.DTOs;
using CMS.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.Core.Interfaces
{
    public interface IShopTireAllRepository
    {
/// <summary>
        /// Give data list of all shoptiresAll.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShopTire> GetAllShopTiresAll();
        /// <summary>
        /// Retrieve a list of shoptiresAll for specified conditions.
        /// </summary>
        /// <param name="ShopTireAll"></param>
        /// <returns></returns>
        List<ShopTireAllDto> GetShopTireAllList(ShopTireAllDto ShopTireAll);
        /// <summary>
        /// Get a specific ShopTireAll using ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopTire FindById(Guid id);


        public ShopTire FindByGuidStore(Guid guidId,int storeNo);


        public bool UpdateShopTireAllImageProd(Guid imageId, ShopTire shopTire);

        public bool SaveScriptAndData(string formMode, ShopTire ShopTireAll, ref TrackingQueuesDto queuesDto, string user);
        /// <summary>
        /// Save ShopTireAll data into database.
        /// </summary>
        /// <param name="ShopTireAll"></param>
        /// <returns></returns>
        //bool Add(ShopTireAll ShopTireAll);
        /// <summary>
        /// Update a specific record of ShopTireAll in Database.
        /// </summary>
        /// <param name="ShopTireAllData"></param>
        /// <returns>Boolean result</returns>
        /// <exception cref="Exception"></exception>
        bool Update(ShopTire ShopTireAllData, string mode);
        /// <summary>
        /// Delete a specific ShopTireAll record from database.
        /// </summary>
        /// <param name="ShopTireAll"></param>
        /// <returns></returns>
        //bool Delete(ShopTireAll ShopTireAll);
        /// <summary>
        /// Generate the script query of ShopTireAll for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="formMode"></param>
        /// <param name="ShopTireAll"></param>
        /// <param name="queuesDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        
        /// <summary>
        /// Add new ShopTireAll data into production database.
        /// </summary>
        /// <returns>Boolean</returns>
        bool InsertShopTireAllProd(List<ShopTire> ShopTireAll);

        /// <summary>
        /// Update ShopTireAll data of production  
    }
}
