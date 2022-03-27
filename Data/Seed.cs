using Microsoft.AspNetCore.Identity;
using StockBand.Models;

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
                if (!context.RoleDbContext.Any())
                {
                    var roles = new List<Role>()
                    {
                        new Role(){Name = "User"},
                        new Role(){Name = "Admin"}
                    };
                    context.RoleDbContext.AddRange(roles);
                    context.SaveChanges();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("RoleDbContext seed successfully created");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("The RoleDbContext has been already created");
                    Console.ResetColor();
                }
                if (!context.UserDbContext.Any())
                {
                    var user = new User()
                    {
                        Name = "admin741useruser",
                        RoleId = 2
                    };
                    var hasher = new PasswordHasher<User>();
                    var pwdHash = hasher.HashPassword(user, "nmK6BWH5,,$D>");
                    user.HashPassword = pwdHash;
                    context.UserDbContext.Add(user);
                    context.SaveChanges();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("UserDbContext seed successfully created");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("The UserDbContext has been already created");
                    Console.ResetColor();
                }
            }
        }
    }
}
