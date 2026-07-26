using CMS.Core.DTOs;
using CMS.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.Core.Interfaces
{
    public interface IShopTireRepository
    {
        /// <summary>
        /// Give data list of all shoptires.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShopTire> GetAllShopTires();
        /// <summary>
        /// Retrieve a list of shoptires for specified conditions.
        /// </summary>
        /// <param name="shopTire"></param>
        /// <returns></returns>
        List<ShopTireDto> GetShopTireList(ShopTireDto shopTire);
        /// <summary>
        /// Get a specific shoptire using ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopTire FindById(Guid id);
        /// <summary>
        /// Save shoptire data into database.
        /// </summary>
        /// <param name="shopTire"></param>
        /// <returns></returns>
        bool Add(ShopTire shopTire);
        /// <summary>
        /// Update a specific record of ShopTire in Database.
        /// </summary>
        /// <param name="shopTireData"></param>
        /// <returns>Boolean result</returns>
        /// <exception cref="Exception"></exception>
        bool Update(ShopTire shopTireData, string mode);
        /// <summary>
        /// Delete a specific shoptire record from database.
        /// </summary>
        /// <param name="shopTire"></param>
        /// <returns></returns>
        bool Delete(ShopTire shopTire);
        /// <summary>
        /// Generate the script query of shoptire for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="formMode"></param>
        /// <param name="shopTire"></param>
        /// <param name="queuesDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        bool SaveScriptAndData(string formMode, ShopTire shopTire, ref TrackingQueuesDto queuesDto, string user);
        /// <summary>
        /// Add new ShopTire data into production database.
        /// </summary>
        /// <returns>Boolean</returns>
        bool InsertShopTireProd(List<ShopTire> shopTire);
        /// <summary>
        /// Update ShopTire data of production database.
        /// </summary>
        /// <returns>Boolean</returns>
        bool UpdateShopTireProd(Guid imageId, List<ShopTire> shopTire);
        /// <summary>
        /// Check that a shoptire is already exists on production?
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool CheckFileOnProd(ShopTire model);
        /// <summary>
        /// Use to upload an image file on FTP server.
        /// </summary>
        /// <param name="imgName"></param>
        /// <returns></returns>
        bool uploadImagetoFTPServer(string imgName);
        Task<IEnumerable<SelectListItem>> GetStoreNumbersList(int lang,string view);
        Task<IEnumerable<SelectListItem>> GetStoreNumbersEditList(Guid guid,ShopTire shopTire);

        public IEnumerable<ShopTire> GetShopTireByGUID(Guid imageGUID);

        public List<ShopTire> FindByIdList(Guid id);

        //public List<ShopTireDto> GetShopTireListDto(ShopTire shopTire);

        public bool SaveImageDetails(ShopTire shopTire, ref TrackingQueuesDto queuesDto, string mode);
        //GetStoresAll(List<StoreViewModel> storeListPud);
    }
}
