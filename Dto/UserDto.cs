public class UserDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }


    //employee
    public string? Position { get; set; }
    public int? Salary { get; set; }

    //member

}
