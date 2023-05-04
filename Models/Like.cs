using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public bool FirstLikeLast { get; set; }
        public bool LastLikeFirst { get; set; }
    }
}
