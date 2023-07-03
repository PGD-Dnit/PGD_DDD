using PGD.Domain.Entities.Usuario;
using System.Collections.Generic;
using PGD.Domain.Filtros;
using PGD.Domain.Paginacao;
using PGD.Domain.Entities.RH;

namespace PGD.Domain.Interfaces.Service
{
    public interface IUsuarioService 
    {
        IEnumerable<Usuario> ObterTodosAdministradores();
        Usuario ObterPorNome(string nome);
        Usuario ObterPorCPF(string cpf);
        Usuario ObterPorEmail(string email);
        IEnumerable<Usuario> ObterTodosPorUnidade(int idUnidade, bool incluirSubordinados = false);
        IEnumerable<Usuario> ObterTodosDaBase();
        IEnumerable<Usuario> ObterDirigentesUnidade(int idUnidade);
        Paginacao<Usuario> Buscar(UsuarioFiltro filtro);
        //csa
        Paginacao<Usuario> BuscarAdminPessoas(UsuarioFiltro filtro, List<Unidade> unidade);
        //csa
        IEnumerable<Usuario> BuscarServidores(int idUnidade, int idPerfil, string CPF);
        
    }
}
