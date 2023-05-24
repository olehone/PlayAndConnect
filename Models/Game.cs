using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace PlayAndConnect.Models
{
    [JsonObject]
    public class Game
    {
        [Key]
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string Title { get; set; }
        public string Description{get; set;} = " No description ";
        public Genre Genre { get; set; }
        public ICollection<User>? Users { get; set; }

    }
}
