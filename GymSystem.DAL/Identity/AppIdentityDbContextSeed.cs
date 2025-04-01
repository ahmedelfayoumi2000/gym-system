using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.DAL.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            await SeedAdminUserAsync(userManager);

        }

        private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
        {
            var adminUser = new AppUser
            {
                DisplayName = "Mohamed",
                UserName = "MohamedSalahadmin",
                Email = "mohamedbedosalah2003@gmail.com",
                PhoneNumber = "01093422099",
                Gender = "Male",
                Age = 22,
                EmailConfirmed = true,
                IsProfileConfirmed = true,
                Address = new Address
                {
                    Country = "Egypt",
                    City = "Menouf",
                    Street = "Tarek Barhim"
                },

                UserCode = GenerateUserCode("Admin", 1)
            };

            const string adminPassword = "Pa$$w0rd123!";
            const string adminRole = "Admin";

            // التحقق من عدم وجود المستخدم مسبقًا
            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create Admin user: {errors}");
                }

                var roleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(adminUser); // Rollback إذا فشل تعيين الدور
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to assign 'Admin' role to user: {errors}");
                }
            }
        }


        private static string GenerateUserCode(string role, int userCount)
        {
            return $"{role.Substring(0, 2).ToUpper()}-{DateTime.UtcNow.ToString("yyMMdd")}-{userCount}";
        }





    }
}