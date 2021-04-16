using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemWebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using archivesystemDomain.Services;
using archivesystemWebUI.Infrastructures;

namespace archivesystemWebUI.Repository
{
    public class FolderRepo : Repository<Folder>, IFolderRepo
    {
        private readonly ApplicationDbContext _context;
        private List<Folder> __folders = new List<Folder>();
        private List<FolderPath> CurrentPathFolders = new List<FolderPath>();

        public FolderRepo(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public void DeleteFolder(int folderId)
        {
            RecursiveDelete(folderId);
            _context.Folders.RemoveRange(__folders);
        }
        public Folder GetFacultyFolder(string name)
        {
            var rootFolder = Find(x=> x.Name== GlobalConstants.RootFolderName && x.FacultyId==null && x.DepartmentId==null)
                .FirstOrDefault();
            var folders = FindWithNavProps(x => x.Name == name , x => x.Subfolders).ToList();
            var folder=folders.SingleOrDefault(x => x.Name == name && x.ParentId == rootFolder.Id);
            return folder;
        }
        public List<FolderPath> GetFolderPath(int folderId)
        {
            var currentfolder = Get(folderId);
            if (currentfolder.Name == GlobalConstants.RootFolderName)
            {
                CurrentPathFolders.Add(new FolderPath { Name = GlobalConstants.RootFolderName, Id = currentfolder.Id });
                return CurrentPathFolders;
            }
            else
            {
                CurrentPathFolders.Add(new FolderPath { Id = currentfolder.Id, Name = currentfolder.Name });
                GetFolderPath((int)currentfolder.ParentId);
            }

            return CurrentPathFolders;
        }

        public List<File> GetFilesThatMactchFileName(int folderId,string filename,bool returnall=false)
        {
            //var data=_context.Folders.Include(x => x.Files.Select( y=> y.FileMeta)).Where(c=> c.Id== folderId);
            var data = FindWithNavProps(x => x.Id == folderId, y => y.Files.Select(z => z.FileContent));
            if (data == null || data.Count()==0) return null;

            var folder = data.SingleOrDefault(c => c.Id == folderId);
            if (folder == null) return null;
            if (folder.Files == null) return null;
            if (returnall) return folder.Files.ToList();

            return folder.Files.Where(x=> x.FileContent.Title.ToLower().Contains(filename.ToLower())).ToList();

        }

        public void MoveFolder(int id, int newParentFolderId)
        {
            var folder = Get(id);
            if (folder.IsRestricted || folder == null)
                return;
            var currentSubfolderNames =  FindWithNavProps(x=>x.Id == newParentFolderId,x => x.Subfolders)
                                        .FirstOrDefault()
                                        .Subfolders.Select(x => x.Name);
            if (currentSubfolderNames.Any(x=> x.ToLower()==folder.Name.ToLower()))
                throw new AlreadyExistInLocationException();

            folder.ParentId = newParentFolderId;
            return;
        }
        private void RecursiveDelete(int folderId)
        {
            var folder = FindWithNavProps(x => x.Id == folderId,x => x.Subfolders).First();
            if (folder.IsRestricted || folder == null)
                return;
            var subFolderCount = folder.Subfolders ?? new List<Folder>();
            if (subFolderCount.Count() > 0)
            {
                foreach (Folder _folder in folder.Subfolders)
                {
                    RecursiveDelete(_folder.Id);
                }
            }

            __folders.Add(folder);
        }
        public bool UpdateDepartmentalFolder(Folder model)
        {
            var folderInDb = FindWithNavProps(x => 
                x.IsRestricted && x.DepartmentId == model.DepartmentId,x=> x.Department)?.SingleOrDefault();
            if (folderInDb == null) return false;

            folderInDb.Name = model.Name;
            if(folderInDb.Department.FacultyId != model.FacultyId)
            {
                var newParentFolder = Find(x => x.IsRestricted && x.FacultyId == model.FacultyId)?.SingleOrDefault();
                if (newParentFolder == null) return false;
                folderInDb.ParentId = newParentFolder.Id;
            }
            return true;
        }
        public bool UpdateFacultyFolder(Folder model)
        {
            var folderInDb = FindWithNavProps(x =>
                x.IsRestricted && x.FacultyId == model.FacultyId,x => x.Department)?.SingleOrDefault();
            if (folderInDb == null) return false;
            folderInDb.Name = model.Name;
            return true;
        }
    }
}