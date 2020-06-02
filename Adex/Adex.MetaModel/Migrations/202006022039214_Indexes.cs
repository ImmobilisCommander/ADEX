namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Entities", "Reference", unique: true);
            CreateIndex("dbo.Members", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Members", new[] { "Name" });
            DropIndex("dbo.Entities", new[] { "Reference" });
        }
    }
}
