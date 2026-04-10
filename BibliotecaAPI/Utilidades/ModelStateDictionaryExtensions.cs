using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliotecaAPI.Utilidades
{
    public static class ModelStateDictionaryExtensions
    {
        public static BadRequestObjectResult ConstruirProblemaDetail(this ModelStateDictionary modelState)
        {
            var problemDetail = new ValidationProblemDetails(modelState)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(problemDetail);
        }
    }
}
