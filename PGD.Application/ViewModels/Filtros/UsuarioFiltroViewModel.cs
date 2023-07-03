using System.Collections.Generic;
using PGD.Application.ViewModels.Filtros.Base;
using PGD.Domain.Entities.RH;
using PGD.Domain.Enums;

namespace PGD.Application.ViewModels.Filtros
{
    public class UsuarioFiltroViewModel : BaseFiltroViewModel
    {
        public UsuarioFiltroViewModel()
        {
            OrdenarPor = "Nome";
        }

        public string Nome { get; set; }
        public string Matricula { get; set; }
        public string Cpf { get; set; }

        public string Sigla { get; set; }
        public int? IdUnidade { get; set; }
        public Perfil? Perfil { get; set; }
        public bool IncludeUnidadesPerfis { get; set; }
        //csa
        public int? Inativo { get; set; }
        //csa
        public int? UserIdUnidade { get; set; }
        //csa
        public int? IdUnidadeSub { get; set; }
        //csa
        public List<Unidade> ListaUnidade { get; set; }

    }
}