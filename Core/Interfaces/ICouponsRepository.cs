using CMS.Core.DTOs;
using CMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.Interfaces
{
    public interface ICouponsRepository
    {
        IEnumerable<CouponPages> GetAllCoupons();
        List<CouponPages> GetCouponPagesList();
        CouponPages FindById(int id);
        bool Add(CouponPages Coupon);
        bool Update(CouponPages coupon, string mode);
        bool Delete(CouponPages coupon);
        bool SaveScriptAndData(string formMode, CouponPages coupon, ref TrackingQueuesDto queuesDto, string user);
        bool InsertCouponProd(CouponPages coupon);
        bool UpdateCouponProd(int language, Guid guid, CouponPages coupon);
        int MaxItemId();
        List<CouponPages> FindByItemId(int itemID);
        List<CouponPages> FindByGuidID(Guid guidId);
        List<CouponPages> TitleExists(Guid guid, string EngTitle, string FrTitle);

        public bool CheckFileOnProd(CouponPages model);
        public CouponPages FindByLangGuid(Guid guidId);
    }
}
