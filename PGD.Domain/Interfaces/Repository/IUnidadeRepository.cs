﻿using PGD.Domain.Entities.RH;
using PGD.Domain.Filtros;
using PGD.Domain.Paginacao;
using System.Collections.Generic;


namespace PGD.Domain.Interfaces.Repository
{
    public interface IUnidadeRepository : IRepository<Unidade>
    {
        Paginacao<Unidade> Buscar(UnidadeFiltro filtro);
        //csa
        Paginacao<Unidade> BuscarUnidadesAdminPessoas(UnidadeFiltro filtro, List<Unidade> unidades);
        IEnumerable<Unidade> ObterUnidadesSubordinadas(int idUnidadePai);
        //csa
        IEnumerable<Unidade> ObterUnidadesParticipante(int idUsuario, int idPerfil);
        //csa
        IEnumerable<Unidade> ObterUnidadesSubordinadasProcedure(int idUnidadePai);
    }
}
