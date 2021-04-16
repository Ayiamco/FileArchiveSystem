using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archivesystemDomain.Entities
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public int? FileContentId { get; set; }
        public FileContent FileContent { get; set; }
        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int? AccessLevelId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public string UploadedById { get; set; }
        public ApplicationUser UploadedBy { get; set; }


    }
}
