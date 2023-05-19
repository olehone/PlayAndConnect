using System.Security.Cryptography;
using System.Text;
namespace PlayAndConnect.Data
{
    public static class Hashing
    {
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // перетворення пароля в масив байтів
            byte[] bytes = Encoding.UTF8.GetBytes(password);
        
            // обчислення хеш-функції
            byte[] hash = sha256.ComputeHash(bytes);
        
            // конвертування хеш-коду в строку шістнадцяткового формату
            StringBuilder sb = new StringBuilder();
        
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
        
            return sb.ToString();
        }
    }
    }
}