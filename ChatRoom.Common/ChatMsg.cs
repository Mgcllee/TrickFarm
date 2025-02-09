using Orleans;

namespace ChatRoom;

// TODO: GenerateSerializer, Immutable 연구
[GenerateSerializer, Immutable]
public record class ChatMsg(
    string? Author,
    string Text)
{
    // TODO: record class keyword 연구
}