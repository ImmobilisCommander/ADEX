namespace Adex.Data.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinksIndex : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Links", "Kind", c => c.String(maxLength: 1000));
            CreateIndex("dbo.Links", "Kind");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Links", new[] { "Kind" });
            AlterColumn("dbo.Links", "Kind", c => c.String());
        }
    }
}
