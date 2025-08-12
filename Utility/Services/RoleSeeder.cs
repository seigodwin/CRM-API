using CRMApi.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace CRMApi.Utility.Services
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public RoleSeeder(RoleManager<IdentityRole> roleManager,  UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }


        string[] roles = new string[] { "Admin", "Employee" };

        public async Task SeedRolesAsync()
        {
            foreach(var role in roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                     await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        
    }
}
