using System.Runtime.InteropServices;

public enum PACKET_TYPE : byte
{
    C2S_LOGIN_USER = 1,
    S2C_LOGIN_USER,

    C2S_LOGOUT_USER,
    S2C_LOGOUT_USER,

    C2S_ENTER_CHATROOM,
    S2C_ENTER_CHATROOM,

    C2S_LEAVE_CHATROOM,
    S2C_LEAVE_CHATROOM,

    C2S_CHAT_MESSAGE,
    S2C_CHAT_MESSAGE,
};

// only use type and size
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BASIC_PACKET
{
    public byte type;
    public byte size;
}

// [Client to Server packet type]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C2S_LOGIN_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] user_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C2S_LOGOUT_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] user_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C2S_ENTER_CHATROOM_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] chatroom_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C2S_LEAVE_CHATROOM_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] chatroom_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C2S_MESSAGE_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] message;
};


// [Server to Client packet type]

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S2C_LOGIN_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] user_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S2C_LOGOUT_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] user_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S2C_ENTER_CHATROOM_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] chatroom_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S2C_LEAVE_CHATROOM_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] chatroom_name;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S2C_MESSAGE_PACKET
{
    public byte type;
    public byte size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] message;
};