using System.Text;
using Api.Endpoints.Notes;
using Application.Auth;
using Application.EventHandlers;
using Application.Notes;
using CoreMesh.Dispatching.Extensions;
using CoreMesh.Endpoints.Extensions;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer((operation, _, ct) =>
    {
        operation.Parameters ??= [];
        operation.Parameters.Insert(0, new OpenApiParameter
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
        return Task.CompletedTask;
    });
});

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDispatching([typeof(AddNoteHandler).Assembly]);
builder.Services.AddEndpoints([typeof(NotesGroup).Assembly]);
builder.Services.AddInfrastructure(builder.Configuration, typeof(NoteDeletedEventHandler).Assembly);

var app = builder.Build();

app.MapOpenApi();
app.MapEndpoints();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
