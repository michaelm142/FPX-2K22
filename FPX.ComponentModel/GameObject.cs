using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FPX.ComponentModel
{
    public sealed class GameObject
    {
        private bool firstFrame = true;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        private List<Component> components = new List<Component>();
        public IEnumerable<Component> Components
        {
            get { return components.AsEnumerable(); }
        }

        public GraphicsDevice graphics;

        public int DrawOrder { get; set; }

        public bool Enabled { get; set; } = true;

        public int UpdateOrder { get; set; }

        public bool Visible { get; set; } = true;

        public string Name { get; set; } = "GameObject";

        public Transform transform
        {
            get { return GetComponent<Transform>(); }
        }

        public Vector3 localPosition
        {
            get { return transform.localPosition; }

            set { transform.localPosition = value; }
        }

        public Vector3 position
        {
            get { return transform.position; }

            set { transform.position = value; }
        }

        public Quaternion localRotation
        {
            get { return transform.localRotation; }

            set { transform.localRotation = value; }
        }

        public Quaternion rotation
        {
            get { return transform.rotation; }

            set { transform.rotation = value; }
        }

        internal uint Id { get; private set; }

        private static List<GameObject> _instances = new List<GameObject>();

        private GameObject(bool isEmpty)
        {
            _instances.Add(this);
            Id = (uint)GetHashCode();
        }

        public GameObject()
        {
            _instances.Add(this);
            AddComponent<Transform>();
            Id = (uint)GetHashCode();
        }

        ~GameObject()
        {
            _instances.Remove(this);
        }

        public bool KnowsMessage(string message)
        {
            return components.Find(c => c.KnowsMessage(message)) != null;
        }

        public void BroadcastMessage(string Name, params object[] parameters)
        {
            foreach (var comp in components.FindAll(c =>c.KnowsMessage(Name)))
                comp.SendMessage(Name, parameters);
        }

        public void Run(GameTime gameTime)
        {
            for (int i = 0; i < components.Count; i++)
                components[i].Run();
        }

        public void Draw(GameTime gameTime)
        {
            BroadcastMessage("Draw", new object[] { gameTime });
        }

        public T AddComponent<T>()
            where T : Component
        {
            Component comp = Activator.CreateInstance<T>();
            comp.gameObject = this;
            components.Add(comp);

            return comp as T;
        }

        public void AddComponent(Component c)
        {
            c.gameObject = this;
            components.Add(c);
        }

        public object AddComponent(Type t)
        {
            Component comp = Activator.CreateInstance(t) as Component;
            comp.gameObject = this;
            components.Add(comp);

            return comp;
        }

        public T GetComponent<T>()
             where T : Component
        {
            return components.Find(c => c is T) as T;
        }
        public List<T> GetComponents<T>()
            where T : Component
        {
            return components.FindAll(c => c is T) as List<T>;
        }

        public override string ToString()
        {
            return Name;
        }

        public static GameObject Empty
        {
            get { return new GameObject(true); }
        }

        public static GameObject Find(string name)
        {
            return _instances.Find(g => g.Name == name);
        }

        public static GameObject Find(uint Id)
        {
            return _instances.Find(g => g.Id == Id);
        }

        public static GameObject Load(XmlElement node)
        {
            GameObject obj = Empty;
            var nameAttr = node.Attributes["Name"];
            if (nameAttr != null)
                obj.Name = nameAttr.Value;

            var enabledAttribute = node.Attributes["Enabled"];
            if (enabledAttribute != null)
                obj.Enabled = bool.Parse(enabledAttribute.InnerText);
            var idAttr = node.Attributes["Id"];
            if (idAttr != null)
                obj.Id = uint.Parse(idAttr.Value);

            foreach (XmlElement componentNode in node.ChildNodes)
            {
                var createType = Utill.FindTypeFromAssemblies(componentNode.Name);
                Component c = Activator.CreateInstance(createType) as Component;
                if (c == null)
                {
                    Debug.LogError("Could not find type {0} in assembly", createType);
                    continue;
                }
                c.gameObject = obj;
                c.LoadXml(componentNode);
                obj.AddComponent(c);
            }

            var transform = obj.GetComponent<Transform>();
            if (transform == null)
                obj.AddComponent<Transform>();

            return obj;
        }

        public static void GlobalBroadcastMessage(string message, params object[] prams)
        {
            GameCore.currentLevel.BroadcastMessage(message, prams);
        }
        public static T FindObjectOfType<T>()
            where T : Component
        {
            return Component.g_collection.Find(c => c is T) as T;
        }

        public static List<T> FindObjectsOfType<T>()
            where T : Component
        {
            return Component.g_collection.FindAll(c => c is T).Cast<T>().ToList();
        }
    }
}
