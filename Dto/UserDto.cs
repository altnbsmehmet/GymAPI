public class SignUpDto
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public IFormFile? ProfilePhoto { get; set; }


    //employee
    public string? Position { get; set; }
    public int? Salary { get; set; }

    //member

}