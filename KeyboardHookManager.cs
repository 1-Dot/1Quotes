using System.Diagnostics;
using System.Runtime.InteropServices;

namespace _1Quotes
{
    /// <summary>
    /// 管理全局低级键盘钩子的注册与生命周期
    /// 当中文输入法处于 Native 模式且按下配置的按键时，拦截原按键并插入直角引号
    /// </summary>
    internal sealed class KeyboardHookManager : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero; // 已安装钩子的句柄
        private readonly NativeMethods.LowLevelKeyboardProc _proc; // 委托保持引用防止被 GC 回收

        public Func<InputTriggerMode> GetTriggerMode { get; set; } = () => InputTriggerMode.ShiftBracket;

        public KeyboardHookManager()
        {
            _proc = HookCallback;
            _hookId = SetHook(_proc);
        }

        /// <summary>
        /// 为当前进程安装低级键盘钩子
        /// </summary>
        private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            return NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                proc,
                NativeMethods.GetModuleHandle(curModule.ModuleName),
                0);
        }

        /// <summary>
        /// 钩子回调——在应用/系统处理之前拦截按键
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= NativeMethods.HC_ACTION &&
                (wParam == NativeMethods.WM_KEYDOWN || wParam == NativeMethods.WM_SYSKEYDOWN))
            {
                var hookStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                uint vkCode = hookStruct.vkCode;

                if (IME.IsChineseImeActive())
                {
                    var mode = GetTriggerMode();
                    bool shiftPressed = NativeMethods.IsShiftPressed();

                    if (vkCode == NativeMethods.VK_OEM_4) // [
                    {
                        if ((mode == InputTriggerMode.ShiftBracket && shiftPressed) || (mode == InputTriggerMode.BracketOnly && !shiftPressed))
                        {
                            Debug.WriteLine("拦截 [ 发送「");
                            InputSimulator.InsertUnicodeText("「");
                            return 1;
                        }
                    }
                    if (vkCode == NativeMethods.VK_OEM_6) // ]
                    {
                        if ((mode == InputTriggerMode.ShiftBracket && shiftPressed) || (mode == InputTriggerMode.BracketOnly && !shiftPressed))
                        {
                            Debug.WriteLine("拦截 ] 发送」");
                            InputSimulator.InsertUnicodeText("」");
                            return 1;
                        }
                    }
                }
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// 卸载钩子
        /// </summary>
        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
    }
}
