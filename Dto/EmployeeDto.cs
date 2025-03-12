public class EmployeeDto
{
        public int? Id { get; set;}
        public string? Position { get; set;}
        public int? Salary { get; set;}
        public string? AboutMe { get; set; }

        public string? UserId { get; set; }
        public UserDto? User { get; set; }
}
