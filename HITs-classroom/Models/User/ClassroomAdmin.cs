using HITs_classroom.Models.Course;
using Microsoft.AspNetCore.Identity;

namespace HITs_classroom.Models.User
{
    public class ClassroomAdmin : IdentityUser
    {
        public string Email { get; set; }
    }
}
