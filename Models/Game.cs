using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description{get; set;} = " No description ";
        public virtual ICollection<Genre> Genres { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
