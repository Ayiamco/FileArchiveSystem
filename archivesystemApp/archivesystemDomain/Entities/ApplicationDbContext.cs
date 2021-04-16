using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace archivesystemDomain.Entities
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

       
        public static ApplicationDbContext Create()
        
        {
            return new ApplicationDbContext();
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<AccessDetail> AccessDetails { get; set; }
        public DbSet<FileContent> FileMetas { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<Signer> Signers { get; set; }
        public DbSet<ApplicationReceiver> ApplicationReceivers { get; set; }
        public DbSet<Activity> Activities { get; set; }
    }
}