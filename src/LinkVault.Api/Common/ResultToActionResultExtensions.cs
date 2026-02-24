using LinkVault.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Common;

public static class ResultToActionResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return onSuccess is null ? controller.Ok(result.Value) : onSuccess(result.Value);
        }

        return BuildFailure(controller, result.Error);
    }

    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller,
        Func<IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is null ? controller.NoContent() : onSuccess();
        }

        return BuildFailure(controller, result.Error);
    }

    private static IActionResult BuildFailure(ControllerBase controller, ResultError? error)
    {
        var (statusCode, title) = error?.Type switch
        {
            ResultErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ResultErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ResultErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ResultErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ResultErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => (StatusCodes.Status400BadRequest, "Request failed")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error?.Message,
            Instance = controller.HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;
        problem.Extensions["correlationId"] = controller.HttpContext.Response.Headers["X-Correlation-ID"].ToString();
        if (error?.ValidationErrors is not null)
        {
            problem.Extensions["errors"] = error.ValidationErrors;
        }

        return controller.StatusCode(statusCode, problem);
    }
}
