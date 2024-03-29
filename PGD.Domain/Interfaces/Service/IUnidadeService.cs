﻿using PGD.Domain.Entities.RH;
using PGD.Domain.Filtros;
using PGD.Domain.Paginacao;
using System.Collections.Generic;

namespace PGD.Domain.Interfaces.Service
{
    public interface IUnidadeService : IService<Unidade>
    {
        IEnumerable<Unidade> ObterUnidades(int idTipoPacto = 0);
        IEnumerable<Unidade> ObterUnidadesSubordinadas(int idUnidadePai);
        Unidade ObterUnidade(int idUnidade);
        Paginacao<Unidade> Buscar(UnidadeFiltro filtro);
        //csa
        Paginacao<Unidade> BuscarUnidadesAdminPessoas(UnidadeFiltro filtro, List<Unidade> unidades);        
        //csa
        IEnumerable<Unidade> ObterUnidadesParticipante(int idUsuario, int idPerfil);
        //csa
        IEnumerable<Unidade> ObterUnidadesSubordinadasProcedure(int idUnidadePai);
    }
}
