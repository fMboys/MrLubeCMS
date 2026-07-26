using CMS.Core.DTOs;
using CMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Interfaces
{
    public interface ICouponImagesRepository
    {
        IEnumerable<CouponImages> GetAllCouponImages();
        List<CouponImages> GetCouponImageList();
        CouponImages FindById(int id);
        CouponImages FindByGuidID(Guid guidId);
        bool Add(CouponImages coupon);
        bool Update(CouponImages CouponImageData, string mode);
        bool Delete(CouponImages CouponImageData);
        bool SaveScriptAndData(string formMode, CouponImages coupon, ref TrackingQueuesDto queuesDto, string user);
        bool InsertCouponImageProd(CouponImages coupon);
        bool UpdateCouponImageProd(Guid imageId, CouponImages coupon);
        List<CouponPages> ddlCouponPages();
        bool IsAlreadyExists(CouponImages coupon);

        bool CheckFileOnProd(CouponImages model);
        //List<CouponPages> ddlCouponPagesSelected(Guid guid);

        //public bool checkCouponURL(Guid id, ref CouponPages couponPages);
    }
}
