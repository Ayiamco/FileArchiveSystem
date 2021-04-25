using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.FolderModels;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using archivesystemWebUI.Infrastructures;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;

namespace archivesystemWebUI.Services
{
    public enum FolderServiceResult
    {
        AlreadyExist, InvalidAccessLevel, MaxFolderDepthReached, Success, Expired
            , InvalidModelState, Prohibited, NotFound, UnknownError,Failure
    }
    public class FolderService : IFolderService
    {
        private IFolderServiceUnitOfWork _repo { get; set; }
        private const int  _GroundLevelAccess=0;
       

        public FolderService(IFolderServiceUnitOfWork repo)
        {
            _repo = repo;
        }

        public FolderServiceResult DeleteFolder(int folderId)
        {
            try
            {
                var folderToDelete = _repo.FolderRepo.Get(folderId);
                if (folderToDelete == null)
                    return FolderServiceResult.NotFound;
                if (folderToDelete.IsRestricted || folderToDelete.Name== GlobalConstants.RootFolderName)
                    return FolderServiceResult.Prohibited;

                _repo.FolderRepo.DeleteFolder(folderToDelete.Id);
                _repo.Save();
                return FolderServiceResult.Success;
            }
            catch
            {
                return FolderServiceResult.UnknownError;
            }

        }

        public bool DoesUserHasAccessToFolder(Folder folder,UserData data)
        {
            if (data.User == null) return false;
            if (folder.AccessLevelId > data.UserAccessLevel) return false;

            if (folder.FacultyId != null && data.User.Department.FacultyId != folder.FacultyId)//folder is faculty folder
                return false;
            if (folder.DepartmentId != null && folder.DepartmentId != data.User.DepartmentId)
                return false;
            
            return true;
        }

        public FolderServiceResult Edit(CreateFolderViewModel model)
        {
            if (model.AccessLevelId == 0 || string.IsNullOrEmpty(model.Name) || model.Id == 0)
                return FolderServiceResult.InvalidModelState;

            var folderInDb = _repo.FolderRepo.Get(model.Id);
            if (folderInDb == null) return FolderServiceResult.NotFound;
            if (folderInDb.IsRestricted) return FolderServiceResult.Prohibited;
            if (folderInDb.Name == GlobalConstants.RootFolderName) return FolderServiceResult.AlreadyExist;

            var parentFolder = _repo.FolderRepo.FindWithNavProps(
                x=> x.Id==(int)folderInDb.ParentId,
                x=> x.Subfolders
                );
            if (parentFolder.Count() != 1) return FolderServiceResult.NotFound;
            if (parentFolder.Single().Subfolders.Select(x => x.Name.ToLower()).Any(x=> x==model.Name.ToLower().Trim())
                && folderInDb.Name != model.Name) return FolderServiceResult.AlreadyExist;
             
            folderInDb.Name= model.Name.Trim();
            folderInDb.AccessLevelId = model.AccessLevelId;
            folderInDb.UpdatedAt = DateTime.Now;
            _repo.Save();
            return FolderServiceResult.Success;
        }

     
        public Folder FilterFolderSubFoldersUsingAccessLevel(Folder folder,int userAccessLevel)
        {
            var subfolders = folder.Subfolders.Where(x => x.AccessLevelId <= userAccessLevel);
            folder.Subfolders = subfolders.Count() != 0 ? subfolders.ToList(): new List<Folder>();
            return folder;
        }

        public CreateFolderViewModel GetCreateFolderViewModel(int parentId,string userId,bool userIsAdmin=false )
        {
            if (userIsAdmin)
                return GetAdminCreateFolderViewModel(parentId);
            
            var parentFolder = _repo.FolderRepo.Get(parentId);
            if (parentFolder == null) return null;

            var user = _repo.UserRepo.FindWithNavProps(c => c.UserId == userId, _ => _.Department).SingleOrDefault();
            if (user == null) return null;

            var userdetails = _repo.AccessDetailsRepo.Find(x => x.AppUserId == user.Id).SingleOrDefault();
            if(userdetails==null) return null;
            
            var userAllowedLevels=_repo.AccessLevelRepo.GetAll().Where(x => x.Id <= userdetails.AccessLevelId);
            userAllowedLevels = userAllowedLevels.Where(x => x.Id >= parentFolder.AccessLevelId);
            var data = new CreateFolderViewModel() { Name = "", ParentId = parentId, AccessLevels = userAllowedLevels };
            return data;
        }

        private CreateFolderViewModel GetAdminCreateFolderViewModel(int parentFolderId)
        {
            var parentFolder = _repo.FolderRepo.Get(parentFolderId);
            if (parentFolder == null) return null;

            var userAllowedLevels = _repo.AccessLevelRepo.GetAll();
            userAllowedLevels = userAllowedLevels.Where(x => x.Id >= parentFolder.AccessLevelId);
            var data = new CreateFolderViewModel() { Name = "", ParentId = parentFolderId, AccessLevels = userAllowedLevels };
            return data;
        }

