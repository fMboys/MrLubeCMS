using CMS.Core.DTOs;
using CMS.Core.Entities;
using CMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Infrastructure.Data
{
    public class PublishService : IPublishService
    {
        private readonly CMSDbContext _context;
        public readonly IConfiguration _configuration;

        public PublishService(CMSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IEnumerable<trackQueModel> GetAllPublish()
        {
            var tblque = new List<trackQueModel>();
             var tblques = (from tp in _context.Tblquecmsimage
                          join ts
                         in _context.Tblquecms on tp.tblquecms_id equals ts.que_id
                            //where tp.img_id == ts.img_id 
                            select new trackQueModel
                         {
                             img_id = tp.img_id,
                             tblquecms_id = ts.que_id,
                             tblquecmsimage_id = tp.img_queId,
                             img_name = tp.img_name,
                             img_uploadPath = tp.img_uploadPath,
                             banner_type = tp.banner_type,
                             action_done = tp.Action,
                             que_script = ts.que_script,
                             status = tp.Status,
                             img_updatedDate = ts.updated_date
                         }).ToList();
            foreach(var que in tblques)
            {
                tblque.Add(que);
            }
            
            var rcount = tblques.Count();
            //tblque.OrderBy(a=>a.img_updatedDate).ToList();
            return tblque;
        }

        public IEnumerable<trackQueModel> PublishOnProd(List<trackQueModel> publishList)
        {
            var tblque = new List<trackQueModel>();
            var tblques = (from tp in _context.Tblquecmsimage
                           join ts
                          in _context.Tblquecms on tp.tblquecms_id equals ts.que_id
                           //where tp.img_id == ts.img_id
                           select new trackQueModel
                           {
                               guid = ts.img_guid,
                               img_id = tp.img_id,
                               tblquecms_id = ts.que_id,
                               tblquecmsimage_id = tp.img_queId,
                               img_name = tp.img_name,
                               img_uploadPath = tp.img_uploadPath,
                               banner_type = tp.banner_type,
                               action_done = tp.Action,
                               que_script = ts.que_script,
                               status = tp.Status,
                               img_updatedDate = ts.updated_date
                           }).ToList();
            foreach (var que in tblques)
            {
                publishList.Add(que);
            }

            var rcount = tblques.Count();
            //tblque.OrderBy(a=>a.img_updatedDate).ToList();
            return publishList;
        }

        public List<tblquecmsModel> GetLeftAdQue(Guid id)
        {
            var que = (from tq in _context.Tblquecms where tq.img_guid == id 
                 select new tblquecmsModel
                 {
                     que_id = tq.que_id,
                     img_id = tq.img_id,
                     img_guid = tq.img_guid,
                     que_script = tq.que_script,
                     Status = tq.Status

                 }).ToList();
            que = que.Where(a => a.Status == "pending").ToList();
            return que;
        }

        public bool Updatetbltracking(int? quecmsId, int? queimgId)
        {
            var quedata = FindquecmsById(quecmsId);
            
            if (quedata != null && quedata.Status == "pending")
            {
                quedata.Status = "completed";
                quedata.updated_date = DateTime.Now;
                //_context.Add(imgsata);
                _context.SaveChanges();
            }
            var trackque = FindQueImgByid(queimgId);
            if (trackque != null && trackque.Status == "pending")
            {
                trackque.Status = "completed";
                trackque.img_updatedDate = DateTime.Now;
                _context.SaveChanges();
            }
            return true;
        }

        public bool Updatetblcmsque(int tblquecmsId)
        {
            var tblcmsqueData = FindquecmsById(tblquecmsId);
            if (tblcmsqueData != null && tblcmsqueData.Status == "pending")
            {
                tblcmsqueData.Status = "completed";
                tblcmsqueData.updated_date = DateTime.Now;
                //_context.Add(imgsata);
                _context.SaveChanges();
            }
            return true;
        }

        tblquecms? FindquecmsById(int? id)
        {
            return _context.Tblquecms.Where(x => x.que_id == id).FirstOrDefault();
        }

        tblquecmsimage? FindQueImgByid(int? id)
        {
            return _context.Tblquecmsimage.Where(x=>x.img_queId == id).FirstOrDefault();
        }
    }
}
