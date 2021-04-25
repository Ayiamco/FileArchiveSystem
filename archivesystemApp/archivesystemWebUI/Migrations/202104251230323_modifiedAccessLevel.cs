namespace archivesystemWebUI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifiedAccessLevel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AccessLevels", "Level", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AccessLevels", "Level", c => c.String());
        }
    }
}
