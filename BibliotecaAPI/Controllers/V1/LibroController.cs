using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Enitities;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros")]
    [Authorize(Policy = "esadmin")]
    public class LibroController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "libro-obtener";

        public LibroController(ApplicationDBContext context, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
        }



        [HttpGet(Name = "ObtenerLibrosV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<LibroDTO>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = context.Libros.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var libros = await queryable.OrderBy(x => x.Titulo).Paginar(paginationDTO).ToListAsync();
            var librosDTO = mapper.Map<IEnumerable<LibroDTO>>(libros);

            return librosDTO;
        }

        [HttpGet("{id:int}", Name = "ObtenerLibroV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<LibrosConAutoresDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(x => x.Autores)
                .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (libro is null)
            {
                return NotFound();
            }

            var libroDTO = mapper.Map<LibrosConAutoresDTO>(libro);


            return libroDTO;
        }

        [HttpPost(Name = "CrearLibroV1")]
        [ServiceFilter<FiltroValidacionLibro>()]
        public async Task<ActionResult> Post(LibroCreateDTO libroCreateDTO)
        {
            var libro = mapper.Map<Libro>(libroCreateDTO);
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("ObtenerLibroV1", new { id = libro.Id }, libroDTO);
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.Autores is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }
        }

        [HttpPut("{id:int}", Name = "ActualizarLibroV1")]
        [ServiceFilter<FiltroValidacionLibro>()]
        public async Task<ActionResult> Put(int id, LibroCreateDTO libroCreateDTO)
        {
            var libroDb = await context.Libros.Include(x => x.Autores).FirstOrDefaultAsync(x => x.Id == id);

            if (libroDb is null)
            {
                return NotFound();
            }

            libroDb = mapper.Map(libroCreateDTO, libroDb);

            AsignarOrdenAutores(libroDb);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "BorrarLibroV1")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (registrosBorrados == 0)
            {
                return NotFound();
            }

            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }
    }
}
