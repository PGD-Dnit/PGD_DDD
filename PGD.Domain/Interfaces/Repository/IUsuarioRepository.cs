using PGD.Domain.Entities.Usuario;
using PGD.Domain.Filtros;
using PGD.Domain.Paginacao;
using System.Collections.Generic;

namespace PGD.Domain.Interfaces.Repository
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Usuario ObterPorNome(string nome);
        Usuario ObterPorCPF(string cpf);
        Usuario ObterPorEmail(string email);
        Paginacao<Usuario> Buscar(UsuarioFiltro filtro);
        //csa
        IEnumerable<Usuario> ObterTodosPorUnidade(int idUnidade, bool incluirSubordinados = false);
    }
}
