using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public enum Genres
    {
        ACTION,
        ADVENTURE,
        ROLE_PLAYING_GAME,
        STRATEGY,
        SHOOTER,
        SPORTS,
        SIMULATION,
        FIGHTING,
        PUZZLE,
        PLATFORMER,
        RACING,
        MMORPG,
        SURVIVAL,
        HORROR,
        STEALTH,
        OPEN_WORLD,
        SANDBOX,
        EDUCATIONAL,
        MUSIC_RHYTHM,
        VISUAL_NOVEL
    }
    public class Genre
    {
        [Key]
        public int Id { get; set; }
        public string Name {get; set;}
        public Genres GameGenre { get; set; }
        public virtual ICollection<Game> Games { get; set; }
    }

}
