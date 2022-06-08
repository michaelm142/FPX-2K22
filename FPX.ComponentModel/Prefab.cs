using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;

namespace FPX.ComponentModel
{
    public class Prefab
    {
        string filename;

        FileInfo file;

        Prefab(string filename)
        {
            this.filename = filename;
            file = new FileInfo(Environment.CurrentDirectory + "\\" + filename);
            if (!file.Exists)
                throw new FileNotFoundException();

        }

        public GameObject SpawnObject()
        {
            XmlDocument doc = new XmlDocument();
            using (FileStream stream = file.Open(FileMode.Open))
            {
                doc.Load(stream);
                var objNode = doc.FirstChild.NextSibling as XmlElement;
                return GameObject.Load(objNode);
            }
        }

        static Dictionary<string, Prefab> prefabPool = new Dictionary<string, Prefab>();

        public static Prefab Load(string filename)
        {
            if (prefabPool.ContainsKey(filename))
                return prefabPool[filename];

            Prefab p = new Prefab(filename);
            prefabPool.Add(filename, p);

            return p;
        }
    }
}
