using Domain.Notes;
using Domain.Shared;

namespace Domain.Exceptions;

public class NoteNotFoundException(NoteId noteId)
    : DomainException($"找不到筆記（ID：{noteId.Value}）");
