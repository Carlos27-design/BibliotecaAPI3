using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Utilidades
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginationDTO paginationDTO)
        {
            return queryable.Skip((paginationDTO.Pagina - 1) * paginationDTO.RecordPorPagina)
                .Take(paginationDTO.RecordPorPagina);
        }
    }
}
