namespace archivesystemWebUI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class intialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccessDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppUserId = c.Int(nullable: false),
                        AccessLevelId = c.Int(nullable: false),
                        AccessCode = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccessLevels", t => t.AccessLevelId, cascadeDelete: true)
                .ForeignKey("dbo.AppUsers", t => t.AppUserId, cascadeDelete: true)
                .Index(t => t.AppUserId)
                .Index(t => t.AccessLevelId);
            
            CreateTable(
                "dbo.AccessLevels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Level = c.String(),
                        LevelDescription = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AppUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 50),
                        Email = c.String(maxLength: 50),
                        Name = c.String(maxLength: 50),
                        Gender = c.Int(nullable: false),
                        Phone = c.String(maxLength: 50),
                        TagId = c.String(maxLength: 50),
                        DepartmentId = c.Int(nullable: false),
                        Designation = c.Int(nullable: false),
                        UserProfileId = c.Int(),
                        Completed = c.Boolean(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Signature_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Departments", t => t.DepartmentId, cascadeDelete: true)
                .ForeignKey("dbo.Files", t => t.Signature_Id)
                .ForeignKey("dbo.UserProfiles", t => t.UserProfileId)
                .Index(t => t.UserId, name: "UserIdIndex")
                .Index(t => t.Email, unique: true, name: "EmailIndex")
                .Index(t => t.Name, unique: true, name: "NameIndex")
                .Index(t => t.TagId, unique: true, name: "TagIdIndex")
                .Index(t => t.DepartmentId)
                .Index(t => t.UserProfileId)
                .Index(t => t.Signature_Id);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        FacultyId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Faculties", t => t.FacultyId, cascadeDelete: true)
                .Index(t => t.FacultyId);
            
            CreateTable(
                "dbo.Faculties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Files",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ContentType = c.String(),
                        FileContentId = c.Int(),
                        FolderId = c.Int(),
                        AccessLevelId = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        UploadedById = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccessLevels", t => t.AccessLevelId)
                .ForeignKey("dbo.FileContents", t => t.FileContentId)
                .ForeignKey("dbo.Folders", t => t.FolderId)
                .ForeignKey("dbo.AspNetUsers", t => t.UploadedById)
                .Index(t => t.FileContentId)
                .Index(t => t.FolderId)
                .Index(t => t.AccessLevelId)
                .Index(t => t.UploadedById);
            
            CreateTable(
                "dbo.FileContents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        Content = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Folders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ParentId = c.Int(),
                        AccessLevelId = c.Int(),
                        DepartmentId = c.Int(),
                        FacultyId = c.Int(),
                        IsRestricted = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccessLevels", t => t.AccessLevelId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.Faculties", t => t.FacultyId)
                .ForeignKey("dbo.Folders", t => t.ParentId)
                .Index(t => t.ParentId)
                .Index(t => t.AccessLevelId)
                .Index(t => t.DepartmentId)
                .Index(t => t.FacultyId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SignatureId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Files", t => t.SignatureId, cascadeDelete: true)
                .Index(t => t.SignatureId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        CreatedAt = c.DateTime(),
                        UpDatedAt = c.DateTime(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppUserId = c.Int(nullable: false),
                        Code = c.String(),
                        Expire = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUsers", t => t.AppUserId, cascadeDelete: true)
                .Index(t => t.AppUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tokens", "AppUserId", "dbo.AppUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AccessDetails", "AppUserId", "dbo.AppUsers");
            DropForeignKey("dbo.AppUsers", "UserProfileId", "dbo.UserProfiles");
            DropForeignKey("dbo.UserProfiles", "SignatureId", "dbo.Files");
            DropForeignKey("dbo.AppUsers", "Signature_Id", "dbo.Files");
            DropForeignKey("dbo.Files", "UploadedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Folders", "ParentId", "dbo.Folders");
            DropForeignKey("dbo.Files", "FolderId", "dbo.Folders");
            DropForeignKey("dbo.Folders", "FacultyId", "dbo.Faculties");
            DropForeignKey("dbo.Folders", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Folders", "AccessLevelId", "dbo.AccessLevels");
            DropForeignKey("dbo.Files", "FileContentId", "dbo.FileContents");
            DropForeignKey("dbo.Files", "AccessLevelId", "dbo.AccessLevels");
            DropForeignKey("dbo.AppUsers", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Departments", "FacultyId", "dbo.Faculties");
            DropForeignKey("dbo.AccessDetails", "AccessLevelId", "dbo.AccessLevels");
            DropIndex("dbo.Tokens", new[] { "AppUserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.UserProfiles", new[] { "SignatureId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Folders", new[] { "FacultyId" });
            DropIndex("dbo.Folders", new[] { "DepartmentId" });
            DropIndex("dbo.Folders", new[] { "AccessLevelId" });
            DropIndex("dbo.Folders", new[] { "ParentId" });
            DropIndex("dbo.Files", new[] { "UploadedById" });
            DropIndex("dbo.Files", new[] { "AccessLevelId" });
            DropIndex("dbo.Files", new[] { "FolderId" });
            DropIndex("dbo.Files", new[] { "FileContentId" });
            DropIndex("dbo.Departments", new[] { "FacultyId" });
            DropIndex("dbo.AppUsers", new[] { "Signature_Id" });
            DropIndex("dbo.AppUsers", new[] { "UserProfileId" });
            DropIndex("dbo.AppUsers", new[] { "DepartmentId" });
            DropIndex("dbo.AppUsers", "TagIdIndex");
            DropIndex("dbo.AppUsers", "NameIndex");
            DropIndex("dbo.AppUsers", "EmailIndex");
            DropIndex("dbo.AppUsers", "UserIdIndex");
            DropIndex("dbo.AccessDetails", new[] { "AccessLevelId" });
            DropIndex("dbo.AccessDetails", new[] { "AppUserId" });
            DropTable("dbo.Tokens");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.UserProfiles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Folders");
            DropTable("dbo.FileContents");
            DropTable("dbo.Files");
            DropTable("dbo.Faculties");
            DropTable("dbo.Departments");
            DropTable("dbo.AppUsers");
            DropTable("dbo.AccessLevels");
            DropTable("dbo.AccessDetails");
        }
    }
}
