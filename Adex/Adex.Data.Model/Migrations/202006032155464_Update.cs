namespace Adex.Data.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialLinks",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Links", t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.Links", "Kind", c => c.String());
            AddColumn("dbo.Links", "Date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FinancialLinks", "Id", "dbo.Links");
            DropIndex("dbo.FinancialLinks", new[] { "Id" });
            DropColumn("dbo.Links", "Date");
            DropColumn("dbo.Links", "Kind");
            DropTable("dbo.FinancialLinks");
        }
    }
}
