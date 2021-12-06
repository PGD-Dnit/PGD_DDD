using PGD.Domain.Entities.RH;
using PGD.Domain.Filtros;
using PGD.Domain.Paginacao;
using System.Collections.Generic;


namespace PGD.Domain.Interfaces.Repository
{
    public interface IUnidadeRepository : IRepository<Unidade>
    {
        Paginacao<Unidade> Buscar(UnidadeFiltro filtro);
        IEnumerable<Unidade> ObterUnidadesSubordinadas(int idUnidadePai);
    }
}
