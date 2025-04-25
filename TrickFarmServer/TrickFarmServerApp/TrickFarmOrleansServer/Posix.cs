using System.Runtime.InteropServices;

public static class Posix
{
    public const int O_RDWR = 2;   // Read/write access
 
    // mmap protection flags
    public const int PROT_READ = 0x1; // Page can be read
    public const int PROT_WRITE = 0x2; // Page can be written

    // mmap flags
    public const int MAP_SHARED = 0x01; // Share changes

    // Error return values
    public static readonly IntPtr MAP_FAILED = new IntPtr(-1);
    public static readonly IntPtr SEM_FAILED = new IntPtr(-1); // Typically represented this way, though C defines it differently

    // --- Native Functions (libc) ---
    [DllImport("libc", SetLastError = true)]
    public static extern int shm_open(string name, int oflag, uint mode);

    [DllImport("libc", SetLastError = true)]
    public static extern int shm_unlink(string name);

    [DllImport("libc", SetLastError = true)]
    public static extern IntPtr mmap(IntPtr addr, UIntPtr length, int prot, int flags, int fd, IntPtr offset);

    [DllImport("libc", SetLastError = true)]
    public static extern int munmap(IntPtr addr, UIntPtr length);

    [DllImport("libc", SetLastError = true)]
    public static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    public static extern IntPtr sem_open(string name, int oflag, uint mode, uint value);

    [DllImport("libc", SetLastError = true)]
    public static extern IntPtr sem_open(string name, int oflag);

    [DllImport("libc", SetLastError = true)]
    public static extern int sem_close(IntPtr sem);

    [DllImport("libc", SetLastError = true)]
    public static extern int sem_unlink(string name);

    [DllImport("libc", SetLastError = true)]
    public static extern int sem_wait(IntPtr sem);

    [DllImport("libc", SetLastError = true)]
    public static extern int sem_post(IntPtr sem);
}