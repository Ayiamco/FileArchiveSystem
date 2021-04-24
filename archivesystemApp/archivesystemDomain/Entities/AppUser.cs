using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace archivesystemDomain.Entities
{
    public enum Designation
    {
        Student = 1,
        Alumni,
        Staff,
    }
    public class AppUser
    {
        public int Id { get; set; }

        [Index("UserIdIndex")]
        [StringLength(50)]
        public string UserId { get; set; }

        [Index("EmailIndex", IsUnique = true)]
        [StringLength(50)]
        public string Email { get; set; }

        [Index("NameIndex", IsUnique = true)]
        [StringLength(50)]
        public string Name { get; set; }
        public Gender Gender { get; set; }
        [StringLength(50)]
        public string Phone { get; set; }
        [Index("TagIdIndex", IsUnique = true)]
        [StringLength(50)]
        public string TagId { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public File Signature { get; set; }
        public Designation Designation { get; set; }
        public int? UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
        public bool Completed { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
