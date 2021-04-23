using archivesystemDomain.Entities;
using archivesystemWebUI.Infrastructures;

namespace archivesystemWebUI.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddFaculty() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
