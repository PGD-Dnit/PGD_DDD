using PGD.Domain.Entities;
using System.Data.Entity.ModelConfiguration;

namespace PGD.Infra.Data.EntityConfig
{
    public class ModalidadeConfig : EntityTypeConfiguration<Modalidade>
    {
        public ModalidadeConfig()
        {
            HasKey(x => x.IdModalidade);

            Property(x => x.NomModalidade).IsRequired();
           

            //Ignore(c => c.ValidationResult);
        }
    }
}
