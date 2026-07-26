using CMS.Core.DTOs;
using CMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Interfaces
{
    public interface IPromosRepository
    {
        /// <summary>
        /// Give a list of all records of promo pages.
        /// </summary>
        /// <returns></returns>
        IEnumerable<PromoPages> GetAllPromos();
        /// <summary>
        /// Retrieve a list of promo pages records for specified conditions.
        /// </summary>
        /// <returns></returns>
        List<PromoPages> GetPromoPagesList();
        /// <summary>
        /// Give a specific promo page record using ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PromoPages FindById(int id);
        /// <summary>
        /// Save a new record of promo page into db.
        /// </summary>
        /// <param name="Promo"></param>
        /// <returns></returns>
        bool Add(PromoPages Promo);
        /// <summary>
        /// Update a specific promo page record of db.
        /// </summary>
        /// <param name="promo"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        bool Update(PromoPages promo, string mode);
        /// <summary>
        /// Delete a promo page record from database.
        /// </summary>
        /// <param name="promo"></param>
        /// <returns></returns>
        bool Delete(PromoPages promo);
        /// <summary>
        /// Generate the script query of promo page for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="formMode"></param>
        /// <param name="promo"></param>
        /// <param name="queuesDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        bool SaveScriptAndData(string formMode, PromoPages promo, ref TrackingQueuesDto queuesDto, string user);
        /// <summary>
        /// Add a new record into production table of promo page.
        /// </summary>
        /// <param name="promo"></param>
        /// <returns></returns>
        bool InsertPromoProd(PromoPages promo);
        /// <summary>
        /// Update a specific promo page record in production.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="guid"></param>
        /// <param name="promo"></param>
        /// <returns></returns>
        bool UpdatePromoProd(int language, Guid guid, PromoPages promo);
        /// <summary>
        /// Retrieve the greatest item id of promo page.
        /// </summary>
        /// <returns></returns>
        int MaxItemId();
        /// <summary>
        /// Retrieve a list of promo pages using item id.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        List<PromoPages> FindByItemId(int itemID);
        /// <summary>
        /// Retrieve a list of promo pages using item guid.
        /// </summary>
        /// <param name="guidId"></param>
        /// <returns></returns>
        List<PromoPages> FindByGuidID(Guid guidId);
        /// <summary>
        /// Check the existing title of a promo page.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="EngTitle"></param>
        /// <param name="FrTitle"></param>
        /// <returns></returns>
        List<PromoPages> TitleExists(Guid guid , string EngTitle, string FrTitle);

        public PromoPages FindByLangGuid(Guid guidId);

        public bool CheckFileOnProd(PromoPages model);
    }
}
