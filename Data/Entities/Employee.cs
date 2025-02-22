namespace Data
{
    public class  Employee
    {
        public int Id { get; set;}
        public string Position { get; set;}
        public int Salary { get; set;}

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}