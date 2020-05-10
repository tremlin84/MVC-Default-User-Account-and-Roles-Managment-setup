using Backend.Roles;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockSystem.Models
{
    public class BaseSetting
    {           
        private ApplicationDbContext db = new ApplicationDbContext();
                 
        public BaseSetting(string user, string password)
        {
            CreateAccount(user, password).Wait();               
        }
                
        protected async Task CreateAccount(string user, string password)
        {
            // Admin role
            const string Admin = "Admin";         
            
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                        
            try
            {   
                if (userManager.Users.Count() == 0)
                {
                    ApplicationUser BaseUser = CreateBaseUser(user);
                    var create = await userManager.CreateAsync(BaseUser, password);
                    if (create.Succeeded && !await roleManager.RoleExistsAsync(Admin))
                    {
                        await SetupRoles(roleManager);
                        await userManager.AddToRoleAsync(BaseUser.Id, Admin);
                    }
                }                          
            }
            catch
            {
                DisposeAll(userManager, roleManager);
            }
        }               
                
        private async Task SetupRoles(RoleManager<IdentityRole> rolemanager)
        {
            foreach (var Role in Enum.GetValues(typeof(RolesList)))
            {                
                await rolemanager.CreateAsync(new IdentityRole { Name = Role.ToString()});
            }
        }
                
        private ApplicationUser CreateBaseUser(string admin)
        {
            return new ApplicationUser {
                UserName = admin,
                Email = admin,
                TwoFactorEnabled = false,
                EmailConfirmed = true, };             
        }

        private void DisposeAll(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db.Dispose();
            userManager.Dispose();
            roleManager.Dispose();
        }                
    }
}
