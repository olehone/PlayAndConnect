using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayAndConnect.Models;

namespace PlayAndConnect.Data.Interfaces
{
    public class CreatingMessage: Message
    {
        public CreatingMessage(Chat chat, User? user, string text)
        {
            TimeOfSending = DateTime.Now;
            Chat = chat;
            User = user;
        }
    }
}