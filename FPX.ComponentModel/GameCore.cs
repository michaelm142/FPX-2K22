using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FPX.ComponentModel;
using FPX.Visual;

namespace FPX
{
    public class GameCore
    {
        public static GameCore Instance { get; private set; }

        public static Game gameInstance { get; private set; }

        public static SpriteBatch spriteBatch { get; set; }

        public static bool IsRunning { get; private set; }

        public static Scene currentLevel
        {
            get { return gameInstance.Components.ToList().Find(c => c is Scene) as Scene; }
        }

        public static GraphicsDevice graphicsDevice
        {
            get { return gameInstance.GraphicsDevice; }
        }

        public static ContentManager content
        {
            get { return gameInstance.Content; }
        }

        public static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

        public static Viewport viewport
        {
            get { return graphicsDevice.Viewport; }
        }

        public static Graphics Graphics
        {
            get { return gameInstance.Components.ToList().Find(c => c is Graphics) as Graphics; }
        }

        public static Game CreateGameInstance(IntPtr? windowHandle = null)
        {
            var gametype = Assembly.GetEntryAssembly().GetTypes().ToList().Find(t => t.BaseType.FullName == "Microsoft.Xna.Framework.Game");
            ConstructorInfo gameConstructor = null;
            if (windowHandle != null)
                gameConstructor = gametype.GetConstructors().ToList().Find(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(IntPtr));
            if (gameConstructor == null)
            {
                var instance = Activator.CreateInstance(gametype);
                gameInstance = (Game)instance;
            }
            else
                gameInstance = Activator.CreateInstance(gametype, windowHandle) as Game;

            PropertyInfo instanceProperty = gametype.GetProperties().ToList().Find(p => p.Name == "Instance");
            if (instanceProperty == null)
                throw new InvalidOperationException("Game instance must have a static 'Instance' property.");
            instanceProperty.GetSetMethod().Invoke(gameInstance, new object[] { gameInstance });

            gameInstance.Exiting += GameInstance_Exiting;

            //gameInstance.Components.Add(new Time());
            //gameInstance.Components.Add(new Physics());
            //gameInstance.Components.Add(new Graphics());
            //gameInstance.Components.Add(new Input());

            gameInstance.Activated += Outval_Activated;

            return gameInstance;
        }

        private static void Outval_Activated(object sender, EventArgs e)
        {
            PropertyInfo spriteBatchProperty = gameInstance.GetType().GetProperties().ToList().Find(p => p.PropertyType == typeof(SpriteBatch));
            if (spriteBatchProperty == null)
                Debug.LogWarning("Game instance does not expose spriteBatch property");
            spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatchProperty.GetSetMethod().Invoke(gameInstance, new object[] { spriteBatch });
        }

        private static void GameInstance_Exiting(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        [STAThread]
        public static void Run(IntPtr? windowHandle = null)
        {
            Debug.BackgroundColor = ConsoleColor.Red;
            Debug.Log("ENGINE LAUNCH");
            Debug.ResetColors();

            if (windowHandle == null)
            {
                GameForm form = new GameForm();
                form.Width = Settings.GetSetting<int>("ScreenWidth");
                form.Height = Settings.GetSetting<int>("ScreenHeight");
                form.Show();
                windowHandle = form.Handle;
            }


            // Thread t = new Thread(new ParameterizedThreadStart(GameLoop));
            // t.Start(windowHandle);
            WaitCallback callback = new WaitCallback(GameLoop);
            ThreadPool.QueueUserWorkItem(callback, windowHandle);
        }

        public static void LoadScene(string sceneName)
        {
            if (!IsRunning || gameInstance == null)
            {
                Debug.LogError("Engine is not currently running");
                return;
            }

            Scene scene = new Scene();
            scene.sceneName = sceneName;
            scene.Initialize();
            scene.Load();

            gameInstance.Components.Add(scene);
        }

        private static void GameLoop(object param)
        {
            IntPtr windowHandle = param == null ? IntPtr.Zero : (IntPtr)param;

            IsRunning = true;
            using (Game gameInstance = CreateGameInstance(windowHandle))
            {
                while (IsRunning)
                    gameInstance.RunOneFrame();
            }

            Camera.Active = null;
            Settings.ShutDown();
            Debug.DumpLog();

            Debug.BackgroundColor = ConsoleColor.DarkGray;
            Debug.ForegroundColor = ConsoleColor.Black;
            Debug.Log("Engine Shutdown");
            Debug.ResetColors();
        }
    }
}
