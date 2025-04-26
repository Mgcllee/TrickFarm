using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SHM_CLIENT 
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.USER_NAME_SIZE)]
    public byte[] user_name;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.MESSAGE_BUFFER_SIZE)]
    public byte[] message;
}