using System.Data.Entity;
using PGD.Domain.Entities.RH;
using PGD.Domain.Filtros;
using PGD.Domain.Interfaces.Repository;
using PGD.Domain.Paginacao;
using PGD.Infra.Data.Context;
using PGD.Infra.Data.Util;
using System.Linq;
using PGD.Domain.Entities;
using System.Collections.Generic;

namespace PGD.Infra.Data.Repository
{
    public class UnidadeRepository : Repository<Unidade>, IUnidadeRepository
    {
        public UnidadeRepository(PGDDbContext context)
            : base(context)
        {

        }

        public Paginacao<Unidade> Buscar(UnidadeFiltro filtro)
        {
            var retorno = new Paginacao<Unidade>();
            var query = DbSet.AsQueryable();

            if (filtro.IncludeUnidadeSuperior)
                query.Include("UnidadeSuperior");            
            
            if (filtro.IncludeUnidadePerfisUnidades)
                query.Include("UsuariosPerfisUnidades");            
            
            if (filtro.IncludeUnidadesFilhas)
                query.Include("Unidades");            
            
            if (filtro.IncludeUnidadeTiposPactos)
                query.Include("UnidadesTiposPactos");


            if (!string.IsNullOrWhiteSpace(filtro.Sigla))
                query = query.Where(x => x.Sigla.ToLower().Contains(filtro.Sigla.ToLower()));

            if(!string.IsNullOrWhiteSpace(filtro.Nome))
                query = query.Where(x => x.Nome.ToLower().Contains(filtro.Nome.ToLower()));
            
            if(!string.IsNullOrWhiteSpace(filtro.NomeOuSigla))
                query = query.Where(x => x.Nome.ToLower().Contains(filtro.NomeOuSigla.ToLower()) 
                                         || x.Sigla.ToLower().Contains(filtro.NomeOuSigla.ToLower()));

            if (filtro.IdUnidadeSuperior.HasValue)
                query = query.Where(x => x.IdUnidadeSuperior == filtro.IdUnidadeSuperior);

            if (filtro.Id.HasValue)
                query = query.Where(x => x.IdUnidade == filtro.Id);
            
            if(filtro.IdUsuario.HasValue)
                query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => !y.Excluido && y.IdUsuario == filtro.IdUsuario));

            if (filtro.IdTipoPacto.HasValue)
                query = query.Where(x => x.UnidadesTiposPactos.Any(y => y.IdTipoPacto == filtro.IdTipoPacto));

            if (!filtro.BuscarExcluidos)
                query = query.Where(x => !x.Excluido);
               

            if (filtro.IdsPactos != null && filtro.IdsPactos.Any())
            {
                var idsUnidades = Db.Set<Pacto>().Where(y => filtro.IdsPactos.Contains(y.IdPacto))
                    .Select(y => y.UnidadeExercicio).Distinct();

                query = query.Where(x => idsUnidades.Contains(x.IdUnidade));
            }

            retorno.QtRegistros = query.Count();

            if (filtro.Skip.HasValue && filtro.Take.HasValue)
            {
                retorno.Lista = filtro.OrdenarDescendente
                    ? query.OrderByDescending(filtro.OrdenarPor).Skip(filtro.Skip.Value).Take(filtro.Take.Value).ToList()
                    : query.OrderBy(filtro.OrdenarPor).Skip(filtro.Skip.Value).Take(filtro.Take.Value).ToList();
            }
            else
            {
                retorno.Lista = query.ToList();
            }

            return retorno;
        }

        public IEnumerable<Unidade> ObterUnidadesSubordinadas(int idUnidadePai)
        {
            var a = from u in Db.Set<Unidade>()
                    where u.IdUnidadeSuperior == idUnidadePai
                    select u;

            var unidadesSubordinadas = RetornaUnidadesSubordinadas(a);
            return unidadesSubordinadas;

        }

        private IQueryable<Unidade> RetornaUnidadesSubordinadas(IQueryable<Unidade> lista, bool forcarParada = false)
        {
            if (lista.All(x => x.IdUnidadeSuperior == null) || forcarParada)
                return lista;

            var lista2 = lista;

            var lista3 = Db.Set<Unidade>()
                .Where(x => lista2.All(y => y.IdUnidade != x.IdUnidade) && lista2.Any(y => y.IdUnidade == x.IdUnidadeSuperior));

            forcarParada = !lista3.Any();

            lista = lista.Concat(lista3);

            return RetornaUnidadesSubordinadas(lista, forcarParada);
        }
    }
}
