using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
