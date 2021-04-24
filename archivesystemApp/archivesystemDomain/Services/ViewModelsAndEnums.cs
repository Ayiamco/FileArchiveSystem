using archivesystemDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace archivesystemDomain.Services
{
    public class AccessLevelNames
    {
        public const string BaseLevel = "1";
    }

    public enum AllowableFolderDepth
    {
        Max=7
    }

    public class FolderPath
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RoleMemberData
    {
        public string UserId { get; set; }
        public Department Department { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class GlobalConstants
    {
        public const string IsAccessValidated = "IsAccessValidated";
        public const string LastVisit = "LastVisit";
        public const string userHasNoAccesscode = " userHasNoAccesscode";
        public const string RootFolderName = "Root";
        public const int MaxFolderDepth = 7;
        public  const byte LOCKOUT_TIME = 100;

    }

    public class RoleNames
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string FacultyOfficer = "FacultyOfficer";
        public const string DeptOfficer="DeptOfficer";
        public const string HOD="HOD";
        public const string AgHOD="AgHOD";
        public const string Secretary="Secretary";
        public const string FolderAllowedRoles = "Admin,FacultyOfficer,DeptOfficer";

        
    }

    public enum ServiceResult
    {
        Succeeded,
        Failure,
        Prohibited
    }

    public class SessionData
    {
        public const string  AccessLevel= "AccessLevel";
        public const string  LastRequestTime= "LastRequestTime";
        public const string AccessValidated = "AccessValidated";
        public const string DeptId = "DeptId";
        public const string FacultyId = "FacultyId";
        public const string OTP = "OTP";
    }
}