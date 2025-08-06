namespace DevExtremeMvcApp2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCalculateFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShapeInputs", "CalculationResult", c => c.Int(nullable: false));
            AddColumn("dbo.ShapeInputs", "IsCalculated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShapeInputs", "IsCalculated");
            DropColumn("dbo.ShapeInputs", "CalculationResult");
        }
    }
}
