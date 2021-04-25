using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using archivesystemDomain.Entities;
using Microsoft.AspNet.Identity.Owin;

namespace archivesystemDomain.Services
{
    public  class SeedAppData
    {

        public static void EnsurePopulated()
        {
            var dbContext = new ApplicationDbContext();
            if (!dbContext.Folders.Any())
            {
                //Create non academic staff folder
                Faculty faculty = new Faculty
                {
                    Name = "Non Academic",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Departments = new List<Department>
                        {
                            new Department
                            {
                                Name = "Maintenanace",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            }
                        }
                };

                //create base acessLevel 
                var baseLevel = new AccessLevel
                {
                    Level = 1,
                    LevelDescription = "Base level",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                //create Root folder
                var rootFolder = new Folder
                {
                    Name = GlobalConstants.RootFolderName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsRestricted = true,
                    AccessLevel = baseLevel,
                    Subfolders = new List<Folder>
                    {
                        new Folder
                        {
                            Name = faculty.Name,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            AccessLevelId = 1,
                            IsRestricted = true,
                            Faculty = faculty

                        }
                    }
                };

                //seed the root folder,faculties , departments and their folders
                dbContext.Folders.Add(rootFolder);

                //Seed AccessLevels
                var accessLevels = GetAccessLevels();
                dbContext.AccessLevels.AddRange(accessLevels);

                //save changes
                dbContext.SaveChanges();
            };
        }

        private static List<AccessLevel> GetAccessLevels()
        {
            var accessLevels = new List<AccessLevel>();
            for (int i=2; i<15; i++)
            {
                new AccessLevel
                {
                    Level = i,
                    LevelDescription = "Access Level " + i,
                    CreatedAt=DateTime.Now,
                    UpdatedAt=DateTime.Now,
                };
            }
            return accessLevels;
        }


    }
}
          

           


        
    
       

    
