﻿using System;
using System.Collections.Generic;
using System.Text;

namespace archivesystemDomain.Interfaces
{
    public interface IUnitOfWork
    {
        IStaffRepository StaffRepo { get; }
    }
}