namespace Application.Interfaces;

public static class CacheKeys
{
    public static string Categories(Guid userId) => $"categories:{userId}";
    public static string NoteList(Guid userId) => $"notes:list:{userId}";
    public static string NoteGraph(Guid userId) => $"notes:graph:{userId}";
    public static string NoteByToken(string token) => $"notes:token:{token}";
    public static string Comments(Guid noteId) => $"comments:{noteId}";
}
