namespace app.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
    public bool IsAdmin { get; set; }
}