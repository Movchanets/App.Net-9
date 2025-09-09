namespace Infrastructure.Data.Models;

public class UserVM
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Image { get; set; }
    public bool IsBlocked { get; set; }
    public string PhoneNumber { get; set; }
    public virtual List<string> UserRoles { get; set; }
}