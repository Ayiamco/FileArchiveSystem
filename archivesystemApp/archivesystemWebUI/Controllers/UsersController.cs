using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Infrastructures;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using static System.String;

namespace archivesystemWebUI.Controllers
{

    [Authorize]
    public class UsersController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
       
       


        public UsersController(
            IUserService userService,
            IDepartmentService departmentService
            )
        {
            _userService = userService;
            _departmentService = departmentService;
        }
        public UsersController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        [Authorize(Roles = "Admin")]
        // GET: Employees
        public ActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public ActionResult EditModal(int id)
        {
            var (found, result) = _userService.Get(id);
            if (!found)
            {
                return HttpNotFound();
            }

            var vm = Mapper.Map<UpdateUserVm>(result);
            vm.Departments = _departmentService.GetAllDepartmentToList();
            return PartialView(vm);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EnrollModal()
        {
            var model = new EnrollViewModel
            {
                Departments = _departmentService.GetAllDepartmentToList()
            };

            return PartialView(model);
        }

        
        public ActionResult Details(int id)
        {
            var (found, result) = _userService.Get(id);
            if (!found)
            {
                return HttpNotFound();
            }
            return View(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enroll( EnrollViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = _departmentService.GetAllDepartmentToList();
                return PartialView("EnrollModal", model);
            }

            var uniqueProps = Mapper.Map<EnrollViewModel, UserUniqueProps>(model);
            var user = Mapper.Map<EnrollViewModel, AppUser>(model);

            var (save, msg) = await _userService.Create(user, uniqueProps);
            if (save)
            {
                return Json(new { status = msg });
            }

            ModelState.AddModelError("", msg);
            model.Departments =
                _departmentService.GetAllDepartmentToList();
            return PartialView("EnrollModal", model);

        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UpdateUserVm model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = _departmentService.GetAllDepartmentToList();
                return PartialView("EditModal", model);
            }
           
            var (save, msg) = _userService.Update(model);
            if (save)
            {
                return Json(new { status = msg, msg = $"{model.FirstName}" });
            }


            ModelState.AddModelError("", msg);
            model.Departments = _departmentService.GetAllDepartmentToList();
            return PartialView("EditModal", model);

        }



        public JsonResult IsEmailTaken(string email, int? id = null)
        {
            // Check if the e-mail already exists
            return Json(!_userService.EmailExists(email, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsPhoneTaken(string phone, int? id = null)
        {
            // Check if the phone already exists
            return Json(!_userService.PhoneExists(phone, id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsNameTaken(string firstName, string lastName, int? id = null)
        {
            // Check if the Name already exists
            return Json(!_userService.NameExists($"{firstName} {lastName}", id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsIdTaken(string tagId, int? id =null)
        {
            // Check if the staffId already exists
            return Json(!_userService.TagIdExists(tagId, id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DelPrompt(int id)
        {
            var (found, result) = _userService.Get(id);
            if (!found)
            {
                return HttpNotFound();
            }

            return PartialView(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResendConfirmation(int id, string email, string name)
        {

            var (sent, msg) = await _userService.ResendConfirmation(id, email, name);

            if (sent)
            {
                TempData["ConfSent"] = msg;
                return RedirectToAction("Details", new { id = id });
            }

            TempData["ConfFailed"] = msg;
            return RedirectToAction("Details", new { id = id });
        }

       
        public  async Task<ActionResult> Officers(int id , string searchTerm)
        {

            var deptOfficers = await _userService.GetDeptOfficers(id, searchTerm);
            return Json(deptOfficers.result, JsonRequestBehavior.AllowGet);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }
            }


            base.Dispose(disposing);
        }

    }
}