using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    public static string HashPassword(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public static string GenerateSalt()
    {
        byte[] saltBytes = new byte[32];
        RandomNumberGenerator.Fill(saltBytes);  
        return Convert.ToBase64String(saltBytes);
    }
}
