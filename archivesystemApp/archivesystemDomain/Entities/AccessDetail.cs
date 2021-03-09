﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archivesystemDomain.Entities
{
    public class AccessDetail
    {
        public int Id { get; set; }   
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int AccessLevelId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public string AccessCode { get; set; }
        public Status Status { get; set; }
    }

}