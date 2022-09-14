using HITs_classroom.Models.ClassroomAdmin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HITs_classroom.Services
{
    public interface IAuthService
    {
        Task Register(string email);
        Task Login(string token);
        Task Logout();
    }
    public class AuthService: IAuthService
    {
        private readonly UserManager<ClassroomAdmin> _userManager;
        private readonly SignInManager<ClassroomAdmin> _signInManager;

        public AuthService(UserManager<ClassroomAdmin> userManager,
                SignInManager<ClassroomAdmin> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public async Task Login(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            string userEmail = jwtToken.Claims.First(claim => claim.Type == "email").Value;
            var user = await _userManager.FindByNameAsync(userEmail);
           
            if (user == null)
            {
                await Register(userEmail);
                user = await _userManager.FindByEmailAsync(userEmail);
            }

            var claims = new List<Claim>
            {
                new (ClaimTypes.Email, userEmail)
            };

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(2),
                IsPersistent = true
            };

            await _signInManager.SignInWithClaimsAsync(user, authProperties, claims);

        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task Register(string email)
        {
            var classroomAdmin = new ClassroomAdmin
            {
                Email = email,
                UserName = email
            };
            var result = await _userManager.CreateAsync(classroomAdmin);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(classroomAdmin, false);
                return;
            }

            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new ArgumentException(errors);
        }
    }
}
