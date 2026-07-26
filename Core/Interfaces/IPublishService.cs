using CMS.Core.DTOs;
using CMS.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace CMS.Core.Interfaces
{
    public interface IPublishService
    {
        /// <summary>
        /// Give list of all the pending records that should be publish on production.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<trackQueModel> GetAllPublish();
        /// <summary>
        /// Give list of all the pending records that should be publish on production.
        /// </summary>
        /// <param name="publishList"></param>
        /// <returns></returns>
        public IEnumerable<trackQueModel> PublishOnProd(List<trackQueModel> publishList);
        /// <summary>
        /// Change the status of script and image queues records to complete.
        /// </summary>
        /// <param name="quecmsId"></param>
        /// <param name="queimgId"></param>
        /// <returns></returns>
        public bool Updatetbltracking(int? quecmsId, int? queimgId);
        public List<tblquecmsModel> GetLeftAdQue(Guid id);

        public bool Updatetblcmsque(int tblquecmsId);


    }
}
