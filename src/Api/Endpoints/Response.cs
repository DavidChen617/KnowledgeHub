using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Response
{
    public bool IsSuccess => Problem is null;
    public ProblemDetails? Problem { get; }

    protected Response(ProblemDetails? problem)
    {
        Problem = problem;
    }

    public static Response Ok() => new(null);
    public static Response Fail(ProblemDetails problem) => new(problem);

    public static Response<T> Ok<T>(T data) => new(data);
    public static Response<T> Fail<T>(ProblemDetails problem) => new(problem);
}

public class Response<T> : Response
{
    public T? Data { get; }
    protected internal Response(T data) : base(null) => Data = data;
    protected internal Response(ProblemDetails problem) : base(problem) { }
}
