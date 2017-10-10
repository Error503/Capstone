namespace MediaGraph.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequestAndNodeType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DatabaseRequests", "RequestType", c => c.Byte(nullable: false));
            AddColumn("dbo.DatabaseRequests", "NodeDataType", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DatabaseRequests", "NodeDataType");
            DropColumn("dbo.DatabaseRequests", "RequestType");
        }
    }
}
