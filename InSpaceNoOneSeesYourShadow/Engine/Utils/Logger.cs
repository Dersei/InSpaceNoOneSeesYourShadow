using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace InSpaceNoOneSeesYourShadow.Engine.Utils
{
    public class Logger
    {
        public static string LogFileName = "info.txt";

        private static string CreateMessage(string message, string memberName)
        {
            return $"{DateTime.Now:MM/dd/yyyy HH:mm:ss} - {memberName} - {message}";
        }

        /// <summary>
        /// Write a message to a log file
        /// </summary>
        /// <param name="message">a message that will append to a log file</param>
        [Conditional("DEBUG")]
        public static void LogFile(string message, [CallerMemberName] string memberName = "")
        {
            File.AppendAllText(LogFileName, CreateMessage(message, memberName) + Environment.NewLine);
        }

        [Conditional("DEBUG")]
        public static void LogConsole(string message, [CallerMemberName] string memberName = "")
        {
            Console.WriteLine(CreateMessage(message, memberName));
        }

        [Conditional("DEBUG")]
        public static void LogToAll(string message, [CallerMemberName] string memberName = "")
        {
            LogFile(message, memberName);
            LogConsole(message, memberName);
        }
    }
}
