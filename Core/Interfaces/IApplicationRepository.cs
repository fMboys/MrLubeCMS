using CMS.Core.DTOs;
using CMS.Core.Entities;

namespace CMS.Core.Interfaces
{
    public interface IApplicationRepository
    {
        tblquecmsimage GetImageDetailByGUID(Guid imageGUID);
        bool UpdateImageDetailByGUID(Guid imageGUID);
        bool UpdateScriptQueueDetailByGUID(Guid imageGUID);
        bool CheckImageQueue();

        bool CheckStoreExist(string storeNo, int lang, string device, int shopTire_id,ref string comastores);
        public List<SubMenu> GetSubMenus(int lang,string view);
        bool VerifyNameImageQueue(string fileName);
        List<SubMenu> GetCheckedPages(Guid GUID);
        bool RemoveQueueImageByGuid(Guid GUID);
        bool RemoveAllQueueScriptsByGuid(Guid GUID);

        bool isFilependingbanner(Guid id,string bannerType,ref List<tblquecmsimage> tblquecms);

        public tblquecmsimage RemoveImgQueData(int id);

        public bool SaveQueDataWithnoImage(banners banner1, ref tblquecmsimage imgIddata, ref tblquecms tblimgqry, string formMode);

        //bool VerifyNameImageQueue(string fileName);

        bool CheckFileOnProd(Guid id);

        public bool CheckImageQueue(Guid guid);
        tblquecmsimage GetImageDetailByID(Guid imageID, int imageQueueId);

        bool UpdateImageDetailByID(Guid imageID, int imageQueueId);
        bool UpdateScriptQueueDetailByID(Guid imageID, int scriptQueueId);
        List<ImageSpecification> GetAllImagesSpecifications();
        bool SaveQueueScriptAndData(string mode, GeneralCMSDto generalDto, ref TrackingQueuesDto queuesDto);
        string GetBlobFolderPathByBanner(string bannerType);
        List<SubMenu> GetSelectedSubMenus(string selectedMenus);

        public bool UpdateShoptireImageQueByGUID(Guid imageGUID);

        public bool RemoveShopTireQueueImageByGuid(Guid GUID);
        public bool UpdateImageDetailBycmsqueId(Guid imageID, int imageQueueId);
    }
}
