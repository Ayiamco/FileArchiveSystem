using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Infrastructures;
using archivesystemWebUI.Infrastructures.CustomFilters;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.FolderModels;
using archivesystemWebUI.Repository;
using archivesystemWebUI.Services;
using AutoMapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;


namespace archivesystemWebUI.Controllers
{
    [Authorize]
    [RegisterRequestTime]
  
    public class FolderController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IAccessCodeGenerator _accessCodeGenerator;
        private readonly IFolderService _service;
       
        public FolderController(IRoleService roleService, IFolderService service, IAccessCodeGenerator accessCodeGenerator) 
        {
            _accessCodeGenerator = accessCodeGenerator;
            _service = service;
            _roleService = roleService;
        }

        // GET: /folders
        [Route("folders")]
        public ActionResult Index(string search = null, string returnUrl="/folders",bool requestForOTP = true)
        {
            if (!requestForOTP && returnUrl != "/folders")
                return RedirectToAction(nameof(GetFolder), new { folderId = returnUrl.Split('/')[1] });
            
            FolderPageViewModel model = GetRootViewModel(returnUrl, search);
            if (model == null) // user does not yet have an access level
            {
                TempData[GlobalConstants.userHasNoAccesscode] = true;
                return RedirectToAction("Index", "Home");
            }

            if (!HttpContext.User.IsInRole(RoleNames.Admin))
            {
                if (requestForOTP == true) _service.SendAccessCode(User.Identity.GetUserId());
                model.RequestForOTP = returnUrl == "/folders" ?  false: requestForOTP;
                var hasCorrectAccessCode = Session[SessionData.AccessValidated] != null;
                var errorMessage = hasCorrectAccessCode ? 
                    "Locked out due to inactivity, Check your mail for your OTP " :
                    "Check your mail for your OTP" ;
                    ModelState.AddModelError("", errorMessage);
            }
            else  model.RequestForOTP = false;

            ViewBag.AllowCreateFolder = false;
            return View("FolderList", model);
        }

        // GET: /folders/add
        [Route("folders/add")]
        [HttpGet]
        public ActionResult Create(int id)
        {
            CreateFolderViewModel viewModel;
            if(HttpContext.User.IsInRole(RoleNames.Admin))
                viewModel=_service.GetCreateFolderViewModel(id, HttpContext.User.Identity.GetUserId(),userIsAdmin:true);
            else
                viewModel = _service.GetCreateFolderViewModel(id, HttpContext.User.Identity.GetUserId());

            if (viewModel == null) return RedirectToAction(nameof(Index));
            return PartialView("_CreateFolder",viewModel );
        }

        //POST: /folders/add
        [Route("folders/add")]
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public ActionResult Create(SaveFolderViewModel model)
        {
            if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (HttpContext.User.IsInRole(RoleNames.Admin)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var userIsnotPermitted = IsCreateActionForbidden(_service.GetFolder(model.ParentId));
            if (userIsnotPermitted) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var result = _service.SaveFolder(model);
            if (result == FolderServiceResult.Success) return new HttpStatusCodeResult(HttpStatusCode.OK);
            if (result == FolderServiceResult.NotFound) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (result == FolderServiceResult.AlreadyExist) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (result == FolderServiceResult.InvalidAccessLevel) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (result== FolderServiceResult.MaxFolderDepthReached) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [Route("folders/move")]
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public ActionResult MoveItem(MoveItemViewModel model)
        {
            if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var result= _service.MoveFolder(model);
            if (result== FolderServiceResult.InvalidModelState) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (result== FolderServiceResult.Prohibited) return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            if (result== FolderServiceResult.Success) return new HttpStatusCodeResult(HttpStatusCode.OK);
            if (result== FolderServiceResult.AlreadyExist) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return new HttpStatusCodeResult(500);
        }

        //POST: /folders/create
        [Route("folders/delete")]
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var result = _service.DeleteFolder(id);
            if (result==FolderServiceResult.Success) return new HttpStatusCodeResult(HttpStatusCode.OK);
            if (result == FolderServiceResult.NotFound) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (result == FolderServiceResult.Prohibited) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);  
        }

