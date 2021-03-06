using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;

namespace archivesystemWebUI.Repository
{
    public class FileRepo : Repository<File>, IFileRepo
    {
        private readonly ApplicationDbContext _context;


        public FileRepo(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }


        public ApplicationDbContext ApplicationDbContext => Context as ApplicationDbContext;
    }
}