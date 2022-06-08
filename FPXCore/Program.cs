using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;
using System.Reflection;
using System.Windows;
using FPX.ComponentModel;

using Application = System.Windows.Forms.Application;

namespace FPX
{
#if WINDOWS || XBOX
    static class Program
    {
        static bool Running = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            Title();
            Settings.Initialize();
            uint frame = 0;
            while (Running)
            {
                Debug.Log(DateTime.Now);

                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("Engine Core Terminal");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(">");
                if (args.Length >= 2 && frame == 0)
                {
                    args.ToList().ForEach(a => Console.Write(a + " "));
                    Console.WriteLine();
                    var pragma = new string[args.Length - 1];
                    for (int i = 1; i < args.Length; i++)
                        pragma[i - 1] = args[i];

                    string command = args[0];

                    ParseCommand(command, pragma);
                }
                else
                {
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] pragma = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string command = pragma[0];
                    var arguments = new string[pragma.Length - 1];
                    for (int i = 0; i < arguments.Length; i++)
                        arguments[i] = pragma[i + 1];

                    ParseCommand(command, arguments);
                }
                frame++;
            }
            Settings.ShutDown();
            Debug.DumpLog();

        }

        static void ParseCommand(string command, string[] pragma)
        {
            var methods = typeof(Program).GetMethods().ToList().FindAll(m => m.Name == command);
            var method = methods.Find(m => m.GetParameters().Length == pragma.Length);
            if (method == null)
            {
                Console.WriteLine("Unknown function: {0}", command);
                return;
            }
            int intPram = 0;
            float floatPram = 0.0f;
            object[] prams = new object[pragma.Length];
            for (int i = 0; i < pragma.Length; i++)
            {
                if (int.TryParse(pragma[i], out intPram))
                    prams[i] = intPram;
                else if (float.TryParse(pragma[i], out floatPram))
                    prams[i] = floatPram;
                else
                    prams[i] = pragma[i];
            }

            try
            {
                method.Invoke(null, prams);
            }
            catch (TargetInvocationException e)
            {
                Exception ex = getInnerException(e);
                Debug.LogError("Exception of type {0} was thrown. {1}", ex.GetType(), ex.Message);
                Debug.LogError(ex.StackTrace);
            }
        }

        static void Title()
        {
            Debug.Log("=================================");
            Debug.Log("  ********* *********  ***    ***");
            Debug.Log(" ********** **      **  ***  *** ");
            Debug.Log(" ****       **     ***   ******  ");
            Debug.Log(" *******    ********      *****  ");
            Debug.Log(" *******    **           *** *** ");
            Debug.Log(" ****       **         ****   ***");
            Debug.Log("=================================");
        }

        static Exception getInnerException(TargetInvocationException e)
        {
            if (e.InnerException is TargetInvocationException)
                return getInnerException(e.InnerException as TargetInvocationException);

            return e.InnerException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            Debug.LogError("Exception of type {0} was thrown", ex.GetType());
            Debug.LogError(ex.StackTrace);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {

            Debug.LogError("Exception of type {0} was thrown. Additional information: {1}", e.Exception.GetType(), e.Exception.Message);
            Debug.LogError(e.Exception.StackTrace);
        }

        public static void Run()
        {
            GameCore.Run();
        }

        public static void LoadScene(string sceneName)
        {
            GameCore.LoadScene(sceneName);
        }

        public static void Editor()
        {
            Debug.BackgroundColor = ConsoleColor.Blue;
            Debug.Log("EDITOR");
            Debug.ResetColors();

            Application.EnableVisualStyles();
            try
            {
                Application.SetCompatibleTextRenderingDefault(true);
            }
            catch (InvalidOperationException) { }

            Application.Run(new GameForm());
        }

        //public static void Editor(string sceneName)
        //{
        //    try
        //    {
        //        Application.EnableVisualStyles();
        //        Application.SetCompatibleTextRenderingDefault(true);
        //    }
        //    catch (Exception) { }

        //    Application.Run(new FPXCore.MicrosoftUIWindow(sceneName));
        //}

        public static void Close()
        {
            Running = false;
        }

        //public static void SetRendererMode(string Mode)
        //{
        //    Graphics.Mode = Mode;
        //    Debug.Log("Set mode to {0}", Mode);
        //}

        public static void SetStringSetting(string name, string value)
        {
            Settings.SetSetting(name, value);
            Debug.Log("Set setting {0} to {1}", name, value);
        }

        public static void SetFloatSetting(string name, float value)
        {
            Settings.SetSetting(name, value);
            Debug.Log("Set setting {0} to {1}", name, value);
        }

        public static void SetIntSetting(string name, int value)
        {
            Settings.SetSetting(name, value);
            Debug.Log("Set setting {0} to {1}", name, value);
        }

        public static void ShowSettings()
        {
            foreach (var v in Settings.Collection)
                Debug.Log(v.Key + ": " + v.Value.ToString());
        }

        public static void Help()
        {
            Debug.Log("List of function names:");
            foreach (var method in typeof(Program).GetMethods())
            {
                if (method.Name != "Main" && typeof(object).GetMethods().ToList().Find(m => m.Name == method.Name) == null)
                    Debug.Log(method.Name);
            }
        }
    }
#endif
}

