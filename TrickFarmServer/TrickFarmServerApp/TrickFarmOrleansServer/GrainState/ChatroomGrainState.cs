using System.Collections.Concurrent;

public class ChatroomGrainState
{
    public readonly ConcurrentDictionary<Guid, string> room_member
        = new ConcurrentDictionary<Guid, string>();
    public readonly List<string> chat_log = new();
}
