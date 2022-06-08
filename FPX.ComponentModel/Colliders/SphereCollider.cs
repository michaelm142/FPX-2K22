using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;

namespace FPX
{
    public class SphereCollider : Collider
    {

        public float radius { get; set; }

        public override Vector3 Psudosize
        {
            get { return Vector3.One * radius; }
        }

        public override void LoadXml(XmlElement node)
        {

            radius = Single.Parse(node.SelectSingleNode("Radius").InnerText);
        }

        public void SaveXml(XmlElement node)
        {
            var radiusNode = node.OwnerDocument.CreateElement("Radius");
            radiusNode.InnerText = radius.ToString();
            node.AppendChild(radiusNode);
        }

        public override bool Contains(Vector3 point)
        {

            if (Vector3.Distance(position, point) <= radius)
                return true;

            return false;
        }

        public override Vector3 ClosestPoint(Vector3 point)
        {

            var L = point - (transform.position + center);
            var length = MathHelper.Clamp(L.Length(), 0.0F, radius);
            L.Normalize();

            return position + L * length;
        }

        public override Vector3 ClosestPoint(Vector3 point, out Vector3 normal)
        {
            Vector3 p = ClosestPoint(point);
            normal = p - position;
            normal.Normalize();

            return p;
        }
    }
}
