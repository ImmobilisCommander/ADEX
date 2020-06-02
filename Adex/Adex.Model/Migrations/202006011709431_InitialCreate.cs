namespace Adex.Model.Migrations
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
                        ExternalId = c.String(maxLength: 200),
                        Designation = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        FromId = c.Int(nullable: false),
                        ToId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Id)
                .ForeignKey("dbo.Entities", t => t.FromId)
                .ForeignKey("dbo.Entities", t => t.ToId)
                .Index(t => t.Id)
                .Index(t => t.FromId)
                .Index(t => t.ToId);
            
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
            DropForeignKey("dbo.Links", "ToId", "dbo.Entities");
            DropForeignKey("dbo.Links", "FromId", "dbo.Entities");
            DropForeignKey("dbo.Links", "Id", "dbo.Entities");
            DropForeignKey("dbo.Companies", "Id", "dbo.Entities");
            DropIndex("dbo.Persons", new[] { "Id" });
            DropIndex("dbo.Links", new[] { "ToId" });
            DropIndex("dbo.Links", new[] { "FromId" });
            DropIndex("dbo.Links", new[] { "Id" });
            DropIndex("dbo.Companies", new[] { "Id" });
            DropTable("dbo.Persons");
            DropTable("dbo.Links");
            DropTable("dbo.Companies");
            DropTable("dbo.Entities");
        }
    }
}
