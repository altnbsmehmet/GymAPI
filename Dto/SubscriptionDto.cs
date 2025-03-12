public class SubscriptionDto
{
    public int? Id { get; set;}
    public DateTime? StartDate { get; set;}
    public DateTime? EndDate { get; set;}

    public string? Status { get; set; }

    public int? MemberId { get; set; }
    public MemberDto? Member { get; set; }

    public int? MembershipId { get; set; }
    public MembershipDto? Membership { get; set; }
}
