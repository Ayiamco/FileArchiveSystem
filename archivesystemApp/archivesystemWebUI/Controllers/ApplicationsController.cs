using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Ninject.Infrastructure.Language;
using WebGrease.Css.Extensions;

namespace archivesystemWebUI.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IApplicationService _applicationService;
        private readonly IUserService _userService;
        private readonly IUpsertFile _upsertFile;
        private readonly IRoleService _roleService;
        private readonly IEmailSender _emailSender;

        public ApplicationsController(
             IDepartmentService departmentService,
             IApplicationService applicationService,
             IUserService userService,
             IUpsertFile upsertFile, 
             IRoleService roleService,
             IEmailSender emailSender)
        {
            _departmentService = departmentService;
            _applicationService = applicationService;
            _userService = userService;
            _upsertFile = upsertFile;
            _roleService = roleService;
            _emailSender = emailSender;
        }

        // GET: Applications
        [Authorize(Roles = "Student,Alumni,Staff")]
        public ActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult Incoming()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult Received()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult Rejected()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult IncomingForwarded()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult RejectedForwarded()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult ReceivedForwarded()
        {
            return View();
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult Archived()
        {
            return View();
        }


        [Authorize(Roles = "DeptOfficer")]
        public ActionResult ToSign()
        {
            return View();
        }


        [Authorize(Roles = "DeptOfficer")]
        public ActionResult Signed()
        {
            return View();
        }


        [Authorize(Roles = "DeptOfficer")]
        public ActionResult Declined()
        {
            return View();
        }


        [Authorize(Roles = "HOD")]
        public ActionResult ToApprove()
        {
            return View();
        }


        [Authorize(Roles = "HOD")]
        public ActionResult Approved()
        {
            return View();
        }


        [Authorize(Roles = "HOD")]
        public ActionResult Disapproved()
        {
            return View();
        }


        [Authorize(Roles = "HOD")]
        public ActionResult Forwarded()
        {
            return View();
        }


        [Authorize(Roles = "Student,Alumni,Staff")]
        public ActionResult New()
        {
            var currentUserId = User.Identity.GetUserId();


            var user = _userService.GetById(currentUserId).result;
            var model = new ApplicationVm
            {
                UserId = user?.Id ?? 0
            };

           RetainFormState(model,user);

           return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ApplicationVm model, HttpPostedFileBase fileBase)
        {
            var currentUserId = User.Identity.GetUserId();
            var user = _userService.GetById(currentUserId).result;

            if (!ModelState.IsValid)
            {
                RetainFormState(model, user);

                return PartialView("New", model);
            }

            var save = _applicationService.Create(model, fileBase, user);
            if (save)
            {
                return Json(new { saved = true });
            }

            ModelState.AddModelError("", "An Error Occured!");
            RetainFormState(model, user);

            return PartialView("New", model);
        }

        [Authorize(Roles = "Secretary")]
        public async  Task<ActionResult> Accept(int id)
        {
            var (accept, msg) = await _applicationService.Accept(id);
            if (accept) return RedirectToAction("Details", new { id });
            
            TempData["Msg"] = msg;

            return  RedirectToAction("Details", new{id});
        }

        [Authorize(Roles = "DeptOfficer")]

        public ActionResult Sign(int id)
        {
            ViewBag.Action = "Sign";
            return PartialView(new SignVm {AppId = id});
        }

        [Authorize(Roles = "HOD")]
        public ActionResult Disapprove(int id)
        {
            ViewBag.Action = "Disapprove";
            return PartialView("Sign",new SignVm { AppId = id });
        }

        [Authorize(Roles = "HOD")]
        public ActionResult SignApprove(int id)
        {
            ViewBag.Action = "SignApprove";
            return PartialView("Sign", new SignVm { AppId = id });
        }

        [Authorize(Roles = "HOD")]
        public ActionResult SignForward(int id)
        {
            ViewBag.Action = "SignForward";
            return PartialView("Sign", new SignVm { AppId = id });
        }

        [Authorize(Roles = "DeptOfficer")]
        public ActionResult Decline (int id)
        {
            ViewBag.Action = "Decline";
            return PartialView("Sign",new SignVm { AppId = id });
        }


        [Authorize(Roles = "Secretary")]
        public ActionResult Reject(int id)
        {
            ViewBag.Action = "Reject";
            return PartialView("Sign", new SignVm { AppId = id });
        }


        [Authorize(Roles = "Secretary")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(SignVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = "Reject";
                return PartialView("Sign",model);
            }

            var (reject, msg) = await _applicationService.Reject(model);

            if (reject) return Json(new { done = true });
            
            ModelState.AddModelError("", msg);
            ViewBag.Action = "Reject";
            return PartialView("Sign", model);
            
        }


        [Authorize(Roles = "DeptOfficer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Sign(SignVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = "Sign";
                return PartialView(model);
            }


            var (sign, msg) = await _applicationService.Sign(model);
            if (sign) return Json(new { done = true });

            ModelState.AddModelError("", msg);
            ViewBag.Action = "Sign";
            return PartialView(model);
        }


        [Authorize(Roles = "DeptOfficer")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Decline(SignVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = "Decline";
                return PartialView("Sign",model);
            }

            var (decline, msg) = await _applicationService.Decline(model);

            if (decline) return Json(new { done = true });

            ModelState.AddModelError("", msg);
            ViewBag.Action = "Decline";
            return PartialView("Sign", model);
           
        }


        [Authorize(Roles = "HOD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignApprove(SignVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = "SignApprove";
                return PartialView("Sign", model);
            }

            var (approve, msg) = await _applicationService.SignApprove(model);

            if (approve) return Json(new { done = true });

            ModelState.AddModelError("", msg);
            ViewBag.Action = "SignApprove";
            return PartialView("Sign", model);

        }

        

        [Authorize(Roles = "HOD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignForward(SignVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = "SignForward";
                return PartialView("Sign", model);
            }

            var (forward, msg) = await _applicationService.SignForward(model);

            if (forward) return Json(new { done = true });

            ModelState.AddModelError("", msg);
            ViewBag.Action = "SignForward";
            return PartialView("Sign", model);
           
        }


        [Authorize(Roles = "HOD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disapprove(SignVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = "Disapprove";
                return PartialView("Sign", model);
            }

            var (disapprove, msg) = await _applicationService.Disapprove(model);

            if(disapprove) return Json(new { done = true });

            ModelState.AddModelError("", msg);
            ViewBag.Action = "Disapprove";
            return PartialView("Sign", model);
        }

        [Authorize(Roles = "DeptOfficer")]
        public ActionResult AssignUsers(int appId)
        {
            var currentUserId = User.Identity.GetUserId();
            var user = _userService.GetById(currentUserId).result;
            if (user==null)
            {
                return HttpNotFound();
            }
            return PartialView(new AssignUsersVm
            {
                Id = appId
            });
        }

        [Authorize(Roles = "HOD")]
        public ActionResult SendToDepts(int appId)
        {
            return PartialView(new AssignDeptsVm
            {
                Id = appId
            });
        }


        [Authorize(Roles = "Admin, Student, Secretary, Secretary, HOD, DeptOfficer,Alumni,Staff")]
        public ActionResult Details(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            var user = _userService.GetById(currentUserId);
            var application =  _applicationService.GetApplication(id);
            if (!application.found || !user.found)
            {
                return HttpNotFound();
            }

            var (secretary, hod, deptOfficer, appOwner) = _applicationService.DoCheck(application.result, user.result);

            if (!secretary && !(deptOfficer | hod) && !appOwner) return HttpNotFound();
            if (application.result.User.UserId == User.Identity.GetUserId() &&
                application.result.Status == ApplicationStatus.Rejected)
            {
                ViewBag.AddNewVersion = true;
            }

            if (User.IsInRole("Secretary"))
            {
                ViewBag.Secretary = true;
                if (application.result.Receivers.FirstOrDefault()?.Received == true 
                    && secretary && application.result.Archive == false && application.result.Approve == null)
                {
                    ViewBag.AssignUser = true;
                }
            }

            if (deptOfficer && application.result.Approve == null)
            {
                ViewBag.DeptOfficer = true;
                
            }

            if (!User.IsInRole("HOD")) return View(application.result);
            ViewBag.HOD = true;
            if (application.result.Approvals.Select(a => a.UserId).Contains(user.result.Id) && application.result.Approve == null)
            {
                ViewBag.Forward = true;
            }

            return View(application.result);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Secretary")]
        public  async Task<ActionResult> AssignUsers(AssignUsersVm model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("AssignUsers", model);
            }

            var (assign, msg, application) = await _applicationService.AssignUsers(model);

            if (assign)
            {
                Response.StatusCode = 201;
                return PartialView("_Assignees", application);
            }
            ModelState.AddModelError("", msg);
            return PartialView("AssignUsers", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignDepts(AssignDeptsVm model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("SendToDepts", model);
            }

            var (assign, msg, application) = await _applicationService.AssignToDept(model);

            if (assign)
            {
                Response.StatusCode = 201;
                return PartialView("_FwdDepts", application.Receivers);
            }

            ModelState.AddModelError("", msg);
            return PartialView("SendToDepts", model);

           
        }

        public ActionResult  Archive(int id)
        {

            var (archive, msg) = _applicationService.Archive(id);

            if (archive) return RedirectToAction("Archived");

            TempData["Msg"] = msg;

            return RedirectToAction("Details", new { id });

        }

        public FileContentResult GetFile(int id, string fileName)
        {
            
            var file = _applicationService.GetFile(id, fileName);
            if (file == null)
            {
                return null;
            }
            Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);
            return File(file.FileContent.Content, file.ContentType);

        }

        private void RetainFormState(ApplicationVm model, AppUser user)
        {
            if (User.IsInRole("Student"))
            {
                model.ApplicationTypes = _applicationService.GetApplicationTypes(Designation.Student);
                model.DepartmentId = user?.DepartmentId ?? 0;
            }
            else if (User.IsInRole("Alumni"))
            {
                model.ApplicationTypes = _applicationService.GetApplicationTypes(Designation.Alumni);
                model.Departments = _departmentService.GetAllDepartmentToList();

            }
            else if (User.IsInRole("Staff"))
            {
                model.ApplicationTypes = _applicationService.GetApplicationTypes(Designation.Staff);
                model.Departments = _departmentService.GetAllDepartmentToList();
            }
        }

    }
}