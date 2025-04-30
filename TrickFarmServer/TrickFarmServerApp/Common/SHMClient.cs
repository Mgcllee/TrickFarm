using System.Runtime.InteropServices;

public enum SHM_CLIENT_TYPE
{
    turn_cpp = 0,
    turn_csharp = 1
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SHM_CLIENT 
{
    public byte turn;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.USER_NAME_SIZE)]
    public byte[] user_name;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.MESSAGE_BUFFER_SIZE)]
    public byte[] message;
}