        [Route("folders/resendotp")]
        public async Task<ActionResult> SendAccessCode()
        {
            var result= await _service.SendAccessCode(HttpContext.User.Identity.GetUserId());
            if(result== FolderServiceResult.Success) return new HttpStatusCodeResult(HttpStatusCode.OK);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        //GET: /folders/{id}
        [Route("folders/{folderId:int}")]
        [ValidateAccess]
        [HttpGet]
        public ActionResult GetFolder(int folderId)
         {
            Folder folder = _service.GetFolder(folderId);
            if (folder.Name == GlobalConstants.RootFolderName || folder== null)
                return RedirectToAction(nameof(Index));

            
            if (HttpContext.User.IsInRole(RoleNames.Admin)) ViewBag.AllowCreateFolder = false;
            else
            {
                FilterFolderSubFolders(folder);
                var userIsnotPermitted = IsCreateActionForbidden(folder);
                if (userIsnotPermitted)  ViewBag.AllowCreateFolder = false;
                else  ViewBag.AllowCreateFolder = true;  
            }
            return View("FolderList", GetViewModelForView(folder, folderId));
        }

        //POST: /folders/{parentId}
        [HttpPost]
        [Route("folders/{parentId}")]
        public ActionResult BackToParent(int parentId)
        {
            return RedirectToAction(nameof(GetFolder), new { folderId = parentId });
        }

        //POST: /Folder/Edit
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public ActionResult Edit(CreateFolderViewModel model) 
        {
            if(!ModelState.IsValid)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var result = _service.Edit(model);

            if (result == FolderServiceResult.Prohibited) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (result == FolderServiceResult.NotFound) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (result == FolderServiceResult.AlreadyExist) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (result == FolderServiceResult.Success) return new HttpStatusCodeResult(HttpStatusCode.OK);  
            return new HttpStatusCodeResult(500);
        }


        //GET: /Folder/GetEditPartialView
        public ActionResult GetEditFolderPartialView(int id)
        {
            var folder = _service.GetFolder(id);
           
            if (folder == null) return new HttpStatusCodeResult(400);
            if (folder.IsRestricted) return new HttpStatusCodeResult(403);

            CreateFolderViewModel model;
            if(!HttpContext.User.IsInRole(RoleNames.Admin))
                model = new CreateFolderViewModel
                {
                    Id = id,
                    AccessLevelId = (int)folder.AccessLevelId,
                    AccessLevels = _service.GetCurrentUserAllowedAccessLevels(HttpContext.User.Identity.GetUserId()),
                    Name = folder.Name,
                };
            else
                model = new CreateFolderViewModel
                {
                    Id = id,
                    AccessLevelId = (int)folder.AccessLevelId,
                    AccessLevels = _service.GetAllAccessLevels(),
                    Name = folder.Name,
                };
            return PartialView("_EditFolder", model);
        }

        //GET: /Folder/GetFiles
        public ActionResult GetFile(string filename,int folderId,bool returnall=false)
        {
            var files = _service.GetFiles(filename, folderId, returnall:returnall);
            if (files == null) return new HttpStatusCodeResult(404); 
            return PartialView("GetAllFiles",files);
        }

        public ActionResult GetFolderFiles(int folderId)
        {
            var files=_service.GetFolderFiles(folderId);
            return PartialView("GetAllFiles",files);
        }

        //GET: /Folder/GetDeleteFolderPartialView
        public ActionResult GetDeleteFolderPartialView(int id)
        {
            var folder = _service.GetFolder(id);
            if (folder == null)
                return new HttpStatusCodeResult(400);
            if (folder.IsRestricted)
                return new HttpStatusCodeResult(403);
            return PartialView("_DeleteFolder", new DeleteFolderViewModel
            {
                Name = folder.Name,
                Id = id,
                ParentId = (int)folder.ParentId
            });
        }

        //GET: /Folder/GetConfirmItemMovePartialView
        public ActionResult GetConfirmItemMovePartialView()
        {
            ViewBag.ItemName = Request.QueryString["itemName"];
            ViewBag.CurrentFolder = Request.QueryString["currentFolder"];
            ViewBag.NewParentFolderId = Request.QueryString["newParentFolderId"];
            return PartialView("_ConfirmItemMove");
        }
         
        //POST: /Folder/VerifyAccessCode
        [HttpPost]
        public async Task< JsonResult> VerifyAccessCode(string accessCode)
        {
            if (string.IsNullOrWhiteSpace(accessCode))
                return Json(new RequestResponse<string> {Status=HttpStatusCode.BadRequest, Message="accesscode cannot be empty" });

            var userId = HttpContext.User.Identity.GetUserId();
            var result =  await _service.VerifyAccessCode(userId,accessCode);
            if (result.Status == HttpStatusCode.OK) 
            {
                Session[SessionData.AccessValidated] = true;
            } 

            return Json(result);
        }


        //GET: /folder/getsubfolders
        [HttpGet]
        public JsonResult GetSubFolders(int folderId)
        {
            var subfolders=_service.GetSubFolders(folderId);
            if (subfolders == null) return Json(new RequestResponse<IEnumerable<SubfolderViewModel>> { Status = HttpStatusCode.NotFound});
            if (subfolders.Count() == 0) return Json(new RequestResponse<IEnumerable<SubfolderViewModel>> { Status = HttpStatusCode.NoContent });
            return Json( subfolders,JsonRequestBehavior.AllowGet );
        }

        

        #region Private Methods
        private void CheckForUserAccessCode(out bool hasCorrectAccessCode, out double timeSinceLastRequest)
        {
            hasCorrectAccessCode =
               Session[GlobalConstants.IsAccessValidated] != null && (bool)Session[GlobalConstants.IsAccessValidated];

            var lastVisitTime = Session[GlobalConstants.LastVisit] ?? new DateTime();
            timeSinceLastRequest = (DateTime.Now - (DateTime)lastVisitTime).TotalMinutes;
        }

        private FolderPageViewModel GetModelUsingService(string returnUrl)
        {
            var userId = HttpContext.User.Identity.GetUserId();
            var userRoles = _roleService.GetCurrentUserRoles();
            var model = _service.GetRootFolderPageViewModel(userId, userRoles);
            if (model == null) return null;
            model.ReturnUrl = returnUrl;
            return model;
        }

        private FolderPageViewModel GetRootViewModel(string returnUrl, string search = null)
        {
            if (string.IsNullOrEmpty(search))
                return GetModelUsingService(returnUrl);

            return SearchForFolders(search);
        }
        private FolderPageViewModel GetViewModelForView(Folder folder, int folderId)
        {
            var folderpath = _service.GetFolderPath(folderId);
            var model = Mapper.Map<FolderPageViewModel>(folder);
            model.CurrentPath = folderpath;
            return model;
        }
        private FolderPageViewModel SearchForFolders(string search)
        {
            var folders = _service.GetFoldersThatMatchName(search).ToList();
            if (!HttpContext.User.IsInRole(RoleNames.Admin))
                folders = FilterFolderSearchResult(folders);
            var model = new FolderPageViewModel
            {
                DirectChildren = folders,
                CurrentPath = new List<FolderPath>(),
                RequestForOTP = false,
                Id = 0
            };
            return model;
        }

        private List<Folder> FilterFolderSearchResult(List<Folder> folders)
        {
            var userData=_service.GetUserData(HttpContext.User.Identity.GetUserId());
            List<Folder> returnObj= new List<Folder>();
            returnObj.AddRange(folders.Where(f => f.DepartmentId == userData.User.DepartmentId));
            var obj = folders.SingleOrDefault(x => x.FacultyId != null && x.FacultyId == userData.User.Department.FacultyId);
            if(obj != null)
                returnObj.Add(obj);
            if (returnObj.Count() == 0 )
                return null;
            return returnObj;
        }

        private bool IsCreateActionForbidden(Folder folder)
        {
            //folder is faculty folder and user is not faculty officer 
            if (folder.FacultyId != null && !HttpContext.User.IsInRole(RoleNames.FacultyOfficer))
                return true;

            if (folder.Name == GlobalConstants.RootFolderName)
                return true;

            return false;
        }
        private void FilterFolderSubFolders( Folder folder)
        {
            var facultyId = (int)Session[SessionData.FacultyId];
            //if folder is rootfolder filter out folders other Faculty folders
            if (folder.Name == GlobalConstants.RootFolderName)
                folder.Subfolders = folder.Subfolders.Where(x => x.FacultyId == facultyId).ToList();
            else
            {
                //Filter out folders with higher accesslevels
                var accessLevel = (int)Session[SessionData.AccessLevel];
                folder.Subfolders = folder.Subfolders.Where(x => x.AccessLevelId <= accessLevel).ToList();

                //filter out folders based on dept
                var deptId = (int)Session[SessionData.DeptId];
                folder.Subfolders = folder.Subfolders.Where(x => x.DepartmentId == deptId).ToList();

            }

           
        }

        private void SendUserAccessCodeToUser()
        {
            var userId = HttpContext.User.Identity.GetUserId();
            _service.SendAccessCode(userId);
        }
        

       
        #endregion
    }
}

