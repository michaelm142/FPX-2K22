using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class Scene : IGameComponent, IUpdateable
    {
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> ObjectInstanciated;

        private List<GameObject> spawnedObjects = new List<GameObject>();
        private List<GameObject> objects = new List<GameObject>();
        public IEnumerable<GameObject> Objects
        {
            get
            {
                foreach (var obj in objects)
                    yield return obj;
            }
        }

        public ContentManager content { get; set; }

        public bool Enabled { get; set; } = true;

        public bool IsLoaded { get; private set; }

        public int UpdateOrder { get; set; }

        public string sceneName { get; set; }

        public static Scene Active { get { return GameCore.currentLevel; } }

        public Scene()
        {
            ObjectInstanciated += Scene_ObjectInstanciated;
        }

        public void Initialize()
        {
        }

        private void Scene_ObjectInstanciated(object sender, EventArgs e)
        {
            GameObject obj = sender as GameObject;
            obj.BroadcastMessage("Awake");
            Debug.Log("Spawned object {0} in scene.", obj);
        }

        public GameObject Spawn(GameObject obj)
        {
            spawnedObjects.Add(obj);
            ObjectInstanciated(obj, new EventArgs());

            return obj;
        }

        public GameObject Spawn(params Type[] Components)
        {
            GameObject obj = new GameObject();
            foreach (var type in Components)
                obj.AddComponent(type);

            spawnedObjects.Add(obj);
            ObjectInstanciated(obj, new EventArgs());

            return obj;
        }

        public GameObject Spawn(Prefab prefab)
        {
            var obj = prefab.SpawnObject();
            spawnedObjects.Add(obj);
            ObjectInstanciated(obj, new EventArgs());
            return obj;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsLoaded)
                return;
            if (Camera.Active == null)
            {
                var camObj = objects.Find(o => o.GetComponent<Camera>() != null);
                if (camObj == null)
                {
                    Debug.LogError("Scene has no active camera");
                    return;
                }
                Camera.Active = camObj.GetComponent<Camera>();
                if (Camera.Active == null)
                {
                    Debug.LogError("Scene has no active camera");
                    return;
                }
            }
            foreach (var obj in objects)
            {
                if (obj.Enabled)
                    obj.Run(gameTime);
            }

            objects.AddRange(spawnedObjects);
            spawnedObjects.Clear();
        }


        public void Load()
        {
            Load(sceneName);
        }

        internal void BroadcastMessage(string Name, params object[] parameters)
        {
            foreach (var obj in objects.ToList().FindAll(o => o.KnowsMessage(Name)))
                obj.BroadcastMessage(Name, parameters);
        }

        public void Load(string filename)
        {
            XmlDocument doc = new XmlDocument();
            FileInfo file = new FileInfo(filename);
            if (!file.Exists)
            {
                Debug.LogError("Could not load scene {0}, because it is not in storage", filename);
                return;
            }
            // load cluster data into doc
            using (FileStream stream = file.Open(FileMode.Open))
            {
                // read in file data
                byte[] filebinary = new byte[stream.Length];
                stream.Read(filebinary, 0, filebinary.Length);

                string fileData = Encoding.UTF8.GetString(filebinary);
                // copy in each cluster
                while (fileData.IndexOf("<!--") != -1)
                {
                    int firstIndex = fileData.IndexOf("<!--") + 4;
                    int secondIndex = fileData.IndexOf("-->");

                    // get the filenane and parameters for this cluster
                    char[] clusterNameArray = new char[secondIndex - firstIndex];
                    fileData.CopyTo(firstIndex, clusterNameArray, 0, clusterNameArray.Length);
                    fileData = fileData.Remove(firstIndex - 4, secondIndex - firstIndex + 7);
                    string clusterInfo = new string(clusterNameArray);
                    string[] clusterPrams = clusterInfo.Split(new char[] { ' ' });
                    string clusterName = clusterPrams[0];

                    // load cluster file
                    using (StreamReader reader = new StreamReader(Environment.CurrentDirectory + "\\" + clusterName))
                    {
                        string clusterData = reader.ReadToEnd();
                        for (int i = 1; i < clusterPrams.Length; i++)
                        {
                            string pram = clusterPrams[i];
                            string[] pramData = pram.Split(new char[] { '=' });
                            string pramName = pramData[0];
                            string pramVal = pramData[1];

                            clusterData = clusterData.Replace(pramName, string.Format("({0})", pramVal));
                        }
                        // copy in data for this cluster
                        fileData = fileData.Insert(firstIndex, clusterData);
                    }
                }

                // Functor for processing math in clusters
                Func<string, char, int, int, string> ProcessMath = delegate (string s, char delim, int i, int ii)
                {
                    var args = s.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (args.Count != 2)
                        return fileData;

                    var argA = args[0].ToList();
                    var argB = args[1].ToList();

                    if (argA.Contains('+'))
                        argA.RemoveAt(argA.IndexOf('+'));
                    if (argB.Contains('+'))
                        argB.RemoveAt(argB.IndexOf('+'));

                    args[0] = new string(argA.ToArray());
                    args[1] = new string(argB.ToArray());

                    decimal a, b;
                    if (decimal.TryParse(args[0], out a) && decimal.TryParse(args[1], out b))
                    {
                        fileData = fileData.Remove(i, ii - i);
                        decimal value = a + b;
                        fileData = fileData.Insert(i, value.ToString());
                    }

                    return fileData;
                };

                // process any math left over from loading in clusters
                for (int i = 0; i < fileData.Length; i++)
                {

                    if (fileData[i] == '"')
                    {
                        int ii = ++i;
                        for (; ii < fileData.Length && fileData[ii] != '"'; ii++) { }
                        char[] valData = new char[ii - i];
                        fileData.CopyTo(i, valData, 0, valData.Length);

                        string val = new string(valData);
                        /*if ((val.Contains("+") && val.Contains("-")) || val.Cast<char>().ToList().FindAll(c => c == '-').Count > 1)
                        {
                            if (val.Cast<char>().ToList().FindAll(c => c == '-').Count > 1)
                            {
                                int cIndex = val.IndexOf('-');
                                if (cIndex == 0)
                                {

                                }
                            }
                            else
                            {

                            }
                        }
                        else*/
                        if (val.Contains('-'))
                            fileData = ProcessMath(val, '-', i, ii);
                        else if (val.Contains('+'))
                            fileData = ProcessMath(val, '+', i, ii);
                    }
                }

                // remove remaining parentheses
                var lFileData = fileData.ToList();
                lFileData.RemoveAll(c => c == '(' || c == ')');
                fileData = new string(lFileData.ToArray());

                // load processed data into xml doc
                using (MemoryStream memstream = new MemoryStream(Encoding.UTF8.GetBytes(fileData)))
                {
                    try
                    {
                        doc.Load(memstream);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Could not load scene because exception of type {0} was thrown. {1}", e.GetType(), e.Message);
                    }
                }
            }

            var scene = doc.SelectSingleNode("Scene");
            if (scene == null)
            {
                Debug.LogError("Could not load scene {0} because it does not contain scene content", sceneName);
                return;
            }
            var objectCollection = scene.FirstChild as XmlElement;

            // parse xml into game scene
            foreach (XmlElement node in objectCollection.ChildNodes.Cast<XmlElement>().ToList().FindAll(n => n.Name == "Object"))
            {
                var obj = GameObject.Load(node);
                objects.Add(obj);
                ObjectInstanciated(obj, new EventArgs());
            }
            foreach (XmlElement node in objectCollection.ChildNodes.Cast<XmlElement>().ToList().FindAll(n => n.Name == "Prefab"))
            {
                var fileNameAttr = node.Attributes["Filename"];
                var nameAttr = node.Attributes["Name"];
                Prefab prefab = Prefab.Load(fileNameAttr.Value);
                var obj = prefab.SpawnObject();
                if (nameAttr != null)
                    obj.Name = nameAttr.Value;
                foreach (XmlElement element in node.ChildNodes)
                {
                    Component comp = obj.GetComponents<Component>().Find(c => c.GetType().ToString().IndexOf(element.Name) != -1);
                    if (comp != null)
                        comp.LoadXml(element);
                    else
                    {
                        var createType = Utill.FindTypeFromAssemblies(element.Name);
                        comp = Activator.CreateInstance(createType) as Component;
                        comp.LoadXml(element);
                        obj.AddComponent(comp);
                    }
                }
                objects.Add(obj);
                ObjectInstanciated(obj, new EventArgs());
            }
            IsLoaded = true;

            Debug.Log("Sucessfully loaded {0} objects into scene", objects.Count);
        }

        public void Save(string filename)
        {
            XmlDocument doc = new XmlDocument();
            var sceneRoot = doc.CreateElement("Scene");
            doc.AppendChild(sceneRoot);
            var objectRoot = doc.CreateElement("Objects");
            sceneRoot.AppendChild(objectRoot);

            foreach (var obj in objects)
            {
                var element = doc.CreateElement("Object");
                var nameAttr = doc.CreateAttribute("Name");
                nameAttr.Value = obj.Name;
                element.Attributes.Append(nameAttr);
                foreach (var component in obj.Components)
                {
                    string[] typeInfo = component.GetType().ToString().Split(new char[] { '.' });
                    var componentNode = doc.CreateElement(typeInfo[typeInfo.Length - 1]);
                    element.AppendChild(componentNode);
                    if (component.KnowsMessage("SaveXml"))
                        component.SendMessage("SaveXml", componentNode);
                }
                objectRoot.AppendChild(element);
            }

            doc.Save(filename);
        }
    }
}
