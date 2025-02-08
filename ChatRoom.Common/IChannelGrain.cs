/*
TODO: 이 프로젝트는
ChatRoom.Client와 ChatRoom.Service 에서 동시에 사용되는데
Dockerfile을 만들 경우, 어떻게 처리해야 하는가
*/

using Orleans;
using Orleans.Runtime;

namespace ChatRoom;

/*
Orleans가 Grain을 추적하는 데 사용하는 식별자에 대해 클래스를 다른 데이터 형식(예: 문자열 또는 정수)으로 표시합니다.
즉, Grain 인터페이스는 IGrainWith___ 인터페이스를 상속 받아서 사용해야 한다.
그리고 Grain 클래스에서 Grain 인터페이스를 상속받아 구현한다.
*/

public interface IChannelGrain : IGrainWithStringKey
{
    // TODO: Task, StreamId(Orleans.Streaming Nuget Package) 연구
    Task<StreamId> Join(string nickname);
}