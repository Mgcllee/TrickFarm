using System.Runtime.InteropServices;

public class Common
{
    public const string SHM_NAME = "/EPollOrleans_shm";
    public const string SEM_MUTEX_NAME = "/EPollOrleans_mutex";
    // public const string sem_csharp2cpp = "/sem_csharp2c";
    public const string SEM_CPP_TURN = "/EPollOrleans_cpp_turn";
    // public const string sem_cpp2csharp = "/sem_c2csharp";
    public const string SEM_CS_TURN = "/EPollOrleans_cs_turn";

    // 128 Clients (each memory 1024 byte)
    public const int SHM_SIZE = 131072; //(128 * 1024)

    public const int MESSAGE_BUFFER_SIZE = 200;
    public const int USER_NAME_SIZE = 10;
}
