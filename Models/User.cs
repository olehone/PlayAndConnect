using System;
using System.ComponentModel.DataAnnotations;


namespace PlayAndConnect.Models
{
    public class User
    {
        //[Key]
        public int Id{get;set;}
        public string Login { get; set; }
        public string? Name { get; set; }
        public string PasswordHash { get; set; }
        
        public string? ImgURL { get; set; }
        //public virtual ICollection<Like> Likes { get; set; }
        //public virtual ICollection<Chat> Chats { get; set; }
        //public virtual ICollection<Game> Games { get; set; }
        //public virtual ICollection<Message> Messages { get; set; }
    }
}
