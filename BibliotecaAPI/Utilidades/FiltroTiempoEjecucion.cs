using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroTiempoEjecucion : IAsyncActionFilter
    {
        private readonly ILogger<FiltroTiempoEjecucion> logger;

        public FiltroTiempoEjecucion(ILogger<FiltroTiempoEjecucion> logger)
        {
            this.logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Antes de la ejecución de la acción
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation($"Inicio acción: {context.ActionDescriptor.DisplayName}");

            await next();

            //Despues de la ejecucion de la accion
            stopwatch.Stop();
            logger.LogInformation($"Fin acción: {context.ActionDescriptor.DisplayName} - Tiempo: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
