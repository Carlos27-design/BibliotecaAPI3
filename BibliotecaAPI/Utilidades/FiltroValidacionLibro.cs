using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroValidacionLibro : IAsyncActionFilter
    {
        private readonly ApplicationDBContext dbContext;

        public FiltroValidacionLibro(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("libroCreateDTO", out var value) || value is not LibroCreateDTO libroCreateDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es Valido");
                context.Result = context.ModelState.ConstruirProblemaDetail();
                return;
            }

            if (libroCreateDTO.AutoresIds is null || libroCreateDTO.AutoresIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(libroCreateDTO.AutoresIds), "No se puede actualizar libro sin autores");
                context.Result = context.ModelState.ConstruirProblemaDetail();
                return;
            }

            var autoresIdExisten = await dbContext.Autores.Where(x => libroCreateDTO.AutoresIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

            if (autoresIdExisten.Count != libroCreateDTO.AutoresIds.Count)
            {
                var autoresIdsNoExisten = libroCreateDTO.AutoresIds.Except(autoresIdExisten);
                var autoresIdsNoExistenString = string.Join(", ", autoresIdsNoExisten);
                var mensajeError = $"Los siguientes autores no existen";
                context.ModelState.AddModelError(nameof(libroCreateDTO.AutoresIds), mensajeError);
                context.Result = context.ModelState.ConstruirProblemaDetail();
                return;
            }



            await next();
        }
    }
}
