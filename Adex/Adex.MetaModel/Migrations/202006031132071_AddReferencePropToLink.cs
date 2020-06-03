namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReferencePropToLink : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Links", "Reference", c => c.String(maxLength: 200));
            CreateIndex("dbo.Links", "Reference", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Links", new[] { "Reference" });
            DropColumn("dbo.Links", "Reference");
        }
    }
}
