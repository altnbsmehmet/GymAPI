namespace Data
{
    public class  Subscription
    {
        public int Id { get; set;}
        public DateTime StartDate { get; set;}
        public DateTime EndDate { get; set;}

        public string Status { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public int MembershipId { get; set; }
        public Membership Membership { get; set; }
    }
}