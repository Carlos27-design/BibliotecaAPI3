using BibliotecaAPI.Enitities;

namespace BibliotecaAPI.Services
{
    public interface IServiciosUsuarios
    {
        Task<Usuario?> ObtenerUsuario();
    }
}