using Orleans;

namespace ChatRoom;

// TODO: GenerateSerializer, Immutable 연구
// Immutable Attribute: 불변 객체로 설정하는 Attribute, 생성 후 내부 데이터는 읽기 전용
// GenerateSerializer: 컴파일 타임에 자동으로 직렬화/역직렬화 시킬 수 있는 Attribute

/*
record 는 Immutable 키워드와 함께 사용되는 속성으로
C#의 Class는 참조 타입(Reference Type)이라서, 
두 객체가 같은 데이터를 가지고 있어도 메모리 주소가 __다르면__ 서로 __다른 객체__ 로 인식.
하지만 record는 값 중심(Value-Based)이라서, 
객체의 메모리 주소가 아니라 __내부 데이터 값이 같으면 동일한 객체__ 로 취급해.
*/

[GenerateSerializer, Immutable]
public record class ChatMsg(
    string? Author, // string?은 Author가 Text와 다르게 Null을 허용한다는 의미.
    string Text)
{
    // TODO: ProtoBuf와 관련이 있는가?
    [Id(0)]
    public string Author { get; init; } = Author ?? "Anonymous";

    [Id(1)]
    public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;
}