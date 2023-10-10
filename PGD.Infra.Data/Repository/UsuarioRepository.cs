using PGD.Domain.Entities.Usuario;
using PGD.Domain.Filtros;
using PGD.Domain.Interfaces.Repository;
using PGD.Domain.Paginacao;
using PGD.Infra.Data.Context;
using PGD.Infra.Data.Util;
using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using PGD.Domain.Entities.RH;
using PGD.Domain.Entities;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Remoting.Contexts;

namespace PGD.Infra.Data.Repository
{
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
    Justification = "False positive.")]
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        private readonly IUnidadeRepository _unidadeRepository;
        public UsuarioRepository(PGDDbContext context, UnidadeRepository unidadeRepository)
            : base(context)
        {
            this._unidadeRepository = unidadeRepository;

        }

        public Usuario ObterPorCPF(string cpf)
        {
            cpf = cpf.PadLeft(11, '0');
            var usuario = DbSet.AsNoTracking()
                .Where(a => a.Cpf.Replace("\r", string.Empty).Replace("\n", string.Empty) == cpf)
                .Include("UsuariosPerfisUnidades")
                .Include("UsuariosPerfisUnidades.Perfil")
                .Include("UsuariosPerfisUnidades.Unidade")
                .FirstOrDefault();
            if (usuario == null)
            {
                long novocpf;
                if (long.TryParse(cpf, out novocpf))
                {
                    cpf = novocpf.ToString();
                    usuario = DbSet.AsNoTracking().Where(a => a.Cpf.Replace("\r", string.Empty).Replace("\n", string.Empty) == cpf)
                        .Include("UsuariosPerfisUnidades")
                        .Include("UsuariosPerfisUnidades.Perfil")
                        .Include("UsuariosPerfisUnidades.Unidade")
                        .FirstOrDefault();
                }

            }
            return usuario;
        }

        public Usuario ObterPorEmail(string email)
        {
            return DbSet.AsNoTracking().Where(a => a.Email.Replace("\r", string.Empty).Replace("\n", string.Empty) == email).FirstOrDefault();
        }

        public Usuario ObterPorNome(string nome)
        {

            return DbSet.AsNoTracking().Where(a => a.Nome.Trim().ToLower().Contains(nome.Trim().ToLower())).FirstOrDefault();
        }

        public override Usuario ObterPorId(int id)
        {
            throw new NotImplementedException();
        }

        public Usuario Teste(string email)
        {
            var usuario = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(email))
                usuario = usuario.Where(x => x.Email.ToUpper() == email.ToUpper());

            return new Usuario();
        }
        //csa
        public IEnumerable<Usuario> ObterTodosPorUnidade(int idUnidade, bool incluirSubordinados = false)
        {
            var usuarios = new List<Usuario>();
            IQueryable<Usuario> lista = null;
            var unidades = _unidadeRepository.ObterUnidadesSubordinadas(idUnidade).ToList();

            foreach (var unid in unidades)
            {
                ///Parei aqui
                var listaUsuarios = from u in Db.Set<Usuario>()
                                    where u.IdUsuario == unid.IdUnidade
                                    select u;
                // var a = from u in Db.Set<Usuario>()
                //         where u.Unidade == unid.IdUnidade
                //         select u;
                //usuariosUnidades = Db.Set<Usuario>().Where(x => x.Unidade == unid.IdUnidade);
                ////.Where(x => lista2.All(y => y.IdUnidade != x.IdUnidade) && lista2.Any(y => y.IdUnidade == x.IdUnidadeSuperior));

                // lista = usuariosUnidades;
                lista = lista.Concat(listaUsuarios);
            }
            return lista;

        }
        public Paginacao<Usuario> Buscar(UsuarioFiltro filtro)
        {
            var retorno = new Paginacao<Usuario>();
            var query = DbSet.AsQueryable();    
            
            if (filtro.IncludeUnidadesPerfis)
                query.Include("UsuariosPerfisUnidades")
                    .Include("UsuariosPerfisUnidades.Perfil")
                    .Include("UsuariosPerfisUnidades.Unidade");

            query = !string.IsNullOrWhiteSpace(filtro.Nome) ? query.Where(x => x.Nome.ToLower().Contains(filtro.Nome.ToLower())) : query;
            query = !string.IsNullOrWhiteSpace(filtro.Cpf) ? query.Where(x => x.Cpf == filtro.Cpf) : query;
            query = !string.IsNullOrWhiteSpace(filtro.Sigla) ? query.Where(x => x.Sigla == filtro.Sigla) : query;
            query = !string.IsNullOrWhiteSpace(filtro.Matricula) ? query.Where(x => x.Matricula.ToLower().Contains(filtro.Matricula.ToLower())) : query;
            query = filtro.IdUnidade.HasValue ? query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == filtro.IdUnidade)) : query;            
            query = filtro.Id.HasValue ? query.Where(x => x.IdUsuario == filtro.Id) : query;

            query = filtro.Perfil.HasValue ? query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdPerfil == (int)filtro.Perfil)) : query;
            
            //csa
            if (filtro.Inativo.HasValue)
                query = query.Where(x => x.Inativo == false);
            //csa
            var unidadeSuperior = BuscarUnidadeSuperior(filtro.IdUnidade).FirstOrDefault();
            if (filtro.Excluido.HasValue)
            {
                if (filtro.IncludeUnidadeSuperior)
                {
                    query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == filtro.IdUnidade && y.Excluido == false && y.IdPerfil == (int)filtro.Perfil) ||
                                   x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == unidadeSuperior.IdUnidade && y.Excluido == false && y.IdPerfil == (int)filtro.Perfil))
                            .Select(u => u);
                }else
                {
                    query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == filtro.IdUnidade && y.Excluido == false && y.IdPerfil == (int)filtro.Perfil))
                            .Select(u => u);
                }
            }
            //csa
            /*if (filtro.IncludeUnidadeSuperior)
            {
                
                var unidadeSuperior = BuscarUnidadeSuperior(filtro.IdUnidade).FirstOrDefault();  
                
                query = query.Where(x =>//////////talvesz trocar unidadeSuperior.IdUnidade por filtro.IdUnidade como tinha dado certo acima
                                    x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == unidadeSuperior.IdUnidade && y.Excluido == false && y.IdPerfil == (int)filtro.Perfil) ||
                                    x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == unidadeSuperior.IdUnidade && y.Excluido == false && y.IdPerfil == (int)filtro.Perfil)).Select(u => u);

            }*/
            //
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
        public Paginacao<Usuario> BuscarAdminPessoas(UsuarioFiltro filtro, List<Unidade> unidades)
        {
            var resultado = new Paginacao<Usuario>();                       
            var resultados = new List<Paginacao<Usuario>>();
            
            foreach (var unid in unidades)
            {
                var retorno = new Paginacao<Usuario>();
                var query = DbSet.AsQueryable();
                if (filtro.IncludeUnidadesPerfis)
                    query.Include("UsuariosPerfisUnidades")
                        .Include("UsuariosPerfisUnidades.Perfil")
                        .Include("UsuariosPerfisUnidades.Unidade");
                

                query = !string.IsNullOrWhiteSpace(filtro.Nome) ? query.Where(x => x.Nome.ToLower().Contains(filtro.Nome.ToLower())) : query;
                query = !string.IsNullOrWhiteSpace(filtro.Cpf) ? query.Where(x => x.Cpf == filtro.Cpf) : query;
                query = !string.IsNullOrWhiteSpace(filtro.Sigla) ? query.Where(x => x.Sigla == filtro.Sigla) : query;
                query = !string.IsNullOrWhiteSpace(filtro.Matricula) ? query.Where(x => x.Matricula.ToLower().Contains(filtro.Matricula.ToLower())) : query;
                query = filtro.IdUnidade.HasValue ? query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == filtro.IdUnidade && y.Excluido == false)) : query;
                //csa
                // query = filtro.IdUnidadeSub.HasValue ? query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == filtro.IdUnidadeSub)) : query;
                query = filtro.Id.HasValue ? query.Where(x => x.IdUsuario == filtro.Id) : query;

                query = filtro.Perfil.HasValue ? query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdPerfil == (int)filtro.Perfil)) : query;
                
                //csa
                if (filtro.Inativo.HasValue)
                    query = query.Where(x => x.Inativo == false);
                //csa
                /*if (filtro.Inativo.HasValue)
                    query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.Excluido == false));*/
                //csa
                query = query.Where(x => x.UsuariosPerfisUnidades.Any(y => y.IdUnidade == unid.IdUnidade && y.Excluido == false));

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
        //csa
        public IEnumerable<Usuario> BuscarServidoresAdminPessoas(int idUnidade, UsuarioFiltro filtro)
        {
            var unidades = from u in Db.Set<Usuario>()
                           join upu in Db.Set<UsuarioPerfilUnidade>()
                           on u.IdUsuario equals upu.IdUsuario
                           where u.Nome.ToLower().Contains(filtro.Nome.ToLower()) || u.Matricula.ToLower().Contains(filtro.Matricula.ToLower()) && upu.IdUnidade == filtro.IdUnidade || u.Inativo == false && upu.IdUnidade == idUnidade && upu.Excluido == false
                           select u;
            //u.Nome.ToLower().Contains(filtro.Nome.ToLower())
            return unidades;
        }
        //csa
        public IEnumerable<Usuario> BuscarServidores(int idUnidade, int idPerfil, string CPF)
        {
            var unidades = from u in Db.Set<Usuario>()
                           join upu in Db.Set<UsuarioPerfilUnidade>()
                           on u.IdUsuario equals upu.IdUsuario
                           where u.Cpf != CPF && u.Inativo == false && upu.IdUnidade == idUnidade && upu.IdPerfil == idPerfil && upu.Excluido == false
                           select u;

            return unidades;
        }
        //csa
        public IEnumerable<Unidade> BuscarUnidadeSuperior(int? idUnidade)
        {
            var unidadesSuperiores = from u in Db.Set<Unidade>()
                                     where u.IdUnidade == idUnidade
                                     select u.UnidadeSuperior;

            return unidadesSuperiores.ToList();
        }
    }
}
