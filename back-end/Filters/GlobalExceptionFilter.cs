using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocApi.Filters;

public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;

        // Choisir le status code et le message selon le type d'exception
        var result = exception switch
        {
            KeyNotFoundException => new NotFoundObjectResult(new { message = exception.Message }),
            ArgumentException or InvalidOperationException => new BadRequestObjectResult(new { message = exception.Message }),
            _ => new ObjectResult(new { message = "Une erreur interne est survenue." })
            { StatusCode = 500 }
        };

        context.Result = result;
        context.ExceptionHandled = true; // L'exception est traitée

        return Task.CompletedTask;
    }
}