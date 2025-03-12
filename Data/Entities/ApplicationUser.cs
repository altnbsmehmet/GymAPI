using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string? ProfilePhotoPath { get; set; }

        [NotMapped]
        public string ProfilePhoto { get; set; }
    }
}