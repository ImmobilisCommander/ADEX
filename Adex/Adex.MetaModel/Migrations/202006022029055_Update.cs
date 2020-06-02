namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Metadatas", "Entity_Id", c => c.Int());
            CreateIndex("dbo.Metadatas", "Entity_Id");
            AddForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities");
            DropIndex("dbo.Metadatas", new[] { "Entity_Id" });
            DropColumn("dbo.Metadatas", "Entity_Id");
        }
    }
}
