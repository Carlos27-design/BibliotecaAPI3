using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class MiFiltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFiltroDeAccion> logger;

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            this.logger = logger;
        }

        /*Esto se ejecuta antes de la acción */
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Ejecutando la acción");
        }

        /*Despues de la acción*/
        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("Acción ejecutada");
        }
    }
}
