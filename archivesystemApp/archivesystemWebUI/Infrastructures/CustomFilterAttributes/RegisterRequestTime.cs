using archivesystemDomain.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace archivesystemWebUI.Infrastructures.CustomFilters
{
    public sealed class RegisterRequestTimeAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var time = context.RequestContext.HttpContext.Session[SessionData.LastRequestTime] == null ?
                DateTime.Parse("01/01/1990") :
                (DateTime?)HttpContext.Current.Session[SessionData.LastRequestTime];

            LogLastRequestTime(time);
            context.RequestContext.HttpContext.Session[SessionData.LastRequestTime] = DateTime.Now;
        }

        private void LogLastRequestTime(DateTime? time) 
        {
            Debug.WriteLine($"Last Request time: {time}");
        }
    }
   
}