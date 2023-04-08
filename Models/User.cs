using System.ComponentModel.DataAnnotations;
using PlayAndConnect.Data;
namespace PlayAndConnect.Models
{
    public class User
    {
        [Key]
        public uint Id { get; set; }
        public enum Gender Gender{ get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ProfilePictureURL { get; set; }
        public string Bio { get; set; }

    }
}
