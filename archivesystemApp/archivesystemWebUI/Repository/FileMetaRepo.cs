using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;

namespace archivesystemWebUI.Repository
{
    public class FileMetaRepo : Repository<FileContent>, IFileMetaRepo
    {
        private readonly ApplicationDbContext _context;

        public FileMetaRepo(ApplicationDbContext context )
        :base(context)
        {
            _context = context;
        }

        public ApplicationDbContext ApplicationDbContext => Context as ApplicationDbContext;
    }
}