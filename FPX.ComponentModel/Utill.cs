using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FPX.Visual;

namespace FPX
{
    public static class Utill
    {

        public static Type FindTypeFromAssemblies(string typename)
        {
            var localType = Assembly.GetExecutingAssembly().GetTypes().ToList().Find(x => x.Name == typename);
            if (localType != null)
                return localType;

            foreach (var file in new DirectoryInfo(Environment.CurrentDirectory).GetFiles())
            {
                if (file.Extension != ".dll")
                    continue;

                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFile(file.FullName);
                }
                catch (BadImageFormatException)
                {
                    continue;
                }

                if (assembly == null)
                    continue;

                Type type = assembly.GetTypes().ToList().Find(t => t.Name == typename);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static Rectangle RectFromXml(XmlElement node)
        {
            var xVal = node.Attributes["X"];
            var yVal = node.Attributes["Y"];
            var widthVal = node.Attributes["Width"];
            var heightVal = node.Attributes["Height"];

            int x = int.Parse(xVal.InnerText);
            int y = int.Parse(yVal.InnerText);
            int width = int.Parse(widthVal.InnerText);
            int height = int.Parse(heightVal.InnerText);

            return new Rectangle(x, y, width, height);
        }

        public static void LoadXml(this Rectangle r, XmlElement node)
        {
            r = RectFromXml(node);
        }

        public static Scene GetCurrentLevel(this Game game)
        {
            return game.Components.ToList().Find(c => c is Scene) as Scene;
        }

        public static void MakeDefault(this Texture2D texture2D)
        {
            Color[] data = new Color[texture2D.Width * texture2D.Height];
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;
            texture2D.SetData(data);
        }
    }
}
