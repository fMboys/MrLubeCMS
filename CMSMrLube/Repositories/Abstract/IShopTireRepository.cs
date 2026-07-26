using MrLubeCMS.Models;

namespace MrLubeCMS.Repositories.Abstract
{
    public interface IShopTireRepository
    {
        IEnumerable<ShopTire> GetAllShopTires();
        List<ShopTire> GetShopTireList(ShopTire shopTire);
        ShopTire FindById(int id);
        bool Update(ShopTire shopTireData);
    }
}
