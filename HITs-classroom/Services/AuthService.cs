using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Util.Store;
using HITs_classroom.Models.TsuAccount;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HITs_classroom.Services
{
    public interface IAuthService
    {
        Task Register(string accountId, string accessToken);
        Task Login(string accountId, string accessToken);
        Task LoginWithPassword(string password);
        Task Logout();
    }
    public class AuthService : IAuthService
    {
        private readonly UserManager<TsuAccountUser> _userManager;
        private readonly SignInManager<TsuAccountUser> _signInManager;
 
        public AuthService(UserManager<TsuAccountUser> userManager,
                SignInManager<TsuAccountUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task Login(string accountId, string accessToken)
        {
            var user = await _userManager.FindByIdAsync(accountId);

            if (user == null)
            {
                await Register(accountId, accessToken);
                user = await _userManager.FindByIdAsync(accountId);
            }

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                IsPersistent = true
            };

            await _signInManager.SignInAsync(user, authProperties);
        }

        public async Task LoginWithPassword(string password)
        {
            var user = await _userManager.FindByIdAsync("admin");
            await _signInManager.PasswordSignInAsync(user, password, false, false);
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task Register(string accountId, string accessToken)
        {
            var tsuUser = new TsuAccountUser();
            tsuUser.Id = accountId;
            var userResult = await _userManager.CreateAsync(tsuUser);
            var tokenResult = await _userManager.SetAuthenticationTokenAsync(tsuUser, "TSU.Account", "AccessToken", accessToken);

            if (userResult.Succeeded && tokenResult.Succeeded)
            {
                await _signInManager.SignInAsync(tsuUser, false);

                return;
            }

            var errors = string.Join(", ", userResult.Errors.Select(x => x.Description), tokenResult.Errors.Select(x => x.Description));
            throw new ArgumentException(errors);
        }
    }
}