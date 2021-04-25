using archivesystemDomain.Entities;
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
        bool DoesUserHasAccessToFolder(Folder folder, UserData userData);
        FolderServiceResult Edit(CreateFolderViewModel model);
        Folder FilterFolderSubFoldersUsingAccessLevel(Folder folder, int userAccessLevel);
        IEnumerable<AccessLevel> GetAllAccessLevels();
        CreateFolderViewModel GetCreateFolderViewModel(int parentId, string userId, bool userIsAdmin = false);
        IEnumerable<AccessLevel> GetCurrentUserAllowedAccessLevels(string userId);

        List<File> GetFiles(string filename, int folderId, bool returnall = false);
        IEnumerable<FolderFilesViewModel> GetFolderFiles(int folderId);
        List<FolderPath> GetFolderPath(int folderId);
        IEnumerable<Folder> GetFoldersThatMatchName(string search);
        Folder GetFolder(int folderId);
        Folder GetRootFolder();
        int? GetCurrentUserAccessLevel(string userId);
        FolderPageViewModel GetRootFolderPageViewModel(string userId, string userRole);
        IEnumerable<SubfolderViewModel> GetSubFolders(int folderId);
        UserData GetUserData(string userId);
        FolderServiceResult MoveFolder(MoveItemViewModel model);
        FolderServiceResult SaveFolder(SaveFolderViewModel model);
        Task<FolderServiceResult> SendAccessCode(string userId);
        Task<RequestResponse<string>> VerifyAccessCode(string userId, string code);


    }
    
}
