using archivesystemDomain.Services;
using archivesystemWebUI.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace archivesystemWebUI.Infrastructures.CustomFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CheckSessionData:FilterAttribute,IAuthorizationFilter
    {
        private  IFolderService _folderService;
        private  IUserService _userService;
        public CheckSessionData(IUserService userService, IFolderService folderService)
        {
            _folderService = folderService;
            _userService = userService;
        }
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            var httpContext = filterContext.HttpContext;
            if (httpContext.Session[SessionData.AccessLevel]==null)
            {
                SetSessionData.Set(_folderService,_userService);
            }
        }
    }
}