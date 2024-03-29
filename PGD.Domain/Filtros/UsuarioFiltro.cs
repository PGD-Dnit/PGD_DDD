﻿using PGD.Domain.Entities.RH;
using System.Collections.Generic;
using PGD.Domain.Enums;
using PGD.Domain.Filtros.Base;

namespace PGD.Domain.Filtros
{
    public class UsuarioFiltro : BaseFiltro
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Sigla { get; set; }
        public string Matricula { get; set; }
        public int? IdUnidade { get; set; }
        public Perfil? Perfil { get; set; }
        public bool IncludeUnidadesPerfis { get; set; }
        //csa
        public int? Inativo { get; set; }
        //csa
        public int? IdUnidadeSub { get; set; }
        //csa
        public List<Unidade> ListaUnidade { get; set; }
        //csa
        public int? Excluido { get; set; }
    }
}