using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
