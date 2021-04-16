using archivesystemDomain.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace archivesystemWebUI.Infrastructures.CustomFilters
{
    public sealed class ValidateAccess : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");
            var httpContext = filterContext.HttpContext;
            if (httpContext.Session[SessionData.AccessLevel] == null)
            {
                httpContext.Response.Redirect("/account/login?returnUrl="+httpContext.Request.RawUrl);
                filterContext.Result = new EmptyResult();
                return;
            }
            LogData();
            var hasCorrectAcessCode = httpContext.Session[SessionData.AccessValidated]!=null;
            var lastRequestTime =  httpContext.Session[SessionData.LastRequestTime] == null ?
               DateTime.Parse("01/01/1970") : (DateTime?)HttpContext.Current.Session[SessionData.LastRequestTime];

            var timeDiff = DateTime.Now - lastRequestTime;
            if (timeDiff> new TimeSpan(0,GlobalConstants.LOCKOUT_TIME,0) || !hasCorrectAcessCode)
            {
                httpContext.Response.Redirect("/folders?returnUrl="+ httpContext.Request.RawUrl);
                filterContext.Result = new EmptyResult();
            }
           

        }

        private void LogData()
        {

            var httpContext = HttpContext.Current;
            Debug.WriteLine($"{SessionData.AccessLevel}: " + (int)httpContext.Session[SessionData.AccessLevel]);
            Debug.WriteLine($"{SessionData.DeptId}: " + httpContext.Session[SessionData.DeptId]);
            Debug.WriteLine($"{SessionData.FacultyId}: " + httpContext.Session[SessionData.FacultyId]);
        }
    }

    
}