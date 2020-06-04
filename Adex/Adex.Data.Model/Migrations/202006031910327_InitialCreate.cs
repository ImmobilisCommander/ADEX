namespace Adex.Data.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Entities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Reference = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Designation = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        From_Id = c.Int(),
                        To_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Id)
                .ForeignKey("dbo.Entities", t => t.From_Id)
                .ForeignKey("dbo.Entities", t => t.To_Id)
                .Index(t => t.Id)
                .Index(t => t.From_Id)
                .Index(t => t.To_Id);
            
            CreateTable(
                "dbo.Persons",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        LastName = c.String(maxLength: 200),
                        FirstName = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Persons", "Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "To_Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "From_Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "Id", "dbo.Entities");
            DropForeignKey("dbo.Companies", "Id", "dbo.Entities");
            DropIndex("dbo.Persons", new[] { "Id" });
            DropIndex("dbo.Links", new[] { "To_Id" });
            DropIndex("dbo.Links", new[] { "From_Id" });
            DropIndex("dbo.Links", new[] { "Id" });
            DropIndex("dbo.Companies", new[] { "Id" });
            DropTable("dbo.Persons");
            DropTable("dbo.Links");
            DropTable("dbo.Companies");
            DropTable("dbo.Entities");
        }
    }
}
