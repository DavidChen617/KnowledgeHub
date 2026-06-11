using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Notes;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record GetNoteGraphQuery(UserId UserId) : IRequest<Result<GetNoteGraphDto>>;

public record GraphNode(string Id, string Type, string Label);
public record GraphEdge(string Source, string Target, string Type);
public record GetNoteGraphDto(IReadOnlyList<GraphNode> Nodes, IReadOnlyList<GraphEdge> Edges);

public class GetNoteGraphHandler(INoteRepository noteRepository, ICategoryRepository categoryRepository)
    : IRequestHandler<GetNoteGraphQuery, Result<GetNoteGraphDto>>
{
    public async Task<Result<GetNoteGraphDto>> Handle(GetNoteGraphQuery query, CancellationToken cancellationToken = default)
    {
        var notes = await noteRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);
        var categories = await categoryRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);

        var nodes = new List<GraphNode>();
        var edges = new List<GraphEdge>();

        foreach (var cat in categories)
            nodes.Add(new GraphNode($"cat-{cat.Id}", "category", cat.Name));

        var noteIds = notes.Select(n => n.Id.Value).ToHashSet();

        foreach (var note in notes)
        {
            nodes.Add(new GraphNode($"note-{note.Id.Value}", "note", note.Title));

            if (note.CategoryId is not null)
                edges.Add(new GraphEdge($"note-{note.Id.Value}", $"cat-{note.CategoryId.Value}", "category"));

            foreach (var linkedId in note.LinkedNoteIds.Where(id => noteIds.Contains(id.Value)))
                edges.Add(new GraphEdge($"note-{note.Id.Value}", $"note-{linkedId.Value}", "link"));
        }

        return Result.Success(new GetNoteGraphDto(nodes, edges));
    }
}
