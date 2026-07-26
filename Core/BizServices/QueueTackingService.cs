using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Core.BizServices
{
    public class QueueTackingService<T> where T : class
    {
        public void GenericScricptTrackingQueue(T column, T value)
        {
			try
			{
				var scriptQuery = "INSERT INTO ";

				//foreach (var col in column)
				//{
				//	scriptQuery = scriptQuery + column.ToString();
				//}
			}
			catch (Exception ex)
			{

				throw;
			}
        }
    }
}
