using System.ComponentModel.DataAnnotations;
 
namespace PlayAndConnect.ViewModels
{
    public class SingupModel
    {
        [Required(ErrorMessage ="Не вказано логін")]
        public string Login{ get; set; }
         
        [Required(ErrorMessage = "Не вказано пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
         
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; }
    }  
}
