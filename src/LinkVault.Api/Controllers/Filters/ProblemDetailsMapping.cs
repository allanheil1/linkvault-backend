using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace LinkVault.Api.Controllers.Filters;

public static class ProblemDetailsMapping
{
    public static void MapProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandler?.Error;

                var status = exception switch
                {
                    UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                    KeyNotFoundException => HttpStatusCode.NotFound,
                    InvalidOperationException => HttpStatusCode.Conflict,
                    _ => HttpStatusCode.BadRequest
                };

                var problem = new ProblemDetails
                {
                    Status = (int)status,
                    Title = exception?.Message ?? "Unexpected error"
                };

                context.Response.StatusCode = problem.Status ?? 500;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problem);
            });
        });
    }
}
