/*
TODO: 이 프로젝트는
ChatRoom.Client와 ChatRoom.Service 에서 동시에 사용되는데
Dockerfile을 만들 경우, 어떻게 처리해야 하는가
*/

using Orleans;
using Orleans.Runtime;

namespace ChatRoom;

public interface IChannelGrain : IGrainWithStringKey
{

}