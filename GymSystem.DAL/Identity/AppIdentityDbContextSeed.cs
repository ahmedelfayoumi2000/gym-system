using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.DAL.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);

        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Id = "2", Name = "Trainer", NormalizedName = "TRAINER" },
                    new IdentityRole { Id = "3", Name = "Member", NormalizedName = "MEMBER" },
                    new IdentityRole { Id = "4", Name = "User", NormalizedName = "USER" },
                    new IdentityRole { Id = "5", Name = "Receptionist", NormalizedName = "RECEPTIONIST" }
                };

                foreach (var role in roles)
                {
                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create role {role.Name}: {errors}");
                    }
                }
            }
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
                    await userManager.DeleteAsync(adminUser); 
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