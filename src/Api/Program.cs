using Api.Endpoints.Notes;
using Application.Notes;
using CoreMesh.Dispatching.Extensions;
using CoreMesh.Endpoints.Extensions;
using Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer((operation, _, ct) =>
    {
        operation.Parameters ??= [];
        operation.Parameters.Insert(0, new OpenApiParameter
        {
            Name = "X-User-Id",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" }
        });
        return Task.CompletedTask;
    });
});

builder.Services.AddDispatching([typeof(AddNoteHandler).Assembly]);
builder.Services.AddEndpoints([typeof(NotesGroup).Assembly]);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();
