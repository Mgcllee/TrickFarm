using System.Net.Sockets;
using System.Text;

class Client
{
    private static TcpClient _client;
    private static NetworkStream _stream;
    private static string user_name;

    static async Task Main(string[] args)
    {
        // 서버의 IP 주소와 포트 번호
        string serverIp = "127.0.0.1"; // 로컬호스트
        int port = 5000;

        _client = new TcpClient();
        await _client.ConnectAsync(serverIp, port);
        Console.WriteLine("서버에 연결되었습니다.");

        Console.WriteLine("유저 이름을 입력해주세요");
        user_name = Console.ReadLine()!;

        _stream = _client.GetStream();
        byte[] buffer = Encoding.UTF8.GetBytes(user_name);
        _stream.Write(buffer, 0, buffer.Length);

        // 수신 작업을 별도의 스레드에서 실행
        var cancellationTokenSource = new CancellationTokenSource();
        var receiveTask = Task.Run(() => ReceiveMessagesAsync(cancellationTokenSource.Token));

        // 사용자 입력 처리
        await HandleUserInputAsync(cancellationTokenSource);

        // 클라이언트 종료 시 수신 작업 취소
        cancellationTokenSource.Cancel();
        await receiveTask; // 수신 작업이 완료될 때까지 대기
        _client.Close();
    }

    private static async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        string user_name = Console.ReadLine();
        byte[] name_buffer = Encoding.UTF8.GetBytes(user_name);
        _stream.Write(name_buffer, 0, name_buffer.Length);

        var buffer = new byte[1024];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) break; // 서버가 연결을 종료한 경우

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"서버로부터 수신: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"수신 중 오류 발생: {ex.Message}");
        }
    }

    private static async Task HandleUserInputAsync(CancellationTokenSource cancellationTokenSource)
    {
        while (true)
        {
            Console.Write("메시지를 입력하세요 (종료하려면 'exit' 입력): ");
            string message = Console.ReadLine();
            if (message.ToLower() == "exit") break;

            // 메시지를 바이트 배열로 변환하여 서버에 전송
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            Console.WriteLine($"서버에 요청 전송: {message}");
        }

        cancellationTokenSource.Cancel(); // 입력 종료 시 수신 작업 취소
    }
}
