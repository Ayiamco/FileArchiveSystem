using archivesystemWebUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using archivesystemDomain.Services;
using Ninject;
using archivesystemWebUI.Services;
using archivesystemWebUI.Repository;

namespace archivesystemWebUI.Infrastructures
{
    public class SetSessionData
    {

        public static void Set(IFolderService _folderService, IUserService _userService)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            if (userId == null) return;
            var user = _userService.GetById(userId);
            var userAccessLevel = _folderService.GetCurrentUserAccessLevel(userId);
            HttpContext.Current.Session[SessionData.AccessLevel] = userAccessLevel;
            HttpContext.Current.Session[SessionData.DeptId] = user.result?.DepartmentId;
            HttpContext.Current.Session[SessionData.FacultyId] = user.result?.Department?.FacultyId;
        }
    }
}