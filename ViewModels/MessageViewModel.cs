using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayAndConnect.ViewModels
{
    public class MessageViewModel
    {
        public bool IsOwn { get; set; }
        public bool IsSystem { get; set; }
        public string Text { get; set; }
        public string Time { get; set; }
    }
}