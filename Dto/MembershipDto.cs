public class MembershipDto
{
        public int? Id { get; set;}
        public string? Type { get; set;}
        public int? Duration { get; set;}
        public int? Price { get; set;}
        public bool? IsActive { get; set; }
        public List<MemberDto>? Subscribers { get; set; }
}