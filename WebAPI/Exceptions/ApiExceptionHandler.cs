using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Exceptions;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = MapException(exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Lỗi không mong đợi khi xử lý {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogInformation(
                "Request {Method} {Path} trả về {StatusCode}: {Detail}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                problem.Status,
                problem.Detail);
        }

        problem.Instance = httpContext.Request.Path;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problem.Status!.Value;

        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });

        return true;
    }

    private static ProblemDetails MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Không tìm thấy dữ liệu",
                Detail = notFound.Message
            },
            ArgumentException invalidInput => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dữ liệu không hợp lệ",
                Detail = invalidInput.Message
            },
            InvalidOperationException conflict => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Xung đột dữ liệu",
                Detail = conflict.Message
            },
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Xung đột dữ liệu",
                Detail = "Dữ liệu đã tồn tại."
            },
            DbUpdateException { InnerException: SqlException { Number: 547 } } => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Xung đột dữ liệu",
                Detail = "Thao tác vi phạm ràng buộc dữ liệu."
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Lỗi máy chủ",
                Detail = "Đã xảy ra lỗi không mong muốn. Hãy cung cấp mã traceId khi báo lỗi."
            }
        };
    }
}
