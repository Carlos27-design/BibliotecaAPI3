using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Enitities;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros")]
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



        [HttpGet]
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

        [HttpGet("{id:int}", Name = "ObtenerLibro")]
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

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreateDTO libroCreateDTO)
        {

            if (libroCreateDTO.AutoresIds is null || libroCreateDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreateDTO.AutoresIds), "No se puede crear un libro sin autores");
                return ValidationProblem();
            }

            var autoresIdsExisten = await context.Autores.Where(x => libroCreateDTO.AutoresIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

            if (autoresIdsExisten.Count != libroCreateDTO.AutoresIds.Count)
            {
                var autoresIdsNoExisten = libroCreateDTO.AutoresIds.Except(autoresIdsExisten);
                var autoresIdsNoExistenString = string.Join(", ", autoresIdsNoExisten);
                var mensajeError = $"Los siguientes autores no existen: {autoresIdsNoExistenString}";
                ModelState.AddModelError(nameof(libroCreateDTO.AutoresIds), mensajeError);
                return ValidationProblem();
            }

            var libro = mapper.Map<Libro>(libroCreateDTO);
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreateDTO libroCreateDTO)
        {

            if (libroCreateDTO.AutoresIds is null || libroCreateDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreateDTO.AutoresIds), "No se puede actualizar libro sin autores");
                return ValidationProblem();
            }

            var autoresIdExisten = await context.Autores.Where(x => libroCreateDTO.AutoresIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

            if (autoresIdExisten.Count != libroCreateDTO.AutoresIds.Count)
            {
                var autoresIdsNoExisten = libroCreateDTO.AutoresIds.Except(autoresIdExisten);
                var autoresIdsNoExistenString = string.Join(", ", autoresIdsNoExisten);
                var mensajeError = $"Los siguientes autores no existen";
                ModelState.AddModelError(nameof(libroCreateDTO.AutoresIds), mensajeError);
                return ValidationProblem();
            }

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

        [HttpDelete("{id:int}")]
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
