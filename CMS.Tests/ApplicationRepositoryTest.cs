using CMS.Core.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Tests
{
    public class ApplicationRepositoryTest
    {
        [Test]
        public void UpdateScriptQueueByGUID()
        {
            List<tblquecms> tblcmsque = new List<tblquecms>();

            tblcmsque = mockDbContext.GetQueryableMockDbSet<tblquecms>(tblcmsque).Where(x => x.img_guid == Guid.Parse("\"9e87fb8f-c6d4-439e-9c3a-8e170fb6af6d\"")).ToList();

            foreach(var i in tblcmsque)
            {
                if (i.Status == "pending")
                {
                    i.Status = "Completed";
                    i.updated_date = DateTime.Now;

                    
                }
            }
            mockDbContext.GetQueryableMockDbSet<tblquecms>(tblcmsque).UpdateRange();
        }
    }
}
