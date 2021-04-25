using System;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Configuration;
using archivesystemDomain.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Owin;

namespace archivesystemDomain.Services
{
    public static class SeedRoles
    {
        private static ApplicationDbContext Context => new ApplicationDbContext();
        private static ApplicationRoleManager RoleManager
            => new ApplicationRoleManager(new RoleStore<ApplicationRole>(Context));


        public static void EnsurePopulated()
        {
            Create(RoleNames.Admin);
            Create(RoleNames.Staff);
            Create(RoleNames.FacultyOfficer);
            Create(RoleNames.HOD);
            Create(RoleNames.DeptOfficer);
            Create(RoleNames.Management);

            void Create(string name)
            {
                if (RoleManager.RoleExists(name)) return;
                var role = new ApplicationRole { Name = name , CreatedAt = DateTime.Now, UpDatedAt = DateTime.Now};
                RoleManager.Create(role);
            }

        }
       

    }
}