        public IEnumerable<AccessLevel> GetAllAccessLevels()
        {
           return  _repo.AccessLevelRepo.GetAll();
        }

        public int? GetCurrentUserAccessLevel(string userId)
        {
            var users = _repo.UserRepo.Find(c => c.UserId == userId);
            var user = users.SingleOrDefault();
            if (user == null) return null;

            var userdetail = _repo.AccessDetailsRepo.Find(x => x.AppUserId == user.Id);
            var userdetails = userdetail.SingleOrDefault();
            var userAccessLevel = userdetails == null ? 0 : userdetails.AccessLevelId;
            return userAccessLevel;
        }

        public IEnumerable<AccessLevel> GetCurrentUserAllowedAccessLevels(string userId)
        {
            var users = _repo.UserRepo.Find(c => c.UserId == userId);
            var user= users.SingleOrDefault();
            if (user == null) return null;

            //_repo.Setup(r => r.AccessDetailsRepo.Find(x => x.AppUserId == dummyAppUserId)).Returns(accessDetails);
            var userdetail = _repo.AccessDetailsRepo.Find(x => x.AppUserId == user  .Id);
             var userdetails=userdetail.SingleOrDefault();
            var userAccessLevel = userdetails == null ? 0 : userdetails.AccessLevelId;
            
            var allowedAccessLevels=_repo.AccessLevelRepo.GetAll().Where(x => x.Id <= userAccessLevel);
            if (allowedAccessLevels.Count() == 0) return null;

            return allowedAccessLevels;
        }

        public async Task<RequestResponse<string>> VerifyAccessCode(string userId,string code)
        {
            var lastVisitTime = HttpContext.Current.Session[SessionData.OTPRequestTime] == null ?
                Convert.ToDateTime("01/01/1970") : (DateTime)HttpContext.Current.Session[SessionData.OTPRequestTime];
            var timeSinceLastActivity = DateTime.Now - lastVisitTime;
            var isExpired = timeSinceLastActivity > new TimeSpan(0, GlobalConstants.LOCKOUT_TIME, 0);


            if (isExpired) {
                await SendAccessCode(userId);
                return new RequestResponse<string>
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = "OTP expired: Check mail for new OTP"
                };
            }
           
            var otp = HttpContext.Current.Session[SessionData.OTP] == null ? "" :
                (string)HttpContext.Current.Session[SessionData.OTP];
            if (code == otp) {
                HttpContext.Current.Session.Remove(SessionData.OTPRequestTime);
                return new RequestResponse<string>
                {
                    Status = HttpStatusCode.OK,
                    Message = "access code is correct"
                };
            }
           

            return new RequestResponse<string>
            {
                Status = HttpStatusCode.BadRequest,
                Message = "Access code is incorrect"
            }; ;
        }

        public List<File> GetFiles(string filename,int folderId, bool returnall=false)
        {
            return _repo.FolderRepo.GetFilesThatMactchFileName(folderId, filename);
     
        }
        public IEnumerable<Folder> GetFoldersThatMatchName(string name)
        {
            return _repo.FolderRepo.FindWithNavProps(x => x.Name.Contains(name));
        }

        public List<FolderPath> GetFolderPath(int folderId)
        {
            return _repo.FolderRepo.GetFolderPath(folderId);
        }

        public Folder GetFolder(int folderId)
        {
            var folders = _repo.FolderRepo.FindWithNavProps(x => x.Id == folderId, x => x.Subfolders);
            if (folders.Count() != 1) return null;
            return folders.Single(x => x.Id == folderId);
        }

        public IEnumerable<FolderFilesViewModel> GetFolderFiles(int folderId)
        {
            var files =_repo.FileRepo.Find(_ => _.FolderId == folderId).ToList();
            if (files.Count() < 1) return null;
            var folderfilesmodel=files.Select(x => new FolderFilesViewModel
            { 
                Id = x.Id,
                Name= x.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ContentType=x.ContentType,
                IsArchived= x.IsArchived

            });;
            return folderfilesmodel;

        }

        public Folder GetRootFolder()
        {
            return _repo.FolderRepo
                    .FindWithNavProps(x => x.Name == "Root" && x.FacultyId == null && x.DepartmentId == null,
                    x => x.Subfolders)
                    .FirstOrDefault();
        }

        public FolderPageViewModel GetRootFolderPageViewModel(string userId,string userRole)
        {
            UserData data = GetUserData(userId);
            if (data.UserAccessLevel == _GroundLevelAccess && !userRole.Contains(RoleNames.Admin))
                return null ;
            var rootFolder = GetRootFolder();
            var model = new FolderPageViewModel
            {
                Id = rootFolder.Id,
                Name = rootFolder.Name,
                CurrentPath = new List<FolderPath> { new FolderPath { Id = rootFolder.Id, Name = GlobalConstants.RootFolderName } },
                DirectChildren = userRole == RoleNames.Admin ?
                rootFolder.Subfolders :
                rootFolder.Subfolders.Where(x => x.FacultyId == data.User.Department.FacultyId).ToList()
            };
            return model;
        }

