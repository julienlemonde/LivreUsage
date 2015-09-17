namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class numtel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Telephone", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Telephone");
        }
    }
}
