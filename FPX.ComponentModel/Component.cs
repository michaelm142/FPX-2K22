using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FPX.Editor;

namespace FPX.ComponentModel
{
    public abstract class Component
    {
        public GameObject gameObject;

        public Transform transform
        {
            get { return GetComponent<Transform>(); }
        }

        [IgnoreInGUI]
        public Vector3 position
        {
            get { return transform.position; }

            set { transform.position = value; }
        }

        [IgnoreInGUI]
        public Vector3 scale
        {
            get { return transform.localScale; }

            set { transform.localScale = value; }
        }

        [IgnoreInGUI]
        public Vector3 localPosition
        {
            get { return transform.localPosition; }

            set { transform.localPosition = value; }
        }

        [IgnoreInGUI]
        public Quaternion rotation
        {
            get { return transform.rotation; }

            set { transform.rotation = value; }
        }

        [IgnoreInGUI]
        public Quaternion localRotation
        {
            get { return GetComponent<Transform>().localRotation; }

            set { GetComponent<Transform>().localRotation = value; }
        }

        internal static List<Component> g_collection = new List<Component>();

        public uint Id { get; protected set; }

        public string Name
        {
            get { return gameObject.Name; }
        }

        public Component()
        {
            g_collection.Add(this);
            Id = (uint)GetHashCode();
        }

        public T GetComponent<T>()
            where T : Component
        {
            return gameObject.GetComponent<T>();
        }


        public List<T> GetComponents<T>()
            where T : Component
        {
            return gameObject.GetComponents<T>();
        }

        public bool KnowsMessage(string message)
        {
            return GetType().GetMethods().ToList().Find(x => x.Name == message) != null;
        }

        public void SendMessage(string message, params object[] parameters)
        {
            if (message == "SendMessage")
                return;

            var method = GetType().GetMethods().ToList().Find(x => x.Name == message);
            if (method != null)
            {
                try
                {
                    method.Invoke(this, parameters);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    Exception innerException = e;
                    while (innerException is System.Reflection.TargetInvocationException && innerException != null)
                        innerException = e.InnerException;

                    Debug.LogError("Sending message raised exception of type {0}. {1} \nStack Trace:\n{2}", innerException.GetType(), innerException.Message, innerException.StackTrace);
                }
            }
        }

        public virtual void LoadXml(XmlElement element)
        {
            var type = GetType();
            var fields = type.GetFields().ToList();
            var properties = type.GetProperties().ToList();

            foreach (var field in fields)
            {
                XmlElement memberXmlElement = element.SelectSingleNode(field.Name) as XmlElement;
                if (memberXmlElement == null)
                    continue;

                string memberData = memberXmlElement.InnerText;
                object fieldData = null;

                if (field.DeclaringType == typeof(string))
                    fieldData = memberData;
                else if (field.DeclaringType == typeof(int))
                {
                    int intVal = 0;
                    if (!int.TryParse(memberData, out intVal))
                    {
                        Debug.LogError("Failed to parse data for {0} of {1}", field.Name, type);
                        continue;
                    }

                    fieldData = intVal;
                }
                else if (field.DeclaringType == typeof(float))
                {
                    float floatVal = 0.0f;
                    if (!float.TryParse(memberData, out floatVal))
                    {
                        Debug.LogError("Failed to parse data for {0} of {1}", field.Name, type);
                        continue;
                    }
                }
                else if (field.DeclaringType == typeof(Vector3))
                {
                    Vector3 vector3Value = LinearAlgebraUtil.Vector3FromXml(memberXmlElement);
                    fieldData = vector3Value;
                }
                else if (field.DeclaringType == typeof(Quaternion))
                {
                    Quaternion quaternionValue = LinearAlgebraUtil.EulerFromXml(memberXmlElement);
                    fieldData = quaternionValue;
                }
                else if (field.DeclaringType == typeof(Color))
                {
                    Color colorValue = LinearAlgebraUtil.ColorFromXml(memberXmlElement);
                    fieldData = colorValue;
                }
                else if (field.DeclaringType == typeof(Texture2D))
                {
                    var filenameAttr = memberXmlElement.Attributes["FileName"];
                    Texture2D texVal = GameCore.content.Load<Texture2D>(filenameAttr.InnerText);
                    fieldData = texVal;
                }

                field.SetValue(this, fieldData);
            }

            foreach (var property in properties)
            {
                string propertyName = property.Name;
                XmlElement memberXmlElement = element.SelectSingleNode(propertyName) as XmlElement;
                if (memberXmlElement == null)
                {
                    char firstChar = propertyName[0];
                    propertyName = propertyName.Remove(0, 1);
                    propertyName = propertyName.Insert(0, char.ToUpper(firstChar).ToString());
                    memberXmlElement = element.SelectSingleNode(propertyName) as XmlElement;
                    if (memberXmlElement == null)
                        continue;
                }

                string memberData = memberXmlElement.InnerText;
                object propertyData = null;

                if (property.PropertyType == typeof(string))
                    propertyData = memberData;
                else if (property.PropertyType == typeof(int))
                {
                    int intVal = 0;
                    if (!int.TryParse(memberData, out intVal))
                    {
                        Debug.LogError("Failed to parse data for {0} of {1}", property.Name, type);
                        continue;
                    }

                    propertyData = intVal;
                }
                else if (property.PropertyType == typeof(float))
                {
                    float floatVal = 0.0f;
                    if (!float.TryParse(memberData, out floatVal))
                    {
                        Debug.LogError("Failed to parse data for {0} of {1}", property.Name, type);
                        continue;
                    }
                }
                else if (property.PropertyType == typeof(Vector3))
                {
                    Vector3 vector3Value = LinearAlgebraUtil.Vector3FromXml(memberXmlElement);
                    propertyData = vector3Value;
                }
                else if (property.PropertyType == typeof(Quaternion))
                {
                    Quaternion quaternionValue = LinearAlgebraUtil.EulerFromXml(memberXmlElement);
                    propertyData = quaternionValue;
                }
                else if (property.PropertyType == typeof(Color))
                {
                    Color colorValue = LinearAlgebraUtil.ColorFromXml(memberXmlElement);
                    propertyData = colorValue;
                }
                else if (property.PropertyType == typeof(Texture2D))
                {
                    Texture2D texVal = GameCore.content.Load<Texture2D>(memberXmlElement.InnerText);
                    propertyData = texVal;
                }

                if (propertyData != null)
                    property.SetValue(this, propertyData);
            }
        }


        public List<T> FindObjectsOfType<T>()
            where T : Component
        {
            return g_collection.FindAll(o => o is T).Cast<T>().ToList();
        }

        public T FindObjectOfType<T>()
            where T : Component
        {
            return g_collection.Find(o => o is T) as T;
        }

        public static GameObject Instanciate(GameObject @object)
        {
            return GameCore.currentLevel.Spawn(@object);
        }

        public static GameObject Instantiate(Prefab prefab)
        {
            return GameCore.currentLevel.Spawn(prefab);
        }

        public override string ToString()
        {
            return string.Format("{0} of {1}", GetType().ToString(), gameObject.Name);
        }

        private bool _firstFrame = true;

        internal  void Run()
        {
            if (_firstFrame && KnowsMessage("Start"))
            {
                SendMessage("Start");
                _firstFrame = false;
            }

            if (KnowsMessage("Update"))
                SendMessage("Update", Time.GameTime);
        }
    }
}
