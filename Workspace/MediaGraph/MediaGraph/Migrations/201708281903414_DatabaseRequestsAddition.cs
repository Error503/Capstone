namespace MediaGraph.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DatabaseRequestsAddition : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DatabaseRequests",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SubmitterRefId = c.String(maxLength: 128),
                        SubmissionDate = c.DateTime(nullable: false),
                        Reviewed = c.Boolean(nullable: false),
                        ReviewerId = c.String(),
                        Approved = c.Boolean(nullable: false),
                        ApprovalDate = c.DateTime(),
                        Notes = c.String(),
                        NodeData = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.SubmitterRefId)
                .Index(t => t.SubmitterRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DatabaseRequests", "SubmitterRefId", "dbo.AspNetUsers");
            DropIndex("dbo.DatabaseRequests", new[] { "SubmitterRefId" });
            DropTable("dbo.DatabaseRequests");
        }
    }
}
