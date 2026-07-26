using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Core.Entities
{
    [Table("users_manage")]
    public class users_manage
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public string? last_user { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }

        public IEnumerable<users_manage> GetUsers()
        {
            return new List<users_manage>();
        }
    }
}
