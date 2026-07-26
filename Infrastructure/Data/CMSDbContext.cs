using CMS.Core.Entities;
using CMS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Data
{
    public class CMSDbContext : DbContext
    {
        public CMSDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<users_manage> users_manages { get; set; }
        public virtual DbSet<banners> banner { get; set; }
        public virtual DbSet<tblquecms> Tblquecms { get; set; }
        public virtual DbSet<tblquecmsimage> Tblquecmsimage { get; set; }
        public virtual DbSet<ShopTire> ShopTire { get; set; }
        public virtual DbSet<PromoPages> PromoPages { get; set; }
        public virtual DbSet<PromoImages> PromoImages { get; set; }
        public virtual DbSet<CouponPages> CouponPages { get; set; }
        public virtual DbSet<CouponImages> CouponImages { get; set; }
        public virtual DbSet<FloatingImage> floating_images { get; set; }
        public virtual DbSet<SubMenu> sub_menu { get; set; }
        public virtual DbSet<Menu> menu { get; set; }
        public virtual DbSet<ImageSpecification> image_Specs { get; set; }
        public virtual DbSet<LeftAd> ads_images { get; set; }
        public virtual DbSet<Store> store { get; set; }

        
    }
}
