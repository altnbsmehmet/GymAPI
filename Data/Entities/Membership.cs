namespace Data
{
    public class  Membership
    {
        public int Id { get; set;}
        public string Type { get; set;} = string.Empty;
        public int Duration { get; set;}
        public int Price { get; set;}
        public bool IsActive { get; set; }
    }
}