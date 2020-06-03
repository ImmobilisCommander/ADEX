namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinkImplementsEntity : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Links", new[] { "Reference" });
            DropPrimaryKey("dbo.Links");
            AlterColumn("dbo.Links", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Links", "Id");
            CreateIndex("dbo.Links", "Id");
            AddForeignKey("dbo.Links", "Id", "dbo.Entities", "Id");
            DropColumn("dbo.Links", "Reference");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Links", "Reference", c => c.String(nullable: false, maxLength: 200));
            DropForeignKey("dbo.Links", "Id", "dbo.Entities");
            DropIndex("dbo.Links", new[] { "Id" });
            DropPrimaryKey("dbo.Links");
            AlterColumn("dbo.Links", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Links", "Id");
            CreateIndex("dbo.Links", "Reference", unique: true);
        }
    }
}
