using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DesafioTecnico.Api.Filters
{
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
