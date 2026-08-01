
using CRMApi.Exceptions.Types;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var problem = CreateProblemDetails(exception , httpContext);

            httpContext.Response.StatusCode = problem.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }

        private static ProblemDetails CreateProblemDetails(Exception exception, HttpContext httpContext)
        {
            var problem = exception switch
            {
                NotFoundException => new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Resource not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = exception.Message
                },

                BadRequestException => new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Bad Request",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = exception.Message
                },

                ConflictException => new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Duplicate",
                    Status = StatusCodes.Status409Conflict,
                    Detail = exception.Message
                },

                AuthenticationException => new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Permission Denied",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = exception.Message
                },

                ValidationsException => new ValidationProblemDetails
                {
                    Type = "about:blank",
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = exception.Message,
                },

                _ => new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Internal server error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occured"
                }
            };

            problem.Instance = httpContext.Request.Path;
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;
            problem.Extensions["timeStamp"] = DateTime.UtcNow;

            return problem;
        }
    }
}