namespace Reptile.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class 新增資料表 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GovProcurements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.Int(nullable: false),
                        AgencNname = c.String(),
                        CaseName = c.String(),
                        Number = c.Int(nullable: false),
                        TenderMethod = c.String(),
                        purchasingProperty = c.String(),
                        AnnouncementDate = c.DateTime(nullable: false),
                        Deadine = c.DateTime(nullable: false),
                        Budget = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GovProcurements");
        }
    }
}
