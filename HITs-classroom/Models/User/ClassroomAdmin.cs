using Microsoft.AspNetCore.Identity;

namespace HITs_classroom.Models.ClassroomAdmin
{
    public class ClassroomAdmin : IdentityUser
    {
        public string Email { get; set; }
    }
}
