namespace Adex.MetaModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequiredAttribute : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities");
            DropForeignKey("dbo.Metadatas", "Member_Id", "dbo.Members");
            DropIndex("dbo.Entities", new[] { "Reference" });
            DropIndex("dbo.Links", new[] { "Reference" });
            DropIndex("dbo.Members", new[] { "Name" });
            DropIndex("dbo.Metadatas", new[] { "Entity_Id" });
            DropIndex("dbo.Metadatas", new[] { "Member_Id" });
            AlterColumn("dbo.Entities", "Reference", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Links", "Reference", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Members", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Metadatas", "Entity_Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Metadatas", "Member_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Entities", "Reference", unique: true);
            CreateIndex("dbo.Links", "Reference", unique: true);
            CreateIndex("dbo.Members", "Name", unique: true);
            CreateIndex("dbo.Metadatas", "Entity_Id");
            CreateIndex("dbo.Metadatas", "Member_Id");
            AddForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Metadatas", "Member_Id", "dbo.Members", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Metadatas", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities");
            DropIndex("dbo.Metadatas", new[] { "Member_Id" });
            DropIndex("dbo.Metadatas", new[] { "Entity_Id" });
            DropIndex("dbo.Members", new[] { "Name" });
            DropIndex("dbo.Links", new[] { "Reference" });
            DropIndex("dbo.Entities", new[] { "Reference" });
            AlterColumn("dbo.Metadatas", "Member_Id", c => c.Int());
            AlterColumn("dbo.Metadatas", "Entity_Id", c => c.Int());
            AlterColumn("dbo.Members", "Name", c => c.String(maxLength: 200));
            AlterColumn("dbo.Links", "Reference", c => c.String(maxLength: 200));
            AlterColumn("dbo.Entities", "Reference", c => c.String(maxLength: 200));
            CreateIndex("dbo.Metadatas", "Member_Id");
            CreateIndex("dbo.Metadatas", "Entity_Id");
            CreateIndex("dbo.Members", "Name", unique: true);
            CreateIndex("dbo.Links", "Reference", unique: true);
            CreateIndex("dbo.Entities", "Reference", unique: true);
            AddForeignKey("dbo.Metadatas", "Member_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.Metadatas", "Entity_Id", "dbo.Entities", "Id");
        }
    }
}
