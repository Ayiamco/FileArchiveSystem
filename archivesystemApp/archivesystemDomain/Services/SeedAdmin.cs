using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using archivesystemDomain.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace archivesystemDomain.Services
{
    
        public static class SeedAdmin
        {
            private static readonly string AdminUser = WebConfigurationManager.AppSettings["AdminUser"];
            private static readonly string AdminPassword = WebConfigurationManager.AppSettings["AdminPwd"];
            private static readonly string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
            private static readonly string AdminPhone = WebConfigurationManager.AppSettings["AdminPhone"];

            public static async void EnsurePopulated()
            {
               var dbContext =  new ApplicationDbContext();
               var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
               var admin = await userManager.FindByNameAsync(AdminUser);

               if (admin != null ) return;
               admin = new ApplicationUser
                    {UserName = AdminUser, Email = AdminEmail, PhoneNumber = AdminPhone, EmailConfirmed = true };
               var createAdmin = await userManager.CreateAsync(admin, AdminPassword);
               if (createAdmin.Succeeded)
                    await userManager.AddToRoleAsync(admin.Id, RoleNames.Admin);

               dbContext.SaveChanges();
            }
        }
}
