using CMS.Core.DTOs;
using CMS.Core.Entities;

namespace CMS.Core.Interfaces
{
    public interface ILeftAdRepository
    {
        /// <summary>
        /// Save the left ad data for all the selected pages.
        /// </summary>
        /// <param name="selectedPages"></param>
        /// <param name="leftAd"></param>
        /// <returns></returns>
        TrackingQueuesDto Add(string selectedPages, LeftAd leftAd);
        /// <summary>
        /// Get a left ad image data based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        LeftAd FindByGuid(Guid guid);
        /// <summary>
        /// Get a left ad data based on ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        LeftAd FindByID(int id);
        LeftAd GetActiveLeftAdPage(string urlKey, string viewDevice, string language);

        /// <summary>
        /// Retrieve all the pages that have active left ad images.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        IEnumerable<LeftAd> GetAllLeftAdPages(int lang,string view);
        /// <summary>
        /// Get a list of all the data of left ads from database.
        /// </summary>
        /// <returns>IEnumerable of leftAds</returns>
        /// <exception cref="Exception"></exception>
        IEnumerable<LeftAd> GetAllLeftAds();
        /// <summary>
        /// Retieve list of pages against a specific left ad image.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        List<SubMenu> GetLeftAdCheckedPages(Guid GUID);
        /// <summary>
        /// Retreive all the records of left ad against the given GUID.
        /// </summary>
        /// <param name="imageGUID"></param>
        /// <returns></returns>
        IEnumerable<LeftAd> GetLeftAdsByGUID(Guid imageGUID);
        /// <summary>
        /// Add all the pages of a left ad image into Production's table.
        /// </summary>
        /// <param name="leftAds"></param>
        /// <returns></returns>
        bool InsertLeftAdProd(List<LeftAd> leftAds);
        /// <summary>
        /// Generate the script query of left ad for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="formMode"></param>
        /// <param name="leftAd"></param>
        /// <param name="queuesDto"></param>
        /// <returns></returns>
        bool SaveScriptAndData(string formMode, LeftAd leftAd, ref TrackingQueuesDto queuesDto, string user);
        /// <summary>
        /// Add new left ad data of selected pages for existing guid.
        /// </summary>
        /// <param name="selectedPages"></param>
        /// <param name="leftAdData"></param>
        /// <returns></returns>
        TrackingQueuesDto Update(string selectedPages, LeftAd leftAdData);
        /// <summary>
        /// Changed status to delete of all records against given guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool UpdateLeftAdByGUID(Guid GUID);
        /// <summary>
        /// Delete all the pages of a left ad by using guid and Add the newely selected image and pages for that guid in Production.
        /// </summary>
        /// <param name="leftAds"></param>
        /// <param name="GUID"></param>
        /// <returns>bool</returns>
        /// <exception cref="Exception"></exception>
        bool UpdateLeftAdsProd(List<LeftAd> leftAds, Guid GUID);

        public bool CheckFileOnProd(LeftAd model);
    }
}
