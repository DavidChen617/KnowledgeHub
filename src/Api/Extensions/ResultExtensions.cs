using ShareKernal;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
        => result.IsSuccess ? onSuccess(result.Value) : result.Error.ToProblem();

    public static IResult ToHttpResult<T>(this Result<T> result)
        => result.ToHttpResult(v => Results.Ok(v));

    public static IResult ToNoContent(this Result result)
        => result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();

    private static IResult ToProblem(this Error error) => error.Type switch
    {
        ErrorType.NotFound           => Results.Problem(statusCode: StatusCodes.Status404NotFound),
        ErrorType.Forbidden          => Results.Problem(statusCode: StatusCodes.Status403Forbidden),
        ErrorType.Conflict           => Results.Problem(statusCode: StatusCodes.Status409Conflict,           detail: error.Description),
        ErrorType.Validation         => Results.Problem(statusCode: StatusCodes.Status400BadRequest,         detail: error.Description),
        ErrorType.ServiceUnavailable => Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, detail: error.Description),
        _                            => Results.Problem(detail: error.Description)
    };
}
