using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class Sprite : Component, IDrawable
    {
        public Texture2D image
        {
            get { return GetComponent<Material>().DiffuseMap; }

            set { GetComponent<Material>().DiffuseMap = value; }
        }

        public float angle
        {
            get { return transform.rotation.GetEulerAngles().Z; }

            set
            {
                Vector3 euler = rotation.GetEulerAngles();
                euler.Z = value;
                rotation.SetEulerAngles(euler);
            }
        }

        public float size
        {
            get { return scale.LengthSquared() / 3; }

            set { scale = Vector3.One * value; }
        }

        public Vector2 origin { get; set; }

        public Color blendColor
        {
            get { return GetComponent<Material>().DiffuseColor; }

            set { GetComponent<Material>().DiffuseColor = value; }
        }

        public Rectangle? sourceRectangle { get; set; }

        public SpriteEffects spriteEffects { get; set; }

        public float depth { get; set; }

        public bool Visible { get; set; } = true;

        public int DrawOrder { get; set; }

        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;

        public void Draw(GameTime gameTime)
        {
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image, position.ToVector2(), sourceRectangle, blendColor, angle, origin, size, spriteEffects, depth);
        }

        public void LoadXxml(XmlElement node)
        {
            var sizeNode = node.SelectSingleNode("Size") as XmlElement;
            var sourceRectangleNode = node.SelectSingleNode("SourceRectangle") as XmlElement;
            var spriteEffectsNode = node.SelectSingleNode("SpriteEffects") as XmlElement;
            var depthNode = node.SelectSingleNode("Depth") as XmlElement;
            var angleNode = node.SelectSingleNode("Angle") as XmlElement;

            if (sizeNode != null)
                size = float.Parse(sizeNode.InnerText);
            if (sourceRectangleNode != null)
                sourceRectangle = Utill.RectFromXml(sourceRectangleNode);
            if (spriteEffectsNode != null)
                spriteEffects = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), spriteEffectsNode.InnerText);
            if (depthNode != null)
                depth = float.Parse(depthNode.InnerText);
            if (angleNode != null)
                angle = float.Parse(angleNode.InnerText);
        }
    }
}
