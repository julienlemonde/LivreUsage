namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Courriel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Courriel", c => c.String());
            DropColumn("dbo.AspNetUsers", "NomCoop");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "NomCoop", c => c.String());
            DropColumn("dbo.AspNetUsers", "Courriel");
        }
    }
}
