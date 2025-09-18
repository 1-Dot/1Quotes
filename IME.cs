using System.Diagnostics;
using System.Runtime.InteropServices;

namespace _1Quotes
{
    /// <summary>
    /// 与当前 IME 状态相关的查询工具
    /// 用于检测当前焦点控件是否处于中文输入法的 Native 模式
    /// </summary>
    internal static partial class IME
    {
        [LibraryImport("user32.dll", SetLastError = true)] internal static partial IntPtr GetForegroundWindow(); // 获取前台窗口句柄
        [LibraryImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)] internal static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam); // 发送消息（宽字节版）
        [LibraryImport("imm32.dll", SetLastError = true)] internal static partial IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd); // 获取默认 IME 窗口句柄

        internal const uint WM_IME_CONTROL = 0x283;      // IME 控制消息
        internal const int IMC_GETOPENSTATUS = 5;        // 获取 IME 是否打开
        internal const int IMC_GETCONVERSIONMODE = 1;    // 获取转换模式
        internal const int IME_CMODE_NATIVE = 1;         // 本机/中文模式标志位

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public int cbSize;        // 结构体大小
            public int flags;         // 标志
            public IntPtr hwndActive; // 当前激活窗口
            public IntPtr hwndFocus;  // 当前焦点控件
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;  // 插入符窗口
            public RECT rcCaret;      // 插入符矩形
        }

        [DllImport("user32.dll", SetLastError = true)] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId); // 获取窗口线程 ID
        [DllImport("user32.dll", SetLastError = true)] private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);     // 获取 GUI 线程信息

        /// <summary>
        /// 检测当前焦点窗口是否使用中文输入法并处于 Native 模式
        /// </summary>
        internal static bool IsChineseImeActive()
        {
            try
            {
                // 获取前台窗口及其线程
                IntPtr fg_hwnd = GetForegroundWindow();
                uint tid = GetWindowThreadProcessId(fg_hwnd, out uint _);

                // 获取该线程的 GUI 信息，从而获得焦点控件句柄
                var gti = new GUITHREADINFO { cbSize = Marshal.SizeOf(typeof(GUITHREADINFO)) };
                GetGUIThreadInfo(tid, ref gti);
                var hwnd = gti.hwndFocus;

                Debug.WriteLine($"前台窗口句柄 (hwnd): 0x{hwnd.ToInt64():X}");
                if (hwnd == IntPtr.Zero)
                {
                    Debug.WriteLine($"GetForegroundWindow 失败。Win32 错误码: {Marshal.GetLastPInvokeError()}");
                    return false;
                }

                // 获取 IME 窗口
                var imeWnd = ImmGetDefaultIMEWnd(hwnd);
                Debug.WriteLine($"IME 窗口句柄 (imeWnd): 0x{imeWnd.ToInt64():X}");
                if (imeWnd == IntPtr.Zero)
                {
                    Debug.WriteLine("ImmGetDefaultIMEWnd 返回 0 —— 可能没有关联 IME。");
                    return false;
                }

                // IME 打开状态
                int isOpen = SendMessage(imeWnd, WM_IME_CONTROL, (IntPtr)IMC_GETOPENSTATUS, IntPtr.Zero).ToInt32();
                Debug.WriteLine($"IME 打开状态: {isOpen}");
                if (isOpen == 0) return false;

                // 转换模式 & 中文模式标志
                int conversionMode = SendMessage(imeWnd, WM_IME_CONTROL, (IntPtr)IMC_GETCONVERSIONMODE, IntPtr.Zero).ToInt32();
                Debug.WriteLine($"IME 转换模式: {conversionMode}");

                bool isNative = (conversionMode & IME_CMODE_NATIVE) != 0;
                Debug.WriteLine($"是否 Native 模式: {isNative}\n--------------------");
                return isNative;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[错误] IsChineseImeActive: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                return false;
            }
        }
    }
}
