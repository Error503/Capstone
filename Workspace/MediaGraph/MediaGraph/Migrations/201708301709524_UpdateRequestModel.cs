namespace MediaGraph.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateRequestModel : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.DatabaseRequests");
            AddColumn("dbo.DatabaseRequests", "ReviewerRefId", c => c.String(maxLength: 128));
            AddColumn("dbo.DatabaseRequests", "ReviewedDate", c => c.DateTime());
            AlterColumn("dbo.DatabaseRequests", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.DatabaseRequests", "Id");
            CreateIndex("dbo.DatabaseRequests", "ReviewerRefId");
            AddForeignKey("dbo.DatabaseRequests", "ReviewerRefId", "dbo.AspNetUsers", "Id");
            DropColumn("dbo.DatabaseRequests", "ReviewerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DatabaseRequests", "ReviewerId", c => c.String());
            DropForeignKey("dbo.DatabaseRequests", "ReviewerRefId", "dbo.AspNetUsers");
            DropIndex("dbo.DatabaseRequests", new[] { "ReviewerRefId" });
            DropPrimaryKey("dbo.DatabaseRequests");
            AlterColumn("dbo.DatabaseRequests", "Id", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.DatabaseRequests", "ReviewedDate");
            DropColumn("dbo.DatabaseRequests", "ReviewerRefId");
            AddPrimaryKey("dbo.DatabaseRequests", "Id");
        }
    }
}
