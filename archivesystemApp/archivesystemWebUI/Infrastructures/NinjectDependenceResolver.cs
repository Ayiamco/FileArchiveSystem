using archivesystemWebUI.Models;
using Ninject;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using archivesystemDomain;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Repository;
using archivesystemWebUI.Services;
using Ninject.Web.Common;
using archivesystemWebUI.Interfaces;
using AutoMapper;

namespace archivesystemWebUI.Infrastructures
{
    public class NinjectDependenceResolver : IDependencyResolver
    {
        private readonly IKernel kernel;
        public NinjectDependenceResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            kernel.Bind(typeof(ApplicationDbContext)).ToSelf().InRequestScope();
            kernel.Bind<IUserRepository>().To<UserRepository>();
            kernel.Bind<IDeptRepository>().To<DeptRepository>();
            kernel.Bind<IAccessDetailsRepository>().To<AccessDetailsRepository>();
            kernel.Bind<IAccessLevelRepository>().To<AccessLevelRepository>();
            kernel.Bind<ITokenRepo>().To<TokenRepository>();
            kernel.Bind<IRoleService>().To<RoleService>();
            kernel.Bind<ITokenGenerator>().To<TokenGenerator>();
            kernel.Bind<IEmailSender>().To<EmailSender>();
            kernel.Bind<IFolderRepo>().To<FolderRepo>();
            kernel.Bind<IUnitOfWork>().To<UnitOfWork>();
            kernel.Bind<IFacultyRepository>().To<FacultyRepository>();
            kernel.Bind<IFileMetaRepo>().To<FileMetaRepo>();
            kernel.Bind<IFacultyService>().To<FacultyService>();
            kernel.Bind<IDepartmentService>().To<DepartmentService>();
            kernel.Bind<IAccessLevelService>().To<AccessLevelService>();
            kernel.Bind<IUserAccessService>().To<UserAccessService>();
            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<IFileService>().To<FileService>();
            kernel.Bind<IFileRepo>().To<FileRepo>();
            kernel.Bind<IFolderService>().To<FolderService>();
            kernel.Bind<IAccessCodeGenerator>().To<AccessCodeGenerator>();
            kernel.Bind<IHomeService>().To<HomeService>();
            kernel.Bind<IFolderServiceUnitOfWork>().To<FolderServiceUnitOfWork>();
        }
    }
}