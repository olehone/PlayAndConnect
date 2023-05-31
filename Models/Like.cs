using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        //[ForeignKey("User1")]
        public int User1Id { get; set; }
        //[ForeignKey("User2")]
        public int User2Id { get; set; }
        public bool User1LikesUser2 { get; set; }
        public bool User2LikesUser1 { get; set; }
        //public bool In
    }
}
