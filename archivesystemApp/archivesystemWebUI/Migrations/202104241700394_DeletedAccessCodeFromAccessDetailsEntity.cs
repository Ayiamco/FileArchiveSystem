namespace archivesystemWebUI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedAccessCodeFromAccessDetailsEntity : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AccessDetails", "AccessCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AccessDetails", "AccessCode", c => c.String());
        }
    }
}
