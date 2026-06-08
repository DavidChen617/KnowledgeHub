using ShareKernal;

namespace Application.Notes;

public static class NoteErrors
{
    public static readonly Error NotFound  = new("Note.NotFound",  "Note not found",          ErrorType.NotFound);
    public static readonly Error Forbidden = new("Note.Forbidden", "Access denied",            ErrorType.Forbidden);
    public static readonly Error ReadOnly  = new("Note.ReadOnly",  "Share token is read-only", ErrorType.Forbidden);
    public static readonly Error TokenNotFound = new("Note.TokenNotFound", "Share token not found", ErrorType.NotFound);
}
