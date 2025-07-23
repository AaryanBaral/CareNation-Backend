using System.Data;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace backend.ExceptionHandling;

public class GlobalExceptionHandling : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandling> _logger;

    public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        _logger.LogError(exception,
            "Could not process a request on machine {MachineName}, TraceId:{TraceId}",
            Environment.MachineName,
            traceId);

        var (statusCode, title) = MapException(exception);

        context.Response.StatusCode = statusCode;
    // ✅ Build a complete ProblemDetails object with the correct status code
    var problemDetails = new ProblemDetails
    {
        Title = title,
        Status = statusCode,
        Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1", // optional
        Extensions = { ["traceId"] = traceId }
    };

    // ✅ Use the correct overload
    await Results.Problem(problemDetails).ExecuteAsync(context);

    return true;
    }

    private static (int statusCode, string title) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentOutOfRangeException => ((int)HttpStatusCode.BadRequest, exception.Message),
            ArgumentNullException => ((int)HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, exception.Message),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, exception.Message),
            TimeoutException => ((int)HttpStatusCode.InternalServerError, exception.Message),
            DbUpdateException => ((int)HttpStatusCode.BadRequest, exception.Message),
            InvalidCastException => ((int)HttpStatusCode.BadRequest, exception.Message),
            FormatException => ((int)HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            AuthenticationFailureException => ((int)HttpStatusCode.BadRequest, exception.Message),
            MySqlException mySqlEx => HandleMySqlException(mySqlEx),
            DuplicateNameException => ((int)HttpStatusCode.BadRequest, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, exception.Message),
        };
    }

    private static (int statusCode, string title) HandleMySqlException(MySqlException ex)
    {
        return ex.Number switch
        {
            1062 => ((int)HttpStatusCode.BadRequest, "Duplicate entry, unique constraint violation."), // ER_DUP_ENTRY
            1452 => ((int)HttpStatusCode.BadRequest, "Foreign key constraint fails."), // ER_NO_REFERENCED_ROW_2
            1049 => ((int)HttpStatusCode.InternalServerError, "Unknown database."), // ER_BAD_DB_ERROR
            1045 => ((int)HttpStatusCode.Unauthorized, "Access denied for user."), // ER_ACCESS_DENIED_ERROR
            1146 => ((int)HttpStatusCode.InternalServerError, "Table doesn't exist."), // ER_NO_SUCH_TABLE
            1364 => ((int)HttpStatusCode.BadRequest, "Field doesn't have a default value."), // ER_NO_DEFAULT_FOR_FIELD
            2002 => ((int)HttpStatusCode.InternalServerError, "Can't connect to MySQL server."),
            2006 => ((int)HttpStatusCode.InternalServerError, "MySQL server has gone away."),
            _ => ((int)HttpStatusCode.InternalServerError, "MySQL Error: " + ex.Message)
        };
    }
}
