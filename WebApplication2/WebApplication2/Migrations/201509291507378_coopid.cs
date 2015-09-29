namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class coopid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "coopid", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "coopid");
        }
    }
}
