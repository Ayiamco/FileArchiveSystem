using archivesystemDomain.Entities;
using archivesystemDomain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace archivesystemWebUI.Models.FolderModels
{
    public class FolderPageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public List<FolderPath> CurrentPath { get; set; }

        public ICollection<Folder> DirectChildren { get; set; }
  

        public string ReturnUrl { get; set; }
        public bool RequestForOTP { get; set; }
    }
}