using Microsoft.AspNetCore.Identity;

namespace HITs_classroom.Models.TsuAccount
{
    public class TsuAccountUser: IdentityUser
    {
        public string Id { get; set; }
    }
}
