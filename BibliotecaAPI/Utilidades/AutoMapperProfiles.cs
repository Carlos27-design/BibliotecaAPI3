using AutoMapper;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Enitities;

namespace BibliotecaAPI.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Autor, AutorDTO>()
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(autor => MapearNombreYApellidosAutor(autor)));
            CreateMap<Autor, AutorConLibrosDTO>()
               .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(autor => MapearNombreYApellidosAutor(autor)));
            CreateMap<AutorCreateDTO, Autor>();
            CreateMap<Autor, AutorPatchDTO>().ReverseMap();
            CreateMap<AutorCreacionDTOConFoto, Autor>().ForMember(ent => ent.Foto, config => config.Ignore());

            CreateMap<Libro, LibroDTO>();
            CreateMap<LibroCreateDTO, Libro>().ForMember(ent => ent.Autores, config => config.MapFrom(dto => dto.AutoresIds.Select(id => new AutorLibro { AutorId = id })));
            CreateMap<Libro, LibrosConAutoresDTO>();
            CreateMap<AutorLibro, AutorDTO>().ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AutorId))
                 .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(ent => MapearNombreYApellidosAutor(ent.Autor!)));

            CreateMap<ComentarioCreateDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail, config => config
                .MapFrom(ent => ent.Usuario!.Email));
            CreateMap<ComentarioPatchDTO, Comentario>().ReverseMap();

            CreateMap<AutorLibro, LibroDTO>().ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.LibroId))
                .ForMember(dto => dto.Titulo, config => config.MapFrom(ent => ent.Libro!.Titulo));
            CreateMap<LibroCreateDTO, AutorLibro>()
                .ForMember(ent => ent.Libro, config => config
                .MapFrom(dto => new Libro { Titulo = dto.Titulo }));
            CreateMap<Usuario, UsuarioDTO>();


        }

        private string MapearNombreYApellidosAutor(Autor autor) => $"{autor.Nombres} {autor.Apellidos}";
    }
}
