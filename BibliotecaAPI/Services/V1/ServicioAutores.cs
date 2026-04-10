using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Services.V1
{
    public class ServicioAutores : IServicioAutores
    {
        private readonly ApplicationDBContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public ServicioAutores(ApplicationDBContext context,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
            )
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AutorDTO>> Get(PaginationDTO paginationDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await httpContextAccessor.HttpContext!.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable
                                .OrderBy(x => x.Nombres)
                                .Paginar(paginationDTO).ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }
    }
}
