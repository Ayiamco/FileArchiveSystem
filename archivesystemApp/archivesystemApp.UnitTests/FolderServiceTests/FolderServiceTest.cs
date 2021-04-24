using Moq;
using NUnit.Framework;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Entities;
using archivesystemWebUI.Services;
using archivesystemDomain.Services;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.FolderModels;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using archivesystemWebUI.Interfaces;

namespace archivesystemApp.UnitTests.FolderServiceTests
{
    [TestFixture]
    public class FolderServiceUnitTests
    {
        private Mock<IFolderServiceUnitOfWork> _repo;
        private FolderService _service;
        private CreateFolderViewModel _editModel;
        private Folder _editFolderInDb;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IFolderServiceUnitOfWork>();
            _service = new FolderService(_repo.Object);
            _editModel = new CreateFolderViewModel { Name = GlobalConstants.RootFolderName, Id = 1, AccessLevelId = 1 };
            _editFolderInDb = new Folder { Id = _editModel.Id, Name = _editModel.Name, AccessLevelId = 1 };
        }
        

        [Test]
        public void DoesUserHasAccessToFolder_FolderIsRootFolderAndUserExist_ReturnsTrue()
        {
            var userData = new UserData { User = new AppUser { DepartmentId = 1 }, UserAccessLevel = 1 };
            var folder = new Folder { AccessLevelId = 1, Department = null, Faculty = null, Name = GlobalConstants.RootFolderName };

            var result = _service.DoesUserHasAccessToFolder(folder, userData);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void DoesUserHasAccessToFolder_UserDoesNotExist_ReturnsFalse()
        {
            var userData = new UserData { User = null };
            var folder = new Folder { AccessLevelId = 1, DepartmentId = 1, Department = new Department { Id = 1 } };

            var result = _service.DoesUserHasAccessToFolder(folder, userData);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void DoesUserHasAccessToFolder_FolderIsFacuLtyFolderAndUserIsInFaculty_ReturnsTrue()
        {
            var userData = new UserData { User = new AppUser { Department = new Department { FacultyId = 1 } }, UserAccessLevel = 1 };
            var folder = new Folder { FacultyId = 1, AccessLevelId = 1 };

            var result = _service.DoesUserHasAccessToFolder(folder, userData);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void DoesUserHasAccessToFolder_FolderIsFacuLtyFolderAndUserIsNotInFaculty_ReturnsTrue()
        {
            var userData = new UserData { User = new AppUser { Department = new Department { FacultyId = 3 } }, UserAccessLevel = 1 };
            var folder = new Folder { FacultyId = 1, AccessLevelId = 1 };

            var result = _service.DoesUserHasAccessToFolder(folder, userData);
            
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void DoesUserHasAccessToFolder_FolderIsDepartmentalFolderAndUserIsNotInDepartment_ReturnsTrue()
        {
            var userData = new UserData { User = new AppUser { Department = new Department { Id = 3 } }, UserAccessLevel = 1 };
            var folder = new Folder { DepartmentId = 1, AccessLevelId = 1 };

            var result = _service.DoesUserHasAccessToFolder(folder, userData);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void DoesUserHasAccessToFolder_FolderIsDepartmentalFolderAndUserIsInDepartment_ReturnsTrue()
        {
            var userData = new UserData { User = new AppUser { DepartmentId = 1 }, UserAccessLevel = 1 };
            var folder = new Folder { DepartmentId = 1, AccessLevelId = 1, Faculty = null };

            var result = _service.DoesUserHasAccessToFolder(folder, userData);

            Assert.That(result, Is.EqualTo(true));
        }


        [Test]
        public void FilterFolderSubFoldersUsingAccessLevel_WhenCalled_ReturnsFolderWithFilteredSufolders()
        {
            //Arrange
            var accessLevel = 2;
            _editFolderInDb.Subfolders = new List<Folder>
            {
                new Folder {AccessLevelId=2},
                new Folder {AccessLevelId=1},
                new Folder {AccessLevelId=2},
                new Folder {AccessLevelId=3},
            };

            //Act
            var result = _service.FilterFolderSubFoldersUsingAccessLevel(_editFolderInDb, accessLevel);

            //Assert
            Assert.That(result.Subfolders.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetCreateFolderViewModel_ParentFolderIsNotFound_ReturnNull()
        {
            var parentFolderId = 3;
            var userId = "dummuy id";
            _repo.Setup(r => r.FolderRepo.Get(parentFolderId)).Returns<Folder>(null);

            var result = _service.GetCreateFolderViewModel(parentFolderId, userId);

            Assert.That(result, Is.EqualTo(null));
            _repo.Verify(r => r.FolderRepo.Get(parentFolderId));
        }

        [Test]
        public void GetCreateFolderViewModel_UserIsNotFound_ReturnNull()
        {
            var parentFolderId = 3;
            var userId = "dummuy id";
            var parentFolder = new Folder { Id = 3 };
            _repo.Setup(r => r.FolderRepo.Get(parentFolderId)).Returns(parentFolder);
            _repo.Setup(r => r.UserRepo.FindWithNavProps(c => c.UserId == userId, _ => _.Department))
                .Returns(new List<AppUser>());
            var result = _service.GetCreateFolderViewModel(parentFolderId, userId);

            Assert.That(result, Is.EqualTo(null));
            _repo.Verify(r => r.FolderRepo.Get(parentFolderId));
            _repo.Verify(r => r.UserRepo.FindWithNavProps(c => c.UserId == userId, _ => _.Department));
        }

        [Test]
        public void GetCreateFolderViewModel_UserAcceesDetailsIsNotFound_ReturnNull()
        {
            //Arrange
            var parentFolderId = 3;
            var userId = "dummuy id";
            var user = new AppUser { UserId = userId, Id = 1 };
            var parentFolder = new Folder { Id = 3 };
            _repo.Setup(r => r.FolderRepo.Get(parentFolderId)).Returns(parentFolder);
            _repo.Setup(r => r.UserRepo.FindWithNavProps(c => c.UserId == userId, _ => _.Department))
                .Returns(new List<AppUser> { user });
            _repo.Setup(r => r.AccessDetailsRepo.Find(c => c.AppUserId == user.Id))
                .Returns(new List<AccessDetail>());

            //Act
            var result = _service.GetCreateFolderViewModel(parentFolderId, userId);

            //Assert
            Assert.That(result, Is.Null);
            _repo.Verify(r => r.FolderRepo.Get(parentFolderId));
            _repo.Verify(r => r.UserRepo.FindWithNavProps(c => c.UserId == userId, _ => _.Department));
            _repo.Verify(r => r.AccessDetailsRepo.Find(c => c.AppUserId == user.Id));
        }

        
    }
}
