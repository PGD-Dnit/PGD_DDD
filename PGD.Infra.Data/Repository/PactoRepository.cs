using PGD.Domain.Entities;
using PGD.Domain.Interfaces.Repository;
using PGD.Infra.Data.Context;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;
using CsQuery.ExtensionMethods;
using PGD.Domain.Entities.RH;
using RefactorThis.GraphDiff;
using PGD.Domain.Services;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using PGD.Domain.Enums;
using static PGD.Infra.Data.Repository.PactoRepository;
using System.Runtime.Remoting.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace PGD.Infra.Data.Repository
{
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
    Justification = "False positive.")]
    public class PactoRepository : Repository<Pacto>, IPactoRepository
    {
        UnidadeService _unidadeService;
        IIniciativaPlanoOperacionalRepository _iniciativaPORepository;

        public PactoRepository(PGDDbContext context, UnidadeService unidadeService, IIniciativaPlanoOperacionalRepository iniciativaPORepository)
            : base(context)
        {
            this._unidadeService = unidadeService;
            this._iniciativaPORepository = iniciativaPORepository;
        }

        public Pacto BuscarPorId(int id)
        {
            return   DbSet.AsNoTracking().Include("Produtos")
                .Include("OrdemServico")
                .Include("Historicos")
                .Include("Produtos.GrupoAtividade")
                .Include("Produtos.Atividade")
                .Include("Produtos.TipoAtividade")
                .Include("Produtos.SituacaoProduto")
                .Include("Cronogramas")
                .Where(a => a.IdPacto == id).FirstOrDefault();
        }

        public override IEnumerable<Pacto> Buscar(Expression<Func<Pacto, bool>> predicate)
        {
            return DbSet.AsNoTracking().Where(predicate);
        }

        public override Pacto ObterPorId(int id)
        {
            return DbSet.AsNoTracking().Where(a => a.IdPacto == id).FirstOrDefault();
        }

        public void AtualizaEstadoEntidadesRelacionadas(Pacto pacto)
        {
            Db.UpdateGraph(pacto, m => m.OwnedCollection(p => p.Produtos, with => with.OwnedCollection(p2 => p2.IniciativasPlanoOperacionalProduto).OwnedCollection(p2 => p2.Avaliacoes, with2 => with2.OwnedCollection(p3 => p3.AvaliacoesDetalhadas))).OwnedCollection(p => p.Cronogramas));
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
        
        public IEnumerable<Pacto> ConsultarPactos(Pacto objFiltro, bool incluirUnidadesSubordinadas = false)
        {
            var query = DbSet.AsNoTracking()
                //.Include("Produtos")
                //.Include("OrdemServico")
                //.Include("Historicos")
                //.Include("Produtos.GrupoAtividade")
                //.Include("Produtos.Atividade")
                //.Include("Produtos.TipoAtividade")
                //.Include("Cronogramas")
                .Include("SituacaoPacto")
                .Include("TipoPacto")
                .AsQueryable();

            if (objFiltro.IdPacto > 0)
            {
                query = query.Where(x => x.IdPacto == objFiltro.IdPacto);
            }

            if (!string.IsNullOrEmpty(objFiltro.Nome))
            {
                query = query.Where(x => x.Nome.Contains(objFiltro.Nome));
            }

            if (objFiltro.UnidadeExercicio > 0)
            {
                if (incluirUnidadesSubordinadas)
                {
                    var a = from u in Db.Set<Unidade>()
                        where u.IdUnidadeSuperior == objFiltro.UnidadeExercicio
                        select u;
                    
                    var unidadesSubordinadas = RetornaUnidadesSubordinadas(a).Select(x => x.IdUnidade).Distinct();
                    
                    query = query.Where(x => x.UnidadeExercicio == objFiltro.UnidadeExercicio || unidadesSubordinadas.Contains(x.UnidadeExercicio));
                }
                else
                {
                    query = query.Where(x => x.UnidadeExercicio == objFiltro.UnidadeExercicio);
                }
            }
            //csa add esse filtro p exluir os planos de trabalho da propria chefia consultante
            if (!string.IsNullOrEmpty(objFiltro.CpfUsuario))
            {
                query = query.Where(x => x.CpfUsuario != objFiltro.CpfUsuario);
            }
            if (objFiltro.IdSituacaoPacto > 0)
            {
                query = query.Where(x => x.IdSituacaoPacto == objFiltro.IdSituacaoPacto);
            }

            if (objFiltro.IdTipoPacto > 0)
            {
                query = query.Where(x => x.IdTipoPacto == objFiltro.IdTipoPacto);
            }

            if (objFiltro.DataPrevistaInicio > DateTime.MinValue && objFiltro.DataPrevistaTermino > DateTime.MinValue)
            {
                query = query.Where(x => x.DataPrevistaInicio >= objFiltro.DataPrevistaInicio && x.DataPrevistaTermino <= objFiltro.DataPrevistaTermino);
            }
            else if (objFiltro.DataPrevistaInicio > DateTime.MinValue && objFiltro.DataPrevistaTermino == DateTime.MinValue)
            {
                query = query.Where(x => x.DataPrevistaInicio >= objFiltro.DataPrevistaInicio);
            }
            else if (objFiltro.DataPrevistaInicio == DateTime.MinValue && objFiltro.DataPrevistaTermino > DateTime.MinValue)
            {
                query = query.Where(x => x.DataPrevistaTermino <= objFiltro.DataPrevistaTermino);
            }

            return query.AsEnumerable();

        }
        // public IEnumerable<Pacto> ObterPactosProcedure(Pacto objFiltro, bool incluirUnidadesSubordinadas = false)
        public IEnumerable<Pacto> ObterPactosProcedure(Pacto objFiltro, bool incluirUnidadesSubordinadas = false)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PGDConnectionString"].ConnectionString;

            SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();
            List<Pacto> resultList = new List<Pacto>();
            var procedureName = "GetPactos";

            using (SqlCommand command = new SqlCommand(procedureName, conexao))
            {

                command.CommandType = System.Data.CommandType.StoredProcedure;
                var addParametro = objFiltro.UnidadeExercicio != 0 ? command.Parameters.AddWithValue("@IdUnidadeSuperior", objFiltro.UnidadeExercicio) : command.Parameters.AddWithValue("@IdUnidadeSuperior", null);
                //command.Parameters.AddWithValue("@IdUnidadeSuperior", null);//0
                command.Parameters.AddWithValue("@ObterPactosUnidadesSubordinadas", incluirUnidadesSubordinadas);
                //command.Parameters.AddWithValue("@Cpf", objFiltro.CpfUsuario);
                addParametro = objFiltro.IdPacto > 0 ? command.Parameters.AddWithValue("@IdPacto", objFiltro.IdPacto) : command.Parameters.AddWithValue("@IdPacto", null);
                //command.Parameters.AddWithValue("@IdPacto", null);//0
                addParametro = !string.IsNullOrEmpty(objFiltro.Nome) ? command.Parameters.AddWithValue("@Nome", objFiltro.Nome) : command.Parameters.AddWithValue("@Nome", null);
                //command.Parameters.AddWithValue("@Nome", null);//""                                
                addParametro = objFiltro.IdSituacaoPacto > 0 ? command.Parameters.AddWithValue("@IdSituacaoPacto", objFiltro.IdSituacaoPacto) : command.Parameters.AddWithValue("@IdSituacaoPacto", null);
                //command.Parameters.AddWithValue("@IdSituacaoPacto", null);//0
                DateTime dataBase = new DateTime(1970, 1, 1, 0, 0, 0);
                //if (objFiltro.DataPrevistaInicio < dataBase) objFiltro.DataPrevistaInicio = '';
                addParametro = objFiltro.DataPrevistaInicio >= dataBase ? command.Parameters.AddWithValue("@DataPrevistaInicio", objFiltro.DataPrevistaInicio) : command.Parameters.AddWithValue("@DataPrevistaInicio", null);
                //command.Parameters.AddWithValue("@DataPrevistaInicio", objFiltro.DataPrevistaInicio);
                addParametro = objFiltro.DataPrevistaTermino >= dataBase ? command.Parameters.AddWithValue("@DataPrevistaTermino", objFiltro.DataPrevistaTermino) : command.Parameters.AddWithValue("@DataPrevistaTermino", null);
                //command.Parameters.AddWithValue("@DataPrevistaTermino", objFiltro.DataPrevistaTermino);

                // Executa o comando e obtém o resultado
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //DateTime? DataTerminoReal;
                        //DateTime? DataInterrupcao;
                        //string CpfUsuarioSolicitante;


                        int IdPacto = reader.GetInt32(0);
                        string CpfUsuario = reader.GetString(1);
                        string Nome = reader.GetString(2);
                        int UnidadeExercicio = reader.GetInt32(4);
                        int IdOrdemServico = reader.GetInt32(7);
                        //DateTime DataPrevistaInicio = (DateTime)(reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11));
                        //DateTime DataPrevistaTermino = (DateTime)(reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12));
                        DateTime DataPrevistaInicio =  reader.GetDateTime(11);
                        DateTime DataPrevistaTermino = reader.GetDateTime(12);
                        int IdSituacaoPacto = reader.GetInt32(15);

                        //DateTime? DataTerminoReal = (DateTime)(reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20));
                        DateTime? DataTerminoReal = reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20);

                        DateTime? DataInterrupcao = (reader.IsDBNull(21) ? (DateTime?)null : reader.GetDateTime(21));
                        int IdTipoPacto = reader.GetInt32(22);
                        
                        string CpfUsuarioSolicitante = reader.IsDBNull(24) ? null : reader.GetString(24);
                        //int StatusAprovacaoSolicitante = reader.GetInt32(25);
                        int? StatusAprovacaoSolicitante = reader.IsDBNull(25) ? (int?)null : reader.GetInt32(25);
                        //string CpfUsuarioDirigente = reader.GetString(27);
                        string CpfUsuarioDirigente = reader.IsDBNull(27) ? null : reader.GetString(27);
                        //int StatusAprovacaoDirigente = reader.GetInt32(28);                       
                        int? StatusAprovacaoDirigente = reader.IsDBNull(28) ? (int?)null : reader.GetInt32(28);
                        
                        string CpfUsuarioCriador = reader.GetString(30);
                        bool IndVisualizacaoRestrita = reader.GetBoolean(31);

                        //bool IndVisualizacaoRestrita = Boolean.Parse(reader.GetString(31));
                        string DescSituacaoPacto = reader.GetString(34);                        
                        string DescTipoPacto = reader.GetString(36); 

                      
                        Pacto pacto = new Pacto
                        {
                            SituacaoPacto = new SituacaoPacto
                            {
                                IdSituacaoPacto = IdSituacaoPacto,
                                DescSituacaoPacto = DescSituacaoPacto
                            },
                            TipoPacto = new TipoPacto
                            {
                                IdTipoPacto = IdTipoPacto,
                                DescTipoPacto = DescTipoPacto
                            },
                            IdPacto = IdPacto,
                            IdSituacaoPacto = IdSituacaoPacto,
                            IdTipoPacto = IdTipoPacto,                            
                            IdOrdemServico = IdOrdemServico,
                            DataPrevistaInicio = DataPrevistaInicio,
                            DataPrevistaTermino = DataPrevistaTermino,
                            DataInterrupcao = DataInterrupcao,
                            DataTerminoReal = DataTerminoReal,
                            Nome = Nome,
                            CpfUsuario = CpfUsuario,
                            CpfUsuarioCriador = CpfUsuarioCriador,
                            CpfUsuarioDirigente = CpfUsuarioDirigente,
                            CpfUsuarioSolicitante = CpfUsuarioSolicitante,
                            UnidadeExercicio = UnidadeExercicio,                            
                            StatusAprovacaoDirigente = StatusAprovacaoDirigente,
                            StatusAprovacaoSolicitante = StatusAprovacaoSolicitante,
                            IndVisualizacaoRestrita = IndVisualizacaoRestrita
                        };

                        resultList.Add(pacto);
                    }
                }               

            }

            conexao.Close();
            return resultList;

        }
        
    }

}
