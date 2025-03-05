using Data;

public class GetUsersResponse : ResponseBase
{
    public List<ApplicationUser> Users { get; set; }
}