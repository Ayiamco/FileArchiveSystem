using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemDomain.Services;
using archivesystemWebUI.Infrastructures;
using archivesystemWebUI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace archivesystemWebUI.Repository
{
    public class AccessLevelRepository: Repository<AccessLevel>, IAccessLevelRepository
    {
        private readonly ApplicationDbContext _context;

        public AccessLevelRepository(ApplicationDbContext context)
            :base(context)
        {
            _context = context;
        }

        /// <summary>
        /// This method accepts an "AccessLevel" object and modifies an existing data in the AccessLevels table.
        /// </summary>
        /// <param name="accessLevel">AccessLevel</param>
        public void EditDetails(AccessLevel accessLevel)
        {
            accessLevel.UpdatedAt = DateTime.Now;
            _context.Entry(accessLevel).State = EntityState.Modified;
        }

        /// <summary>
        /// This method retrieves an "AccessLevel" object using the "Level" parameter of the table
        /// </summary>
        /// <param name="Level">string</param>
        /// <returns>AccessLevel</returns>
        public AccessLevel GetByLevel(string Level)
        {
            return _context.AccessLevels.FirstOrDefault(m => m.Level == Convert.ToInt32(Level));
        }
        public AccessLevel GetBaseLevel()
        {
            return _context.AccessLevels.SingleOrDefault(x => x.Level == AccessLevelNames.BaseLevel);
        }
    }
}