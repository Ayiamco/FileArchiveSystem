﻿using archivesystemDomain.Entities;
using archivesystemDomain.Services;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.FolderModels;
using archivesystemWebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archivesystemWebUI.Interfaces
{
    public interface IFolderService
    {
        FolderServiceResult DeleteFolder(int folderId);
        bool DoesUserHasAccessToFolder(Folder folder,UserData userData);
        FolderServiceResult Edit(CreateFolderViewModel model);
        Folder FilterFolderSubFoldersUsingAccessLevel(Folder folder, int userAccessLevel);
        IEnumerable<AccessLevel> GetAllAccessLevels();
        CreateFolderViewModel GetCreateFolderViewModel(int parentId,string userId, bool userIsAdmin=false);
        string GetCurrentUserAccessCode(string userId);
        IEnumerable<AccessLevel> GetCurrentUserAllowedAccessLevels(string userId);
        List<FolderPath> GetFolderPath(int folderId);
        IEnumerable<Folder> GetFoldersThatMatchName(string search);
        Folder GetFolder(int folderId);
        Folder GetRootFolder();
        FolderPageViewModel GetRootFolderPageViewModel(string userId, string userRole);
        UserData GetUserData(string userId);
        FolderServiceResult MoveFolder(MoveItemViewModel model);
        FolderServiceResult SaveFolder(SaveFolderViewModel model);




    }

    
}