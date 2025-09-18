using System.Runtime.InteropServices;
using static _1Quotes.NativeMethods;

namespace _1Quotes
{
    internal static class InputSimulator
    {
        public static int InsertUnicodeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }
            INPUT[] inputs = new INPUT[text.Length * 2];
            for (int i = 0, j = 0; i < text.Length; i++, j += 2)
            {
                ushort ch = text[i];
                inputs[j] = new INPUT
                {
                    type = 1,
                    mkhi = new MOUSEKEYBDHARDWAREINPUT
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = ch,
                            dwFlags = 0x0004,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                inputs[j + 1] = new INPUT
                {
                    type = 1,
                    mkhi = new MOUSEKEYBDHARDWAREINPUT
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = ch,
                            dwFlags = 0x0004 | 0x0002,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
            }
            uint result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            return result > 0 ? text.Length : 0;
        }
    }
}
