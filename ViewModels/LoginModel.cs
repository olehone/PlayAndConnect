using System.ComponentModel.DataAnnotations;
namespace PlayAndConnect.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не вказано логін")]
        public string Login { get; set; }
         
        [Required(ErrorMessage = "Не вказано пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}