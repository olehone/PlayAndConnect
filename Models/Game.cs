using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description{get; set;} = " No description ";
        public ICollection<Genre> Genres { get; set; }
        public ICollection<User>? Users { get; set; }

    }
}
