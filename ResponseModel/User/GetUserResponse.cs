using Data;

public class GetUserResponse : ResponseBase
{
    public ApplicationUser User { get; set; }
    public Employee? Employee { get; set; }
    public Member? Member { get; set; }
}