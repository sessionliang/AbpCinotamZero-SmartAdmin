namespace Cinotam.AbpModuleZero.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileNameToAttachment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attachments", "FileName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Attachments", "FileName");
        }
    }
}
