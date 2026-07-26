using CMS.Core.DTOs;
using CMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Interfaces
{
    public interface IPromoImagesRepository
    {
        /// <summary>
        /// Give a list of all records of promo images.
        /// </summary>
        /// <returns></returns>
        IEnumerable<PromoImages> GetAllPromoImages();
        /// <summary>
        /// Retrieve a list of promo image records for specified conditions.
        /// </summary>
        /// <returns></returns>
        List<PromoImages> GetPromoImageList();
        /// <summary>
        /// Give a specific promo image using ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PromoImages FindById(int id);
        /// <summary>
        /// Give a specific promo image using guid.
        /// </summary>
        /// <param name="guidId"></param>
        /// <returns></returns>
        PromoImages FindByGuidID(Guid guidId);
        /// <summary>
        /// Save a new record of promo image into db.
        /// </summary>
        /// <param name="promo"></param>
        /// <returns></returns>
        bool Add(PromoImages promo);
        /// <summary>
        /// Update a specific promo image record of db.
        /// </summary>
        /// <param name="PromoImageData"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        bool Update(PromoImages PromoImageData, string mode);
        /// <summary>
        /// Delete a promo image record from database.
        /// </summary>
        /// <param name="PromoImageData"></param>
        /// <returns></returns>
        bool Delete(PromoImages PromoImageData);
        /// <summary>
        /// Generate the script query of promo image for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="formMode"></param>
        /// <param name="promo"></param>
        /// <param name="queuesDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        bool SaveScriptAndData(string formMode, PromoImages promo, ref TrackingQueuesDto queuesDto, string user);
        /// <summary>
        /// Add a new record into production table of promo image.
        /// </summary>
        /// <param name="promo"></param>
        /// <returns></returns>
        bool InsertPromoImageProd(PromoImages promo);
        /// <summary>
        /// Update a specific promo image record in production.
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="promo"></param>
        /// <returns></returns>
        bool UpdatePromoImageProd(Guid imageId, PromoImages promo); 
        List<PromoPages> ddlPromoPages(); 
        bool IsAlreadyExists(PromoImages promo);

        public bool CheckFileOnProd(PromoImages model);
    }
}
