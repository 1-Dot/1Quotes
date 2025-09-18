using System.Runtime.InteropServices;

namespace _1Quotes
{
    /// <summary>
    /// ԭ����������P/Invoke�������ࣺ�����ͼ����̹��Ӽ�����ģ������ĳ������ṹ�뺯����
    /// </summary>
    internal static class NativeMethods
    {
        // ��������Ϣ���� ----------------------------------------------------
        internal const int WH_KEYBOARD_LL = 13;      // �ͼ����̹��� ID
        internal const int WM_KEYDOWN = 0x0100;      // ����������Ϣ
        internal const int WM_SYSKEYDOWN = 0x0104;   // ϵͳ������Alt �ȣ�������Ϣ
        internal const int HC_ACTION = 0;            // ��Ч�Ĺ��Ӷ�������

        // ������� ------------------------------------------------------------
        internal const int VK_LSHIFT = 0xA0; // �� Shift
        internal const int VK_RSHIFT = 0xA1; // �� Shift
        internal const int VK_OEM_4 = 0xDB;  // '[' ��
        internal const int VK_OEM_6 = 0xDD;  // ']' ��

        // SendInput ��־λ ----------------------------------------------------
        internal const uint INPUT_KEYBOARD = 1;      // ������������
        internal const uint KEYEVENTF_KEYUP = 0x0002;     // ����̧��
        internal const uint KEYEVENTF_UNICODE = 0x0004;   // ���� Unicode �ַ�

        // ί�� / �ṹ�� -------------------------------------------------------
        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;      // �������
            public uint scanCode;    // ɨ����
            public uint flags;       // ��־λ
            public uint time;        // ʱ���
            public IntPtr dwExtraInfo; // ������Ϣ
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint type;                        // ��������
            public MOUSEKEYBDHARDWAREINPUT mkhi;     // �����壨���/����/Ӳ����
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;   // ��������
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)] public HARDWAREINPUT hi; // Ӳ������
            [FieldOffset(0)] public KEYBDINPUT ki;    // ��������
            [FieldOffset(0)] public MOUSEINPUT mi;    // �������
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint uMsg;        // ��Ϣ
            public ushort wParamL;   // ������λ
            public ushort wParamH;   // ������λ
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;           // ��� X λ��
            public int dy;           // ��� Y λ��
            public uint mouseData;   // ��긽������
            public uint dwFlags;     // ����־
            public uint time;        // ʱ���
            public IntPtr dwExtraInfo; // ������Ϣ
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort wVk;       // �������
            public ushort wScan;     // Ӳ��ɨ����
            public uint dwFlags;     // ��־λ
            public uint time;        // ʱ���
            public IntPtr dwExtraInfo; // ������Ϣ
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

        [DllImport("user32.dll")] internal static extern short GetKeyState(int nVirtKey); // ��ȡ����״̬

        [DllImport("user32.dll")] internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize); // ����ģ������

        /// <summary>
        /// �ж����� Shift ���Ƿ񱻰��¡�
        /// </summary>
        internal static bool IsShiftPressed()
        {
            return (GetKeyState(VK_LSHIFT) & 0x8000) != 0 || (GetKeyState(VK_RSHIFT) & 0x8000) != 0;
        }
    }
}
