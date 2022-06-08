using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FPX.ComponentModel;

namespace FPX
{
    public abstract class Collider : Component
    {
        public Vector3 center;

        public Vector3 Location
        {
            get { return transform.position + center; }
        }

        public abstract Vector3 Psudosize { get; }

        public abstract bool Contains(Vector3 point);

        public abstract Vector3 ClosestPoint(Vector3 point);

        public abstract Vector3 ClosestPoint(Vector3 point, out Vector3 normal);
    }
}
