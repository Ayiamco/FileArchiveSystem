using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace archivesystemWebUI.Services
{
    public class UserAccessService : IUserAccessService
    {
        #region FIELDS
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccessCodeGenerator _accessCode;
        private readonly IAccessDetailsRepository _accessDetailsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAccessLevelRepository _accessLevelRepository;
        private const string add = "add";
        private const string update = "update";
        #endregion

        #region PROPERTIES
        public IEnumerable<AccessLevel> AccessLevels { get { return _accessLevelRepository.GetAll(); } }
        public IEnumerable<AccessDetail> AccessDetails { get { return _accessDetailsRepository.GetAccessDetails(); } }
        #endregion

        #region CONSTRUCTOR
        public UserAccessService(IUnitOfWork unitOfWork, IAccessCodeGenerator accessCode, IAccessDetailsRepository accessDetailsRepository, IUserRepository userRepository, IAccessLevelRepository accessLevelRepository)
        {
            _unitOfWork = unitOfWork;
            _accessCode = accessCode;
            _accessDetailsRepository = accessDetailsRepository;
            _userRepository = userRepository;
            _accessLevelRepository = accessLevelRepository;
        }
        #endregion

        #region MAIN METHODS
        public async Task<(string, Exception)> AddUser(AddUserToAccessViewModel model)
        {
            try
            {
                var user = _userRepository.GetUserByMail(model.Email);

                var newAccessDetails = new AccessDetail
                {
                    AppUserId = user.Id,
                    AccessLevelId = model.AccessLevel,
                    Status = Status.Active,
                    CreatedAt=DateTime.Now,
                    UpdatedAt=DateTime.Now,
                };

                _accessDetailsRepository.Add(newAccessDetails);
                await _unitOfWork.SaveAsync();

                return ("success", null);
            }
            catch (Exception e)
            {
                return ("failure", e);
            }

        }

        public AppUser GetUserByEmail(string email)
        {
            return _userRepository.GetUserByMail(email);
        }

        public AccessDetail GetByAppUserId(int appUserId)
        {
            return _accessDetailsRepository.GetByAppUserId(appUserId);
        }


        public AccessDetail GetByNullableId(int? id)
        {
            return _accessDetailsRepository.Get(id.Value);

        }

        public AddUserToAccessViewModel AddUserModel()
        {
            return new AddUserToAccessViewModel { AccessLevels = AccessLevels };
        }

        public void EditUserModel(int id, out EditUserViewModel model, out AccessDetail accessDetails)
        {
            accessDetails = _accessDetailsRepository.Get(id);

            model = new EditUserViewModel
            {
                RegenerateCode = CodeStatus.No,
                AccessDetails = accessDetails,
                AccessLevels = AccessLevels
            };

        }

        public async Task<(string, Exception)> UpdateUser(EditUserViewModel model)
        {
            try
            {
                if (model.RegenerateCode == CodeStatus.Yes)
                {
                    var user = _userRepository.Get(model.AccessDetails.AppUserId);
                }

                _accessDetailsRepository.EditDetails(model.AccessDetails);
                await _unitOfWork.SaveAsync();

                return ("success", null);
            }
            catch (Exception e)
            {
                return ("failure", e);
            }
        }

        public async Task<string> Delete(int id)
        {
            try
            {
                var accessDetails = _accessDetailsRepository.Get(id);

                _accessDetailsRepository.Remove(accessDetails);
                await _unitOfWork.SaveAsync();

                return "success";
            }
            catch (Exception)
            {
                return "failure";
            }
        }
        #endregion


    }
}