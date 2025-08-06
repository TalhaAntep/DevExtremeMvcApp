namespace DevExtremeMvcApp2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserIdToShapeInputs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShapeInputs", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShapeInputs", "UserId");
        }
    }
}
