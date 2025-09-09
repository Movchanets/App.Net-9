namespace Application.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";

    public UserDto(long id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
    }
}