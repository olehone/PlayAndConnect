using System;
using System.ComponentModel.DataAnnotations;


namespace PlayAndConnect.Models
{
    public class UserGame
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}