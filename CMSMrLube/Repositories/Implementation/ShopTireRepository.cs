using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using LinqKit;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace MrLubeCMS.Repositories.Implementation
{
    public class ShopTireRepository : IShopTireRepository
    {
        private readonly CMSDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public ShopTireRepository(CMSDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public IEnumerable<ShopTire> GetAllShopTires()
        {
            try
            {
                return _dbContext.ShopTire.ToList().OrderBy(x => x.updated_date);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public ShopTire FindById(int id)
        {
            return _dbContext.ShopTire.Find(id);
        }

        public List<ShopTire> GetShopTireList(ShopTire shopTire)
        {
            try
            {
                var predicate = PredicateBuilder.True<ShopTire>();

                if (!string.IsNullOrEmpty(shopTire.store_num.ToString())) { predicate.And(x => x.store_num.Equals(shopTire.store_num)); }
                if (!string.IsNullOrEmpty(shopTire.title)) { predicate.And(x => x.title.Equals(shopTire.title)); }
                if (!string.IsNullOrEmpty(shopTire.image)) { predicate.And(x => x.image.Equals(shopTire.image)); }
                if (!string.IsNullOrEmpty(shopTire.view_device)) { predicate.And(x => x.view_device.Equals(shopTire.view_device)); }
                if (!string.IsNullOrEmpty(shopTire.image_status)) { predicate.And(x => x.image_status.Equals(shopTire.image_status)); }
                if (!string.IsNullOrEmpty(shopTire.ad_hyperlink)) { predicate.And(x => x.ad_hyperlink.Equals(shopTire.ad_hyperlink)); }

                List<ShopTire> shopTires = (from q in _dbContext.ShopTire.Where(predicate)
                                            select new ShopTire
                                            {
                                                shopTire_id = q.shopTire_id,
                                                language_id = q.language_id,
                                                store_num = q.store_num,
                                                title = q.title,
                                                image = q.image,
                                                view_page = q.view_page,
                                                view_device = q.view_device,
                                                last_user = q.last_user,
                                                image_status = q.image_status,
                                                ad_hyperlink = q.ad_hyperlink
                                            }).ToList();

                return shopTires;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }

        public bool Update(ShopTire shopTireData)
        {
            try
            {                
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        ShopTire shopTire = FindById(shopTireData.shopTire_id);
                        if (shopTire != null)
                        {
                            shopTireData.created_date = shopTire.created_date;
                            shopTireData.updated_date = DateTime.Now;

                            _dbContext.Entry(shopTire).CurrentValues.SetValues(shopTireData);
                            _dbContext.SaveChanges();
                            transaction.Commit();
                        }
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Method Name: " + MethodBase.GetCurrentMethod().DeclaringType.Name, ex);
            }
        }
    }
}
