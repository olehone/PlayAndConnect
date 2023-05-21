using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using PlayAndConnect.Models;
public class UserInfo
{
    public int Id { get; set; }
    [Range(3, 100)]
    public int Age { get; set; } = 18;
    public string? Name { get; set; }
    public string? ImgURL { get; set; }
    public User User { get; set; }
    public int UserId { get; set;}
}