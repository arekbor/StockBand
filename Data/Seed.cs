using Microsoft.AspNetCore.Identity;
using StockBand.Models;
using StockBand.Services;

namespace StockBand.Data
{
    public class Seed
    {
        public static void SeedData(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                if (!context.UserDbContext.Any())
                {
                    var user = new User()
                    {
                        Name = ConfigurationHelper.config.GetSection("DefaultAdminName").Value,
                        Role = UserRoles.Roles[1]
                    };
                    var hasher = new PasswordHasher<User>();
                    var pwdHash = hasher.HashPassword(user, ConfigurationHelper.config.GetSection("DefaultAdminPassword").Value);
                    user.HashPassword = pwdHash;
                    user.CreatedTime = DateTime.Now;
                    context.UserDbContext.Add(user);
                    context.SaveChanges();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(SystemMessage.Code01("User"));
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(SystemMessage.Code02("User"));
                    Console.ResetColor();
                }
            }
        }
    }
}
