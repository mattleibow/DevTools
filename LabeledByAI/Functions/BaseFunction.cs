using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LabeledByAI;

public abstract class BaseFunction<TBody>(ILogger logger)
{
#if DEBUG
    private const bool IncludeExceptionDetails = true;
#else
    private const bool IncludeExceptionDetails = false;
#endif

    private HttpRequest? _request;

    public virtual async Task<IResult> Run(HttpRequest request)
    {
        _request = request;

        logger.LogInformation("Function is starting...");

        try
        {
            // parse the request body
            var parsedBody = await ParseRequestBodyAsync(request);

            // run the function
            return await OnRun(request, parsedBody);
        }
        catch (ProblemDetailsException ex)
        {
            // we got a problem details exception

            logger.LogError("Failed to execute the function.");

            return Problem(ex.ProblemDetails, ex.InnerException);
        }
        catch (UnauthorizedAccessException ex)
        {
            // something else happened and we don't know what

            logger.LogError(ex, "Failed to execute the function.");

            return Problem(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Detail = ex.Message
            }, ex);
        }
        catch (ArgumentException ex)
        {
            // something else happened and we don't know what

            logger.LogError(ex, "Failed to execute the function.");

            return Problem(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            }, ex);
        }
        catch (Exception ex)
        {
            // something else happened and we don't know what

            logger.LogError(ex, "Failed to execute the function.");

            return Problem(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = string.IsNullOrWhiteSpace(ex.Message)
                    ? $"There was a problem executing the request, please use the " +
                      $"trace identifier for more information. "
                    : ex.Message

            }, ex);
        }
        finally
        {
            // done.

            logger.LogInformation("Function run is complete for Trace ID.");

            _request = null;
        }
    }

    protected abstract Task<IResult> OnRun(HttpRequest request, TBody parsedBody);

    private async Task<TBody> ParseRequestBodyAsync(HttpRequest request)
    {
        TBody? body;
        try
        {
            body = await JsonSerializer.DeserializeAsync<TBody>(
                request.Body, JsonExtensions.SerializerOptions);
        }
        catch (Exception ex)
        {
            throw new ProblemDetailsException(new()
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = $"Failed to deserialize the body."
            }, ex);
        }

        if (body is null)
        {
            throw new ProblemDetailsException(new()
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "The deserialized body was an empty object.",
            });
        }

        return body;
    }

    protected IResult Problem(ProblemDetails problem, Exception? exception = null)
    {
        // copy the problem details to a new instance
        var pd = new ProblemDetails
        {
            Detail = problem.Detail,
            Status = problem.Status,
            Title = problem.Title,
            Type = problem.Type,
            Instance = problem.Instance,
        };

        if (problem.Extensions is not null)
            pd.Extensions = new Dictionary<string, object?>(problem.Extensions);

        // add the trace and activity identifiers
        if (_request is not null)
        {
            if (pd.Instance is null)
                pd.Instance = $"{_request.HttpContext.Request}";

            if (_request.HttpContext.TraceIdentifier is { } traceId)
                pd.Extensions.TryAdd("traceId", traceId);

            if (_request.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity is { } activity)
                pd.Extensions.TryAdd("activityId", activity.Id);

            if (exception is not null)
                pd.Extensions.TryAdd("exceptions", GetErrors(exception).ToList());
        }

        return TypedResults.Problem(pd);
    }

    private IEnumerable<IDictionary<string, object?>> GetErrors(Exception exception)
    {
        yield return GetError(exception);

        if (IncludeExceptionDetails)
        {
            if (exception.InnerException is not null)
                yield return GetError(exception.InnerException);
        }

        static Dictionary<string, object?> GetError(Exception exception)
        {
            var dic = new Dictionary<string, object?>()
            {
                ["message"] = exception.Message,
            };
            if (IncludeExceptionDetails)
            {
                dic["type"] = exception.GetType().FullName;
                dic["stackTrace"] = exception.StackTrace;
            }
            return dic;
        }
    }
}
