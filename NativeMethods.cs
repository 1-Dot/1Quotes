using System.Runtime.InteropServices;

namespace _1Quotes
{
    /// <summary>
    /// 原生互操作（P/Invoke）辅助类：包含低级键盘钩子及输入模拟所需的常量、结构与函数。
    /// </summary>
    internal static class NativeMethods
    {
        // 钩子与消息常量 ----------------------------------------------------
        internal const int WH_KEYBOARD_LL = 13;      // 低级键盘钩子 ID
        internal const int WM_KEYDOWN = 0x0100;      // 按键按下消息
        internal const int WM_SYSKEYDOWN = 0x0104;   // 系统按键（Alt 等）按下消息
        internal const int HC_ACTION = 0;            // 有效的钩子动作代码

        // 虚拟键码 ------------------------------------------------------------
        internal const int VK_LSHIFT = 0xA0; // 左 Shift
        internal const int VK_RSHIFT = 0xA1; // 右 Shift
        internal const int VK_OEM_4 = 0xDB;  // '[' 键
        internal const int VK_OEM_6 = 0xDD;  // ']' 键

        // SendInput 标志位 ----------------------------------------------------
        internal const uint INPUT_KEYBOARD = 1;      // 键盘输入类型
        internal const uint KEYEVENTF_KEYUP = 0x0002;     // 按键抬起
        internal const uint KEYEVENTF_UNICODE = 0x0004;   // 发送 Unicode 字符

        // 委托 / 结构体 -------------------------------------------------------
        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;      // 虚拟键码
            public uint scanCode;    // 扫描码
            public uint flags;       // 标志位
            public uint time;        // 时间戳
            public IntPtr dwExtraInfo; // 额外信息
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint type;                        // 输入类型
            public MOUSEKEYBDHARDWAREINPUT mkhi;     // 共用体（鼠标/键盘/硬件）
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;   // 键盘输入
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)] public HARDWAREINPUT hi; // 硬件输入
            [FieldOffset(0)] public KEYBDINPUT ki;    // 键盘输入
            [FieldOffset(0)] public MOUSEINPUT mi;    // 鼠标输入
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint uMsg;        // 消息
            public ushort wParamL;   // 参数低位
            public ushort wParamH;   // 参数高位
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;           // 鼠标 X 位移
            public int dy;           // 鼠标 Y 位移
            public uint mouseData;   // 鼠标附加数据
            public uint dwFlags;     // 鼠标标志
            public uint time;        // 时间戳
            public IntPtr dwExtraInfo; // 额外信息
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort wVk;       // 虚拟键码
            public ushort wScan;     // 硬件扫描码
            public uint dwFlags;     // 标志位
            public uint time;        // 时间戳
            public IntPtr dwExtraInfo; // 额外信息
        }

        // P/Invoke -----------------------------------------------------------
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")] internal static extern short GetKeyState(int nVirtKey); // 获取按键状态

        [DllImport("user32.dll")] internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize); // 发送模拟输入

        /// <summary>
        /// 判断任意 Shift 键是否被按下。
        /// </summary>
        internal static bool IsShiftPressed()
        {
            return (GetKeyState(VK_LSHIFT) & 0x8000) != 0 || (GetKeyState(VK_RSHIFT) & 0x8000) != 0;
        }
    }
}
