using Azure.Identity;
using BookShop.DataAccess.Data;
using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        public void Initialize()
        {
            // migrations if they are not applied
            try
            {
                if(_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch(Exception ex)
            {

            }

            // create roles if they are not created
            if (!_roleManager.RoleExistsAsync(StaticDetails.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Employee)).GetAwaiter().GetResult();

                // if roles are not created, then we will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "John Doe",
                    PhoneNumber = "1234567890",
                    StreetAddress = "street addredd",
                    State = "state",
                    PostalCode = "12345",
                    City = "city"
                }, "Admin123!").GetAwaiter().GetResult();

                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
                _userManager.AddToRoleAsync(user, StaticDetails.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
