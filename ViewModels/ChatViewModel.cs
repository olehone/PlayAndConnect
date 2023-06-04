using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayAndConnect.ViewModels
{
public class ChatViewModel
{
    public string Login { get; set; }
    public string Message { get; set; }
    public string ImagePath { get; set; }
    public bool IsNew {get;set;}
    public bool IsSelected { get; set; }
    public DateTime TimeOfLastMessage{get;set;}
}
}