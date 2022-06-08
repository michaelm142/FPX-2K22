using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using Diagnostics = System.Diagnostics.Debug;

namespace FPX
{
    public static class Debug
    {
        public static ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }

            set { Console.ForegroundColor = value; }
        }

        public static ConsoleColor BackgroundColor
        {
            get { return Console.BackgroundColor; }

            set { Console.BackgroundColor = value; }
        }

        public static void ResetColors()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        static StringWriter writer = new StringWriter();

        public static void Log(object message)
        {
            Log(message.ToString());
        }

        public static void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
            Diagnostics.WriteLine(message, args);
            writer.WriteLine(message, args);
        }

        public static void LogWarning(string warning, params object[] args)
        {
            ForegroundColor = ConsoleColor.Black;
            BackgroundColor = ConsoleColor.DarkYellow;
            Log(warning, ConsoleColor.Yellow, args);
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }

        public static void LogError(string error, params object[] args)
        {
            BackgroundColor = ConsoleColor.Red;
            ForegroundColor = ConsoleColor.White;
            Log(error, args);
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.Gray;
        }

        public static void ClearConsole()
        {
            Console.Clear();
        }

        public static void DumpLog(string filename = "Log.txt")
        {
            if (writer == null)
                return;

            FileInfo logFile = new FileInfo(Environment.CurrentDirectory + "\\" + filename);
            if (!logFile.Exists)
                logFile.Create();

            var sb = writer.GetStringBuilder();
            writer.Close();
            writer = null;

            using (StreamWriter writer = new StreamWriter(logFile.Open(FileMode.Append, FileAccess.Write)))
            {
                var date = DateTime.Now.Date;
                var time = DateTime.Now.TimeOfDay;
                writer.WriteLine(); writer.WriteLine();
                writer.WriteLine("=================================== [{0}/{1}/{2} - {3}:{4}:{5}]===================================", date.Day, date.Month, date.Year, time.Hours % 12, time.Minutes, time.Seconds);
                writer.WriteLine(sb);
            }
            writer = new StringWriter();

            Log("Output log to {0}", logFile.FullName);
        }
    }
}
