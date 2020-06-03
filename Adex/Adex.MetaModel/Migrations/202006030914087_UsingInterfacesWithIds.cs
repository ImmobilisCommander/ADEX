namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UsingInterfacesWithIds : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Links", "From_Id", c => c.Int());
            AddColumn("dbo.Links", "To_Id", c => c.Int());
            CreateIndex("dbo.Links", "From_Id");
            CreateIndex("dbo.Links", "To_Id");
            AddForeignKey("dbo.Links", "From_Id", "dbo.Entities", "Id");
            AddForeignKey("dbo.Links", "To_Id", "dbo.Entities", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Links", "To_Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "From_Id", "dbo.Entities");
            DropIndex("dbo.Links", new[] { "To_Id" });
            DropIndex("dbo.Links", new[] { "From_Id" });
            DropColumn("dbo.Links", "To_Id");
            DropColumn("dbo.Links", "From_Id");
        }
    }
}
