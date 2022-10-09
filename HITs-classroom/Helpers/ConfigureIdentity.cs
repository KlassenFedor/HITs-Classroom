using HITs_classroom.Models.Password;
using HITs_classroom.Models.TsuAccount;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace HITs_classroom.Helpers
{
    public static class ConfigureIdentity
    {
        public static async Task ConfigureIdentityAsync(this WebApplication app)
        {
            using var serviceScope = app.Services.CreateScope();
            var userManager = serviceScope.ServiceProvider.GetService<UserManager<TsuAccountUser>>();
            var adminUser = await userManager.FindByIdAsync("admin");
            if (adminUser == null)
            {
                StreamReader streamReader = new StreamReader("./Keys/admin-password.json");
                string jsonString = streamReader.ReadToEnd();
                PasswordModel? password = JsonConvert.DeserializeObject<PasswordModel>(jsonString);
                if (password == null)
                {
                    throw new InvalidOperationException("Unable to create admin user");
                }
                var userResult = await userManager.CreateAsync(new TsuAccountUser
                {
                    Id = "admin",
                    UserName = "admin"
                }, password.Password);
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException("Unable to create admin user");
                }
            }
        }

    }
}
