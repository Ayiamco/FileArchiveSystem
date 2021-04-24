using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archivesystemDomain.Entities
{
    public enum Status
    {
        Active, Inactive
    }

    public class AccessDetail
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int AccessLevelId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt {get;set;}
        public DateTime UpdatedAt { get; set; }
    }

}
