namespace Data
{
    public class Member
    {
        public int Id { get; set;}

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}