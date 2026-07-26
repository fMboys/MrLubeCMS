using Infrastructure.Data;

namespace MrLubeCMS.Repositories.Implementation
{
    public class appuserService
    {
        private readonly CMSDbContext _context;

        public appuserService(CMSDbContext context)
        {
            _context = context; 
        }

        public void Getuser(string email)
        {
            var eml =  _context.users_manages.Where(a=>a.Email == email).SingleOrDefault();
        }
    }
}
