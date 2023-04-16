using System;
using System.Runtime.InteropServices;


namespace Protector.Protections.Injections
{
    internal class anti_dump_runtime
    {
        [DllImport("kernel32.dll")]
        public static extern unsafe bool VirtualProtect(
          byte* lpAddress,
          int dwSize,
          uint flNewProtect,
          out uint lpflOldProtect);

        public static unsafe void Initialize()
        {
            byte* hinstance = (byte*)(void*)Marshal.GetHINSTANCE(typeof(anti_dump_runtime).Module);
            byte* numPtr1 = hinstance + 60;
            byte* numPtr2;
            byte* numPtr3 = (numPtr2 = hinstance + *(uint*)numPtr1) + 6;
            ushort num1 = *(ushort*)numPtr3;
            byte* numPtr4 = numPtr3 + 14;
            ushort num2 = *(ushort*)numPtr4;
            byte* lpAddress1 = numPtr2 = numPtr4 + 4 + (int)num2;
            byte* numPtr5 = stackalloc byte[11];
            uint lpflOldProtect;
            anti_dump_runtime.VirtualProtect(lpAddress1 - 16, 8, 64U, out lpflOldProtect);
            *(int*)(lpAddress1 - 12) = 0;
            byte* lpAddress2 = hinstance + *(uint*)(lpAddress1 - 16);
            *(int*)(lpAddress1 - 16) = 0;
            anti_dump_runtime.VirtualProtect(lpAddress2, 72, 64U, out lpflOldProtect);
            byte* lpAddress3 = hinstance + *(uint*)(lpAddress2 + 8);
            *(int*)lpAddress2 = 0;
            *(int*)(lpAddress2 + 4) = 0;
            //   *(int*)(lpAddress2 + (new IntPtr(2) * 4).ToInt64()) = 0;
            //   *(int*)(lpAddress2 + (new IntPtr(3) * 4).ToInt64()) = 0;
            anti_dump_runtime.VirtualProtect(lpAddress3, 4, 64U, out lpflOldProtect);
            *(int*)lpAddress3 = 0;
            for (int index = 0; index < (int)num1; ++index)
            {
                anti_dump_runtime.VirtualProtect(lpAddress1, 8, 64U, out lpflOldProtect);
                Marshal.Copy(new byte[8], 0, (IntPtr)(void*)lpAddress1, 8);
                lpAddress1 += 40;
            }
        }
    }
}
