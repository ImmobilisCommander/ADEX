namespace Adex.Data.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MetadataIndex : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Metadatas", "Value", c => c.String(maxLength: 1000));
            CreateIndex("dbo.Metadatas", "Value");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Metadatas", new[] { "Value" });
            AlterColumn("dbo.Metadatas", "Value", c => c.String());
        }
    }
}
