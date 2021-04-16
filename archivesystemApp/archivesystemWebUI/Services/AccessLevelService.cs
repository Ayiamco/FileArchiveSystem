using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemWebUI.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace archivesystemWebUI.Services
{
    public class AccessLevelService : IAccessLevelService
    {
        #region FIELD
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccessLevelRepository _accessLevelRepository;
        #endregion

        #region CONSTRUCTOR
        public AccessLevelService(IUnitOfWork unitOfWork, IAccessLevelRepository accessLevelRepository)
        {
            _unitOfWork = unitOfWork;
            _accessLevelRepository = accessLevelRepository;

        }
        #endregion

        #region MAIN METHODS
        public IEnumerable<AccessLevel> GetAll()
        {
            return _accessLevelRepository.GetAll();
        }

        public async Task Create(AccessLevel newAccess)
        {
            _accessLevelRepository.Add(newAccess);
            await _unitOfWork.SaveAsync();
        }

        public AccessLevel GetById(int id)
        {
            return _accessLevelRepository.Get(id);
        }

        public async Task Update(AccessLevel accessLevel)
        {
            _accessLevelRepository.EditDetails(accessLevel);
            await _unitOfWork.SaveAsync();
        }

        public bool CheckLevel(string Level)
        {
            var checkLevel = _accessLevelRepository.GetByLevel(Level);
            return checkLevel == null;
        } 
        #endregion
    }
}