namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Links : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        From_Id = c.Int(),
                        To_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.From_Id)
                .ForeignKey("dbo.Entities", t => t.To_Id)
                .Index(t => t.From_Id)
                .Index(t => t.To_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Links", "To_Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "From_Id", "dbo.Entities");
            DropIndex("dbo.Links", new[] { "To_Id" });
            DropIndex("dbo.Links", new[] { "From_Id" });
            DropTable("dbo.Links");
        }
    }
}
