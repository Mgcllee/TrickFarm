using System.Runtime.InteropServices;

public class Common
{
    public const string SHM_NAME = "/EPollOrleans_shm";
    public const string SEM_MUTEX_NAME = "/EPollOrleans_mutex";
    // public const string sem_csharp2cpp = "/sem_csharp2c";
    public const string SEM_CPP_TURN = "/EPollOrleans_cpp_turn";
    // public const string sem_cpp2csharp = "/sem_c2csharp";
    public const string SEM_CS_TURN = "/EPollOrleans_cs_turn";

    public const int MAX_CLIENT = 128;

    public readonly int SHM_SIZE = MAX_CLIENT * Marshal.SizeOf(new SHM_CLIENT());

    public const int MESSAGE_BUFFER_SIZE = 200;
    public const int USER_NAME_SIZE = 10;
}
