using archivesystemDomain.Services;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(archivesystemWebUI.Startup))]
namespace archivesystemWebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            SeedRoles.EnsurePopulated();
            SeedAppData.EnsurePopulated();
            SeedAdmin.EnsurePopulated();
           
            
        }
    }
}
