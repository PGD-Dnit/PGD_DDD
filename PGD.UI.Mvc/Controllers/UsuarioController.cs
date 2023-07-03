using CsQuery.ExtensionMethods;
using PGD.Application.Interfaces;
using PGD.Application.Util;
using PGD.Application.ViewModels;
using PGD.Application.ViewModels.Filtros;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PGD.Domain.Enums;
using PGD.Domain.Entities;
using PGD.Domain.Entities.Usuario;
using PGD.Domain.Paginacao;
using CGU.Util;
using PGD.Application.ViewModels.Paginacao;
using CGU.Util.ServicoCorreios;
using PGD.Domain.Entities.RH;
using PGD.Domain.Interfaces.Service;
using CsQuery.ExtensionMethods.Internal;
using iTextSharp.text;

namespace PGD.UI.Mvc.Controllers
{
    public class UsuarioController : BaseController
    {
        public UsuarioController(IUsuarioAppService usuarioAppService, IUnidadeService unidadeService) : base(usuarioAppService)
        {
            _unidadeService = unidadeService;
        }

        public ActionResult Index()
        {
            return View(new SearchUsuarioViewModel());
        }

        [HttpPost]
        public ActionResult Index(SearchUsuarioViewModel model)
        {
            if (!ModelState.IsValid) return Json(new { camposNaoPreenchidos = RetornaErrosModelState() });
            
            var userLogado = getUserLogado();
            var  unidades = new List<Domain.Entities.RH.Unidade>();
            if (userLogado.IsAdminPessoas)
            {
                //ver erro aqui Exceção gerada: 'System.NullReferenceException' em PGD.UI.Mvc.dll
                //Referência de objeto não definida para uma instância de um objeto.
                 unidades = _unidadeService.ObterUnidadesSubordinadas((int)userLogado.IdUnidadeSelecionada).ToList();
                var usuarios = new PaginacaoViewModel<UsuarioViewModel>();
                //var usuariosLista = new List<UsuarioViewModel>();

                var listaDeUsuarios = new PaginacaoViewModel<UsuarioViewModel>();
                //List<UsuarioViewModel> listaDeUsuarios = new List<UsuarioViewModel>();
              
                    var filtro = new UsuarioFiltroViewModel
                    {
                        Nome = model.NomeUsuario,
                        Matricula = model.MatriculaUsuario,
                        IdUnidade = model.IdUnidade,                       
                        IncludeUnidadesPerfis = true,
                        Inativo = 0,
                        Take = model.Take,
                        Skip = model.Skip,
                        //csa
                        //UserIdUnidade = user.IdUnidadeSelecionada                       
                    };
                                    
                usuarios = _usuarioAppService.BuscarAdminPessoas(filtro, unidades);
                usuarios.Lista.ForEach(x => x.CPF = x.CPF.MaskCpfCpnj());
                return Json(usuarios);


            }
            else 
            {
                var filtro = new UsuarioFiltroViewModel
                {
                    Nome = model.NomeUsuario,
                    Matricula = model.MatriculaUsuario,
                    IdUnidade = model.IdUnidade,
                    IncludeUnidadesPerfis = true,
                    Inativo = 0,
                    Take = model.Take,
                    Skip = model.Skip,
                    //csa
                    //UserIdUnidade = user.IdUnidadeSelecionada
                };
                var usuarios = _usuarioAppService.Buscar(filtro);
                usuarios.Lista.ForEach(x => x.CPF = x.CPF.MaskCpfCpnj());
                return Json(usuarios);

            }
            
            
            
        }

        [HttpGet]
        public ActionResult VincularPerfil(int id)
        {
            var usuario = _usuarioAppService.Buscar(new UsuarioFiltroViewModel
            {
                Id = id
            }).Lista.FirstOrDefault();

            PrepararTempDataPerfis();

            return View(new VincularPerfilUsuarioViewModel { 
                Cpf = usuario.CPF.MaskCpfCpnj(),
                Matricula = usuario.Matricula,
                Nome = usuario.Nome,
                IdUsuario = usuario.IdUsuario
            });
        }

        [HttpPost]
        public ActionResult VincularPerfil(VincularPerfilUsuarioViewModel model)
        {
            if (!ModelState.IsValid) return Json(new { camposNaoPreenchidos = RetornaErrosModelState() });

            var usuarioLogado = getUserLogado();
            var retorno = _usuarioAppService.VincularUnidadePerfil(model, usuarioLogado.CPF);

            return Json(retorno);
        }

        [HttpPost]
        public ActionResult ExcluirVinculo(long idUsuarioPerfilUnidade)
        {
            var usuarioLogado = getUserLogado();
            var retorno = _usuarioAppService.RemoverVinculoUnidadePerfil(idUsuarioPerfilUnidade, usuarioLogado.CPF);
            return Json(retorno, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult BuscarUnidadesPerfisUsuario(int idUsuario, int take, int skip)
        {
            var retorno = _usuarioAppService.BuscarPerfilUnidade(new UsuarioPerfilUnidadeFiltroViewModel
            {
                IdUsuario = idUsuario,
                //Skip = skip,
                //Take = take,
                OrdenarDescendente = true
            });
            return Json(retorno, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuscarUnidadesPorNomeOuSigla(string query)
        {
            //csa
            var userLogado = getUserLogado();
            var unidades = new List<Domain.Entities.RH.Unidade>();
            if (userLogado.IsAdminPessoas)
            {
                
                unidades = _unidadeService.ObterUnidadesSubordinadas((int)userLogado.IdUnidadeSelecionada).ToList();
                

                var retorno = _usuarioAppService.BuscarUnidadesAdimPessoas(new UnidadeFiltroViewModel { NomeOuSigla = query }, unidades);
                return Json(retorno, JsonRequestBehavior.AllowGet);



            }
            else
            {
                var retorno = _usuarioAppService.BuscarUnidades(new UnidadeFiltroViewModel { NomeOuSigla = query });
                return Json(retorno, JsonRequestBehavior.AllowGet);
            }
        }

        private void PrepararTempDataPerfis()
        {
            var userLogado = getUserLogado();
            var lista = new List<PerfilViewModel>();
            if (userLogado.IsAdminPessoas)
            {
                 lista = new List<PerfilViewModel>
            {                
                new PerfilViewModel { Nome = Domain.Enums.Perfil.Dirigente.ToString(), IdPerfil = Domain.Enums.Perfil.Dirigente.GetHashCode() },
                new PerfilViewModel { Nome = Domain.Enums.Perfil.Solicitante.ToString(), IdPerfil = Domain.Enums.Perfil.Solicitante.GetHashCode() }

            };

            }
            else {

                 lista = new List<PerfilViewModel>
                {

                    new PerfilViewModel { Nome = Domain.Enums.Perfil.Administrador.ToString(), IdPerfil = Domain.Enums.Perfil.Administrador.GetHashCode() },
                    new PerfilViewModel { Nome = Domain.Enums.Perfil.AdminPessoas.ToString(), IdPerfil = Domain.Enums.Perfil.AdminPessoas.GetHashCode() },
                    new PerfilViewModel { Nome = Domain.Enums.Perfil.Dirigente.ToString(), IdPerfil = Domain.Enums.Perfil.Dirigente.GetHashCode() },
                    new PerfilViewModel { Nome = Domain.Enums.Perfil.Solicitante.ToString(), IdPerfil = Domain.Enums.Perfil.Solicitante.GetHashCode() }

                };

            }
             
            TempData["lstPerfil"] = lista;
        }
    }
}