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
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System;
using System.Configuration;


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
            {
                if (filtro.IncludePerfil.HasValue)
                {
                    /*query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => !y.Excluido && y.IdUsuario == filtro.IdUsuario) ||
                                   x.UsuariosPerfisUnidades.Any(y => y.Excluido && y.IdUsuario == filtro.IdUsuario && y.IdPerfil == (int)Domain.Enums.Perfil.Solicitante))
                                   .Select(u => u);*/
                    query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUsuario == filtro.IdUsuario && y.IdPerfil == filtro.IncludePerfil));

                }
                else 
                {
                    query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => !y.Excluido && y.IdUsuario == filtro.IdUsuario));
                }
                    

                
            }
                
            //csa add um filtro p perfil
            //if (filtro.IdPerfilSelecionado.HasValue)
                //query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdPerfil == filtro.IdPerfilSelecionado));
               // query = query.Where(x => x.UsuariosPerfisUnidades.Join(y => y.IdPerfil == filtro.IdPerfilSelecionado));
                //query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdPerfil == filtro.IdPerfilSelecionado));

            if (filtro.IdTipoPacto.HasValue)
                query = query.Where(x => x.UnidadesTiposPactos.Any(y => y.IdTipoPacto == filtro.IdTipoPacto));

            if (!filtro.BuscarExcluidos)
                query = query.Where(x => !x.Excluido);

            /*if (filtro.UsuarioPerfilUnidadeExcluidos)
                query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.Excluido && y.IdUsuario == filtro.IdUsuario && y.IdPerfil == (int)Domain.Enums.Perfil.Solicitante));
            */

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
        //csa
        public Paginacao<Unidade> BuscarUnidadesAdminPessoas(UnidadeFiltro filtro, List<Unidade> unidades)
        {

            var resultado = new Paginacao<Unidade>();
            var resultados = new List<Paginacao<Unidade>>();

            foreach (var unid in unidades)
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

                if (!string.IsNullOrWhiteSpace(filtro.Nome))
                    query = query.Where(x => x.Nome.ToLower().Contains(filtro.Nome.ToLower()));

                if (!string.IsNullOrWhiteSpace(filtro.NomeOuSigla))
                    query = query.Where(x => x.Nome.ToLower().Contains(filtro.NomeOuSigla.ToLower())
                                             || x.Sigla.ToLower().Contains(filtro.NomeOuSigla.ToLower()));

                if (filtro.IdUnidadeSuperior.HasValue)
                    query = query.Where(x => x.IdUnidadeSuperior == filtro.IdUnidadeSuperior);

                if (filtro.Id.HasValue)
                    query = query.Where(x => x.IdUnidade == filtro.Id);

                if (filtro.IdUsuario.HasValue)
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
                query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == unid.IdUnidade && y.Excluido == false));
                retorno.QtRegistros = query.Count();
                if (query.Count() != 0)
                {
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
                    resultados.Add(retorno);
                }
               
            }

            resultado.Lista = resultados.SelectMany(r => r.Lista).Distinct().OrderBy(z => z.Nome).ToList();
            resultado.QtRegistros = resultado.Lista.Count();
            return resultado;

        }
        public IEnumerable<Unidade> ObterUnidadesSubordinadas(int idUnidadePai)
        {
            var a = from u in Db.Set<Unidade>()
                    where u.IdUnidadeSuperior == idUnidadePai                    
                    //csa acrescentei a propria unidade
                    || u.IdUnidade == idUnidadePai                                       
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
        //csa
        public IEnumerable<Unidade> ObterUnidadesParticipante(int idUsuario, int idPerfil)
        {
            var unidades = from u in Db.Set<Unidade>()
                           join upu in Db.Set<UsuarioPerfilUnidade>()
                           on u.IdUnidade equals upu.IdUnidade
                           where upu.IdUsuario == idUsuario && upu.IdPerfil == idPerfil && upu.Excluido == false
                           //ver onde e usada p analisar se e necessaria a alteração abaixo
                           //where u.Excluido == false && upu.IdUsuario == idUsuario && upu.IdPerfil == idPerfil && upu.Excluido == false
                           select u;

            return unidades;

        }

        //csa função criada p selecionar as unidades subordinadas Inativo = 0 
        public IEnumerable<Unidade> ObterUnidadesSubordinadasProcedure(int idUnidadePai)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PGDConnectionString"].ConnectionString;           
                      
            SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();
            List<Unidade> resultList = new List<Unidade>();
            var procedureName = "GetUnidSubordinadas";


            using (SqlCommand command = new SqlCommand(procedureName, conexao))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@IdUnidadeSuperior", idUnidadePai);                

                // Executa o comando e obtém o resultado
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int idUnidade = reader.GetInt32(0);
                        string nomeUnidade = reader.GetString(1);
                        //string siglaUnidade = reader.GetString(2);
                        
                        Unidade unidade = new Unidade
                        {
                            IdUnidade = idUnidade,
                            Nome = nomeUnidade,
                            //Sigla = siglaUnidade
                        };

                        resultList.Add(unidade);
                    }
                }
            }
            conexao.Close();
            return resultList;
            
        }
    }
}
