using CMS.Core.DTOs;
using CMS.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace CMS.Core.Interfaces
{
    public interface IbannerService
    {
        /// <summary>
        /// Give a list of all banners.
        /// </summary>
        /// <returns></returns>
        IEnumerable<banners> GetAllBanner();
        /// <summary>
        /// Save a new banner's data into database.
        /// </summary>
        /// <param name="banners"></param>
        /// <returns></returns>
        bool Add(banners banners);
        /// <summary>
        /// Retrieve a list of banners for specified conditions.
        /// </summary>
        /// <param name="bannerMode"></param>
        /// <returns></returns>
        List<bannerModel> GetAllBannerList(banners bannerMode);
        /// <summary>
        /// Generate the script query of home banner for production table and save the relevant data into script queue table.
        /// </summary>
        /// <param name="banner1"></param>
        /// <param name="imgIddata"></param>
        /// <param name="tblimgqry"></param>
        /// <param name="formMode"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        bool SaveQueData(banners banner1, ref tblquecmsimage imgIddata,ref tblquecms tblimgqry, string formMode, string user);

        //bool SaveQueDataDel(int id, string formMode);
        /// <summary>
        /// Upload a home banner on FTP server.
        /// </summary>
        /// <param name="imgFile"></param>
        void FtpUploadImage(IFormFile imgFile);

        void Getimage(Guid img, ref bannerModel modelBanner);
        /// <summary>
        /// Get a specific home banner and upload it to FTP Server and add the entry into image queue.
        /// </summary>
        /// <param name="ImgId"></param>
        /// <param name="tblque"></param>
        /// <param name="tblimgque"></param>
        /// <param name="uploadedimg"></param>
        void GetImgbyId(Guid ImgId, int tblque, int tblimgque, ref bool uploadedimg);
        /// <summary>
        /// Find a banner for specific ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public banners FindbyId(Guid id);
        /// <summary>
        /// Delete a banner for specific ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemovebyId(Guid id);
        /// <summary>
        /// Update a specific record of banner.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Boolean Edit(banners model);
        /// <summary>
        /// Give number of exisiting banner records in database.
        /// </summary>
        /// <returns></returns>
        public int GetBannerCount();
        /// <summary>
        /// Give list of all the banners.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<banners> GetAll();
        /// <summary>
        /// Check that a banner is already exists in production table.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool CheckFileOnProd(banners model);
    }
}
