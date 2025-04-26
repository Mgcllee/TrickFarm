using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.AccessControl;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

class TcpChatServer
{
    private readonly IGrainFactory _grainFactory;

    private RedisConnector redis_connector = null!;

    public TcpChatServer(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task StartAsync(RedisConnector redis_connector)
    {
        this.redis_connector = redis_connector;

        Console.WriteLine("[Server][Start] Posix Shared memory");

        int shm_fd = -1;
        IntPtr mappedPtr = Posix.MAP_FAILED;
        IntPtr semWrite = Posix.SEM_FAILED;
        IntPtr semRead = Posix.SEM_FAILED;
        
        

        try {
            shm_fd = Posix.shm_open(Common.SHM_NAME, Posix.O_RDWR, 0);
            mappedPtr = Posix.mmap(IntPtr.Zero, Common.SHM_SIZE, Posix.PROT_READ, Posix.MAP_SHARED, shm_fd, IntPtr.Zero);
            semWrite = Posix.sem_open(Common.SEM_CS_TURN, 0);
            semRead = Posix.sem_open(Common.SEM_CPP_TURN, 0);

            Posix.sem_wait(semWrite); // EPollServer의 작업이 끝날 때까지 대기.

            string message = Marshal.PtrToStringUTF8(mappedPtr + 1024);

        }
        catch (Exception ex) {
            Console.WriteLine($"[Server][Start] shm_open error: {ex.Message}");
            return;
        }

        while (true)
        {
            
            /*
            if (name_len > 0 && user_name is not "leave" 
                && user_name.Length <= 10 
                && user_name.Contains(" ")
                && redis_connector.write_user_info(user_guid, user_name))
            {
                var client_grain = _grainFactory.GetGrain<IChatClientGrain>(user_guid);
                Console.WriteLine($"Redis에 기록 성공! 어서오세요 {user_name}님");
                await ClientRequestWorkerAsync(client_grain);
            }
            else
            {
                redis_connector.delete_user_info(user_guid);
                Console.WriteLine("Redis에 기록 실패...");
            }
            */
        }
    }

    private async Task ClientRequestWorkerAsync(IChatClientGrain client_grain)
    {
        while(true) 
        {
            byte[] request_buffer = new byte[1024];
            // client_stream.ReadAsync()
            switch(request_buffer[0]) 
            {
                
                default: {
                    break;
                }
            }
        }
    }

    public void StopServer()
    {
        redis_connector.disconnect_redis();
        Console.WriteLine("Redis 메모리 FlushALL 처리 완료");
    }
}