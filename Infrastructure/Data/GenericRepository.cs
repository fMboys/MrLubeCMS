using CMS.Core.Interfaces;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Infrastructure.Data
{
    //InCompelete class logic
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private IGenericRepository<T> repository = null;

        //public GenericRepository()
        //{
        //    this.repository = new GenericDAL<T>();
        //}

        public GenericRepository(IGenericRepository<T> repository)
        {
            this.repository = repository;
        }

        public IEnumerable<T> SelectAll()
        {
            return repository.SelectAll();
        }

        public void Insert(T obj)
        {
            repository.Insert(obj);
            repository.Save();
        }
        
        public T SelectByID(object id)
        {
            return repository.SelectByID(id);
        }

        public void Update(T obj)
        {
            throw new NotImplementedException();
        }

        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void UpdateMulti(T obj)
        {
            throw new NotImplementedException();
        }
    }
}