        public IEnumerable<SubfolderViewModel> GetSubFolders(int folderId)
        {
            var folder= _repo.FolderRepo.FindWithNavProps(x => x.Id == folderId, x => x.Subfolders).AsQueryable();
            if (folder.Count() != 1) return null;
            if (folder.Single().Subfolders.Count() < 1) return new List<SubfolderViewModel>();
            return folder.Single().Subfolders.Select( x=> new SubfolderViewModel{Name=x.Name,Id=x.Id });

        }
        public UserData GetUserData(string userId)
        {
            var user = _repo.UserRepo.FindWithNavProps(c => c.UserId == userId, _ => _.Department).SingleOrDefault();
            if (user == null) return new UserData { User = null, UserAccessLevel = 0 };
            var userdetails = _repo.AccessDetailsRepo.Find(x=> x.AppUserId==user.Id).SingleOrDefault();
            var userAccessLevel = userdetails == null ? 0 : userdetails.AccessLevelId;
            return  new UserData { User = user, UserAccessLevel = userAccessLevel };
        }

        public FolderServiceResult MoveFolder(MoveItemViewModel model)
        {
            if (model.Id == 0 || model.NewParentFolderId == 0 || model == null)
                return FolderServiceResult.InvalidModelState;
            if (model.Id == model.NewParentFolderId)
                return FolderServiceResult.Prohibited; //Cannot move folder into itself
            
            var moveIsSuccessful=TryMoveFolder(model);
            if(moveIsSuccessful)
                return FolderServiceResult.Success;
            return FolderServiceResult.AlreadyExist;
        }

        public FolderServiceResult SaveFolder(SaveFolderViewModel model)
        {
            var parentFolder = GetFolder(model.ParentId);
            if (parentFolder == null) return FolderServiceResult.NotFound;
            if (parentFolder.Name == GlobalConstants.RootFolderName)
                return FolderServiceResult.Prohibited;
            if (parentFolder.Subfolders.Select(x => x.Name.ToLower()).Contains(model.Name.ToLower()) 
                || model.Name == GlobalConstants.RootFolderName) return FolderServiceResult.AlreadyExist;
            if (parentFolder.AccessLevelId > model.AccessLevelId)
                return FolderServiceResult.InvalidAccessLevel;
            if (GetFolderCurrentDepth(model.ParentId) >= GlobalConstants.MaxFolderDepth)
                return FolderServiceResult.MaxFolderDepthReached;

            TrySaveFolder(model);
            return FolderServiceResult.Success;
        }

        public async Task<FolderServiceResult> SendAccessCode(string userId)
        {
            var user=_repo.UserRepo.Find(x => x.UserId == userId);
            if (user.Count() != 1) return FolderServiceResult.NotFound;
            if (string.IsNullOrEmpty(user.Single().Email)) return FolderServiceResult.Failure;
            var appuserId = user.First().Id;
            var userAccessDetails=_repo.AccessDetailsRepo.Find(x => x.AppUserId == appuserId).ToList();
            if(userAccessDetails.Count() != 1) return FolderServiceResult.NotFound;

            var code=_repo.CodeGenerator.NewOTP();
            HttpContext.Current.Session[SessionData.OTP] = code;
            HttpContext.Current.Session[SessionData.OTPRequestTime] = DateTime.Now;
            await  _repo.MailSender.SendEmailAsync(
                    user.Single().Email,
                    "One Time AccessCode",
                    $"Hi {user.Single().Name}, Here is your one time accesscode {code}"
                    );
            return FolderServiceResult.Success;
        }

        
        #region Private Methods
        private int GetFolderCurrentDepth(int parentId)
        {
            var parentFolderPath = GetFolderPath(parentId);
            return parentFolderPath.Count();
        }

        private bool TryMoveFolder(MoveItemViewModel model)
        {
            try
            {
                _repo.FolderRepo.MoveFolder(model.Id, model.NewParentFolderId);
                _repo.Save();
                return true;
            }
            catch (AlreadyExistInLocationException) { return false; } 
        }
        private void TrySaveFolder(SaveFolderViewModel model)
        {
            var parentFolder = GetFolder(model.ParentId);
            var folder = new Folder
            {
                Name = model.Name.Trim(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ParentId = parentFolder.Id,
                AccessLevelId = model.AccessLevelId,
                IsRestricted = false,
                DepartmentId = parentFolder.DepartmentId,
            };
            _repo.FolderRepo.Add(folder);
            _repo.Save();
        }
        #endregion
    }
}