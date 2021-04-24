using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Infrastructures;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace archivesystemWebUI.Services
{
    public class UserService : IUserService
    {
        private ApplicationUserManager _userManager;
       
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IRoleService _roleService;
        private readonly IEmailSender _emailSender;
        private readonly ITokenRepo _tokenRepo;
        private readonly ITokenGenerator _tokenGenerator;
        private static HttpContext Context => HttpContext.Current;
        private readonly UrlHelper _url = new UrlHelper(Context.Request.RequestContext);

        public UserService(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IRoleService roleService,
            IEmailSender emailSender,
            ITokenRepo tokenRepo,
            ITokenGenerator tokenGenerator)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _roleService = roleService;
            _emailSender = emailSender;
            _tokenRepo = tokenRepo;
            _tokenGenerator = tokenGenerator;
        }

        public UserService(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
          
        }

        private ApplicationUserManager UserManager
        {
            get => _userManager ?? Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            set => _userManager = value;
        }
        public async Task<(bool save, string msg)> Create(AppUser user, UserUniqueProps uniqueProps)
        {

            var (msg, propExists) = Validate(uniqueProps);

            if (propExists)
            {
                return (false, msg);
            }

            try
            {
                _userRepository.Add(user);

                var code = _tokenGenerator.Code(user.Id);

                await _unitOfWork.SaveAsync();

                var callbackUrl = _url.Action("Register", "Account", new { userId = user.Id, code }, protocol: Context.Request.Url.Scheme);


                await  _emailSender
                    .SendEmailAsync(user.Email,
                        "Complete Your Registration",
                        $"Hi, {user.Name} complete your Enrollment Process by clicking" +
                        " <a href=" + callbackUrl + ">here</a>");
                return (true, "success");

            }
            catch (Exception e)
            {
              
               return (false, $"{e.Message}, {e.InnerException}");
            }

        }

        public (bool save, string msg) Update(UpdateUserVm model )
        {
            var userDb = _userRepository.Get(model.Id);

            if (userDb == null)
            {
                return (false, "Entity To Update Not Found!");
            }

            var name = $"{model.FirstName} {model.LastName}";

            if (!string.IsNullOrWhiteSpace(userDb.UserId))
            {
                var appUser = UserManager.FindById(userDb.UserId);

                if (!string.Equals(model.Email, appUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.Email = model.Email;
                }

                if (!string.Equals(name, appUser.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.UserName = name;
                }

                if (!string.Equals(model.Phone, appUser.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.PhoneNumber = model.Phone;
                }

                UserManager.Update(appUser);
            }

            Mapper.Map(model, userDb);
            userDb.UpdatedAt = DateTime.Now;
            _unitOfWork.Save();

            return (true, "success");

        }


        public bool UpdateUserId(string email, string id)
        {
            var user = _userRepository.GetUserByMail(email);
            if (user == null)
            {
                return false;
            }

            user.UserId = id;
            return true;
        }

        public void CompleteReg(AppUser user)
        {
            user.Completed = true;
            _unitOfWork.Save();
        }
        public (bool found, AppUser result) Get(int id)
        {
            var user = _userRepository.FindWithNavProps(u => u.Id == id, _ => _.Department).SingleOrDefault();
            if (user != null) return (true, user);
            return (false, null);
        }

        public (bool found, AppUser result) GetById(string id)
        {
            var user = _userRepository.FindWithNavProps(u => u.UserId == id, _ => _.Department).SingleOrDefault();
            if (user != null) return (true, user);
            return (false, null);
        }
        public (bool found, AppUser result) GetByMail(string email)
        {
            var user = _userRepository.FindWithNavProps(u => u.Email == email, _ => _.Department).SingleOrDefault();

            if (user != null) return (true, user);

            return (false, null);
        }
        public IEnumerable<AppUser> GetAll()
        {
            return _userRepository.GetAllWithNavProps(_ => _.Department);
        }

        private (string msg, bool exists) Validate(UserUniqueProps uniqueProps, int? userId = null)
        {
           
            if (NameExists(uniqueProps.Name, userId))
            {
                return ("Name Already taken", true);
            }

            if (EmailExists(uniqueProps.Email, userId))
            {
                return ("Email Already Exists", true);
            }

            if (TagIdExists(uniqueProps.TagId, userId))
            {
                return ("Already Exists", true);
            }
            return PhoneExists(uniqueProps.Phone, userId) ? ("Phone Number Already Exists", true) : ("", false);
        }


        public async Task<(bool sent, string msg)> ResendConfirmation(int id, string email, string name)
        {
            var code = _tokenGenerator.Code(id);

           await _unitOfWork.SaveAsync();

           var callbackUrl = _url.Action("Register", "Account", new { userId = id, code }, protocol: Context.Request.Url.Scheme);
          
            try
            {
                await _emailSender.SendEmailAsync(email,
                    "Complete Your Registration", "Hi," + 
                    $" {name} complete your Enrollment Process by" +
                    " clicking <a href=" + callbackUrl + ">here</a>");

                return (true, "Confirmation Link Sent");
            }
            catch (Exception e)
            {
                return (false, $"{e.Message}");
            }

        }

        public Token GetToken(string code)
        {
            return _tokenRepo.Find(t => t.Code == code).SingleOrDefault();
        }

        public bool Remove(int id)
        {
            var employee = _userRepository.Get(id);
            if (employee == null)
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(employee.UserId))
            {
                var iUser = UserManager.FindById(employee.UserId);
                UserManager.Delete(iUser);
            }
            _userRepository.Remove(employee);
            _unitOfWork.Save();
            return true;
        }

        public void RemoveToken(Token token)
        {
            _tokenRepo.Remove(token);
            _unitOfWork.Save();
        }
        public async Task<(bool found, IEnumerable result)> GetDeptOfficers(int id, string searchTerm)
        {
            var currentUserId = Context.User.Identity.GetUserId();
            var officersInDept = await _roleService.GetUserIdsOfUsersInRole("DeptOfficer");
            // var assocApp = _applicationRepo.FindWithNavProps(a => a.Id == id, _ => _.User).SingleOrDefault();
            if (officersInDept  == null)
            {
                return (false, null);
            }

            var users = _userRepository.Find(u => u.DepartmentId == id
                                                  && officersInDept.Contains(u.UserId)
                                                  && u.UserId != currentUserId
                                                  && u.Name.Contains(searchTerm)
                ).Select(x => new
                {
                    id = x.Id,
                    text = x.Name
                });


            if (string.IsNullOrWhiteSpace(searchTerm))
            {

                users = _userRepository.Find(
                    u => u.DepartmentId == id
                         && officersInDept.Contains(u.UserId)
                         && u.UserId != currentUserId
                ).Select(x => new
                {
                    id = x.Id,
                    text = x.Name
                });
            }

            return (true, users);
        }

       
        public bool NameExists(string name, int? userId)
        {
            if (userId == null)
                return _userRepository.GetAll().Any(e => string.Equals(e.Name, name,
                    StringComparison.OrdinalIgnoreCase));

            return _userRepository.GetAll().Any(e => string.Equals(e.Name, name,
                StringComparison.OrdinalIgnoreCase) && e.Id != userId.Value);
        }


        public bool EmailExists(string email, int? userId)
        {

            if (userId == null)
                return _userRepository.GetAll().Any(e => string.Equals(e.Email, email,
                    StringComparison.OrdinalIgnoreCase));
            return GetAll().Any(e => string.Equals(e.Email, email,
                StringComparison.OrdinalIgnoreCase) && e.Id != userId.Value);
        }


        public bool TagIdExists(string tagId, int? userId)
        {

            if (userId == null)
                return GetAll().Any(e => string.Equals(e.TagId, tagId,
                    StringComparison.OrdinalIgnoreCase));

            return GetAll().Any(e => string.Equals(e.TagId, tagId,
                StringComparison.OrdinalIgnoreCase) && e.Id != userId.Value);
        }

        public bool PhoneExists(string phone, int? userId)
        {
            if (userId == null)
                return GetAll().Any(e => string.Equals(e.Phone, phone,
                    StringComparison.OrdinalIgnoreCase));
            return GetAll().Any(e => string.Equals(e.Phone, phone,
                StringComparison.OrdinalIgnoreCase) && e.Id != userId.Value);
        }

    }
}