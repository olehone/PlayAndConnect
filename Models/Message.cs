﻿using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public DateTime TimeOfSending { get; set; }
        public Chat Chat { get; set; }
        public User User { get; set; }
        public string Text { get; set; }
        public Message(Chat chat, User user, string text)
        {
            TimeOfSending = DateTime.Now;
            Chat = chat;
            User = user;
        }
    }
}
