using System.Runtime.InteropServices;

public enum PACKET_TYPE : byte
{
    C2S_LOGIN_USER = 1,
    C2S_CHAT_MESSAGE,


};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
class BASIC_PACKET
{
    public byte type;
    public byte size;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
class C2S_LOGIN_PACKET : BASIC_PACKET
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] user_name = new byte[10];
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
class C2S_MESSAGE_PACKET : BASIC_PACKET
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] message = new byte[100];
};