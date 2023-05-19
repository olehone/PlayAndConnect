using System;
using System.ComponentModel.DataAnnotations;


namespace PlayAndConnect.Models
{
    public class User
    {
        public int Id{get;set;}
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public UserInfo Info {get; set;}
        //public ICollection<Game>? Games {get; set;} 
        //public virtual ICollection<Like> Likes { get; set; }
        //public virtual ICollection<Chat> Chats { get; set; }
        //public virtual ICollection<Game> Games { get; set; }
        //public virtual ICollection<Message> Messages { get; set; }
    }
}
