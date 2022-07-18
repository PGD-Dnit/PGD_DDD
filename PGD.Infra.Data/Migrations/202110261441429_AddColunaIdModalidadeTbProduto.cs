namespace PGD.Infra.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColunaIdModalidadeTbProduto : DbMigration
    {
        public override void Up()
        {
            
            CreateTable(
                "dbo.Modalidade",
                c => new
                    {
                        IdModalidade = c.Int(nullable: false, identity: true),
                        NomModalidade = c.String(nullable: false, maxLength: 100, unicode: false),
                        Inativo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdModalidade);
            
            
                AddColumn("dbo.Produto", "IdModalidade", c => c.Int(nullable: false));




        }
        
        public override void Down()
        {
            
            DropTable("dbo.Modalidade");

            DropColumn("dbo.Produto", "IdModalidade");

        }
    }
}
