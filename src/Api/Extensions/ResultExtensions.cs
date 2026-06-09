using Api.Endpoints;
using Microsoft.AspNetCore.Mvc;
using ShareKernal;

namespace Api.Extensions;

public static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public IResult ToHttpResult()
            => result.IsSuccess ? Results.Ok(Response.Ok(result.Value)) : result.Error.ToResponse();

        public IResult ToHttpResult<TOut>(Func<T, TOut> map)
            => result.IsSuccess ? Results.Ok(Response.Ok(map(result.Value))) : result.Error.ToResponse();

        public IResult ToCreated(Func<T, string> uri)
            => result.IsSuccess
                ? Results.Created(uri(result.Value), Response.Ok(result.Value))
                : result.Error.ToResponse();
    }

    extension(Result result)
    {
        public IResult ToNoContent()
            => result.IsSuccess ? Results.NoContent() : result.Error.ToResponse();

        public IResult ToHttpResult<TOut>(TOut value)
            => result.IsSuccess ? Results.Ok(Response.Ok(value)) : result.Error.ToResponse();
    }

    public static IResult NotFound() => Results.Json(
        Response.Fail(new ProblemDetails { Status = StatusCodes.Status404NotFound }),
        statusCode: StatusCodes.Status404NotFound);

    extension(Error error)
    {
        private IResult ToResponse()
        {
            var status = error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
                ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError,
            };
            return Results.Json(
                Response.Fail(new() { Status = status, Detail = error.Description }),
                statusCode: status);
        }
    }
}
