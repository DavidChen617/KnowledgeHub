using Api.Endpoints.Notes;
using Application.Notes;
using CoreMesh.Dispatching.Extensions;
using CoreMesh.Endpoints.Extensions;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDispatching([typeof(AddNoteHandler).Assembly]);
builder.Services.AddEndpoints([typeof(NotesGroup).Assembly]);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();
