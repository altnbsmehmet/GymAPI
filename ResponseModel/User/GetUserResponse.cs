public class GetUserResponse : ResponseBase
{
    public UserDto User { get; set; }
    public EmployeeDto? Employee { get; set; }
    public MemberDto? Member { get; set; }
}