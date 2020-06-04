namespace Adex.Data.MetaModel.Migrations
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
                        Reference = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Reference, unique: true);
            
            CreateTable(
                "dbo.Members",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Alias = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.Metadatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        Entity_Id = c.Int(nullable: false),
                        Member_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Entity_Id, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.Member_Id, cascadeDelete: true)
                .Index(t => t.Entity_Id)
                .Index(t => t.Member_Id);
            
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        From_Id = c.Int(),
                        To_Id = c.Int(),
                        Kind = c.String(),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entities", t => t.Id)
                .ForeignKey("dbo.Entities", t => t.From_Id)
                .ForeignKey("dbo.Entities", t => t.To_Id)
                .Index(t => t.Id)
                .Index(t => t.From_Id)
                .Index(t => t.To_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Links", "To_Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "From_Id", "dbo.Entities");
            DropForeignKey("dbo.Links", "Id", "dbo.Entities");
            DropForeignKey("dbo.Metadatas", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities");
            DropIndex("dbo.Links", new[] { "To_Id" });
            DropIndex("dbo.Links", new[] { "From_Id" });
            DropIndex("dbo.Links", new[] { "Id" });
            DropIndex("dbo.Metadatas", new[] { "Member_Id" });
            DropIndex("dbo.Metadatas", new[] { "Entity_Id" });
            DropIndex("dbo.Members", new[] { "Name" });
            DropIndex("dbo.Entities", new[] { "Reference" });
            DropTable("dbo.Links");
            DropTable("dbo.Metadatas");
            DropTable("dbo.Members");
            DropTable("dbo.Entities");
        }
    }
}
