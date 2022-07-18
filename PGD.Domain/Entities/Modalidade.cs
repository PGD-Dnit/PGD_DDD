using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGD.Domain.Entities
{
    public class Modalidade
    {
        public Modalidade()
        {

        }
        public int IdModalidade { get; set; }
        public string NomModalidade { get; set; }
        public bool Inativo { get; set; }
    }
}
