using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DesafioTecnico.Api.Filters
{
    /// <summary>
    /// Rejeita requisições onde algum parâmetro de rota do tipo <see cref="Guid"/> seja <see cref="Guid.Empty"/>, retornando 400.
    /// </summary>
    public class ValidateRouteGuidsFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var (key, value) in context.ActionArguments)
            {
                if (value is Guid g && g == Guid.Empty)
                {
                    context.Result = new BadRequestObjectResult(
                        new { error = $"O parâmetro '{key}' não pode ser um Guid vazio." });
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
