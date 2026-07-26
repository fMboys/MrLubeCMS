using CMS.Core.DTOs;
using CMS.Core.Entities;

namespace CMS.Core.Interfaces
{
    public interface IFloatingImageRepository
    {
        /// <summary>
        /// Save all the pages and data of a floating image.
        /// </summary>
        /// <param name="selectedPages"></param>
        /// <param name="floatingImage"></param>
        /// <returns></returns>
        TrackingQueuesDto Add(string selectedPages, FloatingImage floatingImage);
        /// <summary>
        /// Get a floating image data based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        FloatingImage FindByGuid(Guid guid);
        /// <summary>
        /// Get a list of all the data of floating images from database.
        /// </summary>
        /// <returns>IEnumerable of FloatingImage</returns>
        /// <exception cref="Exception"></exception>
        IEnumerable<FloatingImage> GetAllFloatingImages();
        /// <summary>
        /// Retrieve a list of records for floating images.
        /// </summary>
        /// <param name="floatingImage"></param>
        /// <returns></returns>
        List<FloatingImage> GetFloatingImageList(FloatingImage floatingImage);
        /// <summary>
        /// Give all the data of floating image for a specific guid.
        /// </summary>
        /// <param name="imageGUID"></param>
        /// <returns></returns>
        IEnumerable<FloatingImage> GetFloatingImagesByGUID(Guid imageGUID);
        /// <summary>
        /// Add all the pages of a floating image into Production's table.
        /// </summary>
        /// <param name="floatingImages"></param>
        /// <returns></returns>
        bool InsertFloatingImageProd(List<FloatingImage> floatingImages);
        /// <summary>
        /// Delete all the floating images from Production server using guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        bool RemoveAllFloatingImageProdByGUID(Guid GUID);
        /// <summary>
        /// Add new floating image data for newely selected pages against existing guid.
        /// </summary>
        /// <param name="selectedPages"></param>
        /// <param name="floatingImageData"></param>
        /// <returns></returns>
        TrackingQueuesDto Update(string selectedPages, FloatingImage floatingImageData);
        /// <summary>
        /// Changed status to delete of all records against given guid.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool UpdateFloatingImageByGUID(Guid GUID);
        /// <summary>
        /// Delete all the pages of a floating image by using guid and Add the newely selected image and pages for that guid in Production.
        /// </summary>
        /// <param name="floatingImages"></param>
        /// <param name="GUID"></param>
        /// <returns>bool</returns>
        /// <exception cref="Exception"></exception>
        bool UpdateFloatingImageProd(List<FloatingImage> floatingImages, Guid GUID);
        /// <summary>
        /// Generate the script query of floating image for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="formMode"></param>
        /// <param name="floatingImage"></param>
        /// <param name="queuesDto"></param>
        /// <returns></returns>
        bool SaveScriptAndData(string formMode, FloatingImage floatingImage, ref TrackingQueuesDto queuesDto, string user);
        /// <summary>
        /// Retrieve all the pages that have active floating images.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        IEnumerable<FloatingImage> GetAllFloatingImagePages(int lang,string view);
        /// <summary>
        /// Get a floating image data based on ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Floating Image object</returns>
        /// <exception cref="Exception"></exception>
        FloatingImage FindByID(int id);
        /// <summary>
        /// Retrieve data of a page that is already have an active floating image.
        /// </summary>
        /// <param name="urlKey"></param>
        /// <param name="viewDevice"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        FloatingImage GetActiveFloatingImagePage(string urlKey, string viewDevice, string language);

        public bool CheckFileOnProd(FloatingImage model);
    }
}
