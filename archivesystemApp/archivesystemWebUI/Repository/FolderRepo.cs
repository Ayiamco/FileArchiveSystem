﻿using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemWebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using archivesystemDomain.Services;

namespace archivesystemWebUI.Repository
{
    public class FolderRepo : Repository<Folder>, IFolderRepo
    {
        private readonly ApplicationDbContext _context;
        private List<Folder> Folders = new List<Folder>();

        public FolderRepo(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public Folder GetRootFolder()
        {
            var folder = _context.Folders.SingleOrDefault(x => x.Name == "Root");
            return folder;
        }

        public Folder GetRootWithSubfolder()
        {
            var folder = _context.Folders.Include(c => c.Subfolders).SingleOrDefault(x => x.Name == "Root");
            return folder;
        }

        public Folder GetFolderByName(string name)
        {
            return _context.Folders.SingleOrDefault(x => x.Name == name);
        }

        public Folder GetFacultyFolder(string name)
        {
            var rootFolder = GetRootFolder();
            var folder = _context.Folders.SingleOrDefault(x => x.Name == name && x.ParentId == rootFolder.Id);
            return folder;
        }

        public void UpdateFolder(Folder folder)
        {
            var folderInDb = _context.Folders.Find(folder.Id);
            if (folderInDb == null)
                return;
            folderInDb.Name = folder.Name;
            folderInDb.AccessLevelId = folder.AccessLevelId;
            folder.UpdatedAt = DateTime.Now;
        }

        public void DeleteFolder(int folderId)
        {
            RecursiveDelete(folderId);
            _context.Folders.RemoveRange(Folders);
        }

        public List<Folder> GetFoldersThatMatchName(string name)
        {
            return _context.Folders.Where(x => x.Name.Contains(name)).ToList();
        }

        public Folder GetFolder(int id)
        {
            var folder = _context.Folders.Include(x => x.Subfolders).Include(x => x.Files).Single(x => x.Id == id);
            return folder;
        }

        //public Stack<Folder> GetFolderPath(int id)
        //{
        //    var folders = new Stack<Folder>();
        //    var isNotRootFolder = true;
        //    folders.Push(_context.Folders.SingleOrDefault(x => x.Name == "Root"));
        //    var stack = new Stack<Folder>();
        //    while (isNotRootFolder)
        //    {
        //        var folder = _context.Folders.SingleOrDefault(x => x.Id == id);
        //        if (folder.ParentId == null)
        //            isNotRootFolder = false;
        //        else
        //        {
        //            stack.Push(folder);
        //            id = (int)folder.ParentId;
        //        }
        //    }
        //    while (stack.Count() > 0)
        //    {
        //        folders.Push(stack.Pop());
        //    }

        //    return folders;
        //}

        public void SaveFolderPath(int folderId)
        {
            Folder currentFolder = _context.Folders.Find(folderId);
            Folder currentFolderParent = _context.Folders.Find(currentFolder.ParentId);
            _context.SaveChanges();
        }

        private void RecursiveDelete(int folderId)
        {
            var folder = _context.Folders.Include(x => x.Subfolders).Single(x => x.Id == folderId);
            var subFolderCount = folder.Subfolders ?? new List<Folder>();
            if (subFolderCount.Count() > 0)
            {
                foreach (Folder _folder in folder.Subfolders)
                {
                    RecursiveDelete(_folder.Id);
                }
            }

            if (folder.IsDeletable)
                Folders.Add(folder);
        }

        void IFolderRepo.MoveFolder(int id, int newParentFolderId)
        {
            var folder = _context.Folders.Find(id);
            var currentSubfolderNames = _context.Folders.Include(x => x.Subfolders).Single(x => x.Id == newParentFolderId)
                .Subfolders.Select(x => x.Name);
            if (currentSubfolderNames.Contains(folder.Name))
                throw new Exception("folder already exist in location");

            folder.ParentId = newParentFolderId;
            return;
        }

        private List<FolderPath> CurrentPathFolders = new List<FolderPath>();

        public List<FolderPath> GetFolderPath(int folderId)
        {
            var currentfolder=_context.Folders.Find(folderId);
            if (currentfolder.Name == "Root")
            {
                CurrentPathFolders.Add(new FolderPath { Name = "Root", Id = currentfolder.Id });
                return CurrentPathFolders;
            }   
            else
            {
                CurrentPathFolders.Add(new FolderPath { Id=currentfolder.Id, Name=currentfolder.Name});
                GetFolderPath((int)currentfolder.ParentId);
            }

            return CurrentPathFolders;
        }

        
    }
}