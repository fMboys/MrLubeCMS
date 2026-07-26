using Core.DTOs;
using Core.Entities;
using Microsoft.Extensions.Primitives;
using MrLubeCMS.Models;
using MrLubeCMS.Models.DTO;
using System;

namespace MrLubeCMS.Repositories.Abstract
{
    public interface IbannerService
    {
        IEnumerable<banners> GetAllBanner();
        void Add(banners banners);  
        List<bannerModel> GetAllBannerList(banners bannerMode);
        void SaveQueData(banners banner1, ref tblquecmsimageModel imgIddata);

        void FtpUploadImage(IFormFile imgFile);
       void Getimage(int img, ref bannerModel modelBanner);

        void GetImgbyId(int ImgId,ref bool uploadedimg);

        public banners FindbyId(int id);

        public Boolean Edit(bannerModel model);



    }
}
