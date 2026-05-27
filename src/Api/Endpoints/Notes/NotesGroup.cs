using CoreMesh.Endpoints;

namespace Api.Endpoints.Notes;

public sealed class NotesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/notes";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Notes");
    }
